using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Server
{
    public class MonsterAIController
    {
        private MonsterEntity _monster = null;

        private List<Entity> _aggroStack = new List<Entity>();
        private long _lastAttackTime = 0;

        private MoveParam? _lastMoveInfo = null;
        private long _lastMovetime = 0;

        private EAIMode _aiMode = EAIMode.FREE;

        public MonsterAIController(MonsterEntity monster)
        {
            _monster = monster;
        }

        //NOTE: Timer 클래스에 등록하게 될 예정.
        public void StartAIRoutine()
        {
            if (_monster.IsDead())
                return;

            if (_aiMode == EAIMode.FREE)
            {
                if (SelectTarget())
                {
                    if(CheckAttackRange())
                    {
                        ClearLastMove(); 
                        AttackRoutine();
                    }
                    else
                    {
                        _monster.MoveToTarget(CurrentTarget().currentPos);
                    }
                }
                else
                {
                    if (_monster.currentPos.IsSame(_monster.spawnPos) == false)
                        ReturnToRespawnArea();
                }
            }

            MoveRoutine();

            JobTimer.Instance.Push(() =>
            {
                StartAIRoutine();
            }, DEFINE.AI_CHECK_MS);
        }

        private void AttackRoutine()
        {
            var currentTarget = CurrentTarget();

            if (ServerTime.GetCurrentTimestamp() - _lastAttackTime >= _monster.stat.AttackSpeed * DEFINE.SEC_TO_MS)
            {
                var damagedVal = BattleCalculator.ComputeDamagedValue(_monster.stat, currentTarget.stat);

                currentTarget.ReduceHP(damagedVal);
                currentTarget.OnDamaged(_monster);
                _lastAttackTime = ServerTime.GetCurrentTimestamp();

                _monster.OnAttackSuccess(currentTarget, damagedVal);
            }

            return;
        }

        private void MoveRoutine()
        {
            if (_lastMoveInfo == null)
                return;

            if (_lastMovetime > _lastMoveInfo.Value.timestamp)
                return;

            _lastMovetime = _lastMoveInfo.Value.timestamp;

            var currentPos = _monster.currentPos;
            var targetPos = _lastMoveInfo.Value.targetPos;

            var normalizedVec = currentPos.NormalizeToTargetPVec3(targetPos);
            float ratio = _monster.stat.MoveSpeed * DEFINE.AI_CHECK_MS * DEFINE.MS_TO_SEC;

            _monster.currentPos.x = currentPos.x + normalizedVec.x * ratio;
            _monster.currentPos.y = currentPos.y + normalizedVec.y * ratio;

            if (_monster.currentPos.IsSame(targetPos))
            {
                Logger.GetInstance().Debug($"Arrived !! id : {_monster.id}");

                if (_aiMode == EAIMode.RETURN_TO_RESPAWN_AREA)
                {
                    _aiMode = EAIMode.FREE;
                    ClearAggro();
                }

                ClearLastMove();
            }
        }

        private bool CheckAttackRange()
        {
            return BattleCalculator.CanAttackTarget(_monster, CurrentTarget());
        }

        private bool SelectTarget()
        {
            if (BattleCalculator.IsOutOfSpawnArea(_monster))
            {
                ClearAggro();
                return false;
            }

            var currentTarget = CurrentTarget();
            if (currentTarget != null && !currentTarget.IsDead())
            {
                return true;
            }
            else
            {
                while (ExistAggro())
                {
                    currentTarget = GetNextTarget();
                    if (currentTarget != null && !currentTarget.IsDead())
                        return true;
                }
            }

            return false;
        }

        public void UpdateNextMove(MoveParam? moveParam)
        {
            _lastMoveInfo = moveParam;
        }

        public void ReturnToRespawnArea()
        {
            _aiMode = EAIMode.RETURN_TO_RESPAWN_AREA;
            _monster.MoveToRespawn();
            Logger.GetInstance().Debug("ReturnToRespawnArea");
        }

        public void UpdateAggro(Entity target)
        {
            Logger.GetInstance().Debug($"aggroid : {target.id}, self : {_monster.id}");
            if (ExistAggro())
            {
                if (CurrentTarget().id == target.id)
                    return;

                _aggroStack.Remove(target);
            }

            _aggroStack.Add(target);
        }

        public bool ExistAggro()
        {
            return _aggroStack.Count > 0;
        }

        private Entity CurrentTarget()
        {
            if(ExistAggro())
                return _aggroStack[_aggroStack.Count - 1];

            return null;
        }

        private Entity GetNextTarget()
        {
            if (ExistAggro())
            {
                _aggroStack.RemoveAt(_aggroStack.Count - 1);

                if(ExistAggro())
                    return _aggroStack[_aggroStack.Count - 1];
            }

            return null;
        }

        public void Clear()
        {
            ClearAggro();
            ClearLastMove();
        }

        private void ClearAggro()
        {
            _aggroStack.Clear();
        }

        public void ClearLastMove()
        {
            _lastMoveInfo = null;

            _monster.MoveStop(new IdleParam()
            {
                currentPos = _monster.currentPos,
                timestamp = ServerTime.GetCurrentTimestamp()
            });
        }
    }
}
