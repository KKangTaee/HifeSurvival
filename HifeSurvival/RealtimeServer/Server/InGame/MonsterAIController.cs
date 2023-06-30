﻿using System.Collections.Generic;

namespace Server
{
    public class MonsterAIController
    {
        private MonsterEntity _monster;

        private List<Entity> _aggroList = new List<Entity>();
        private long _lastAttackTime;
        private long _lastAnimTime;

        private MoveParam? _lastMoveInfo;
        private long _lastMovetime;

        private EAIMode _aiMode;

        public MonsterAIController(MonsterEntity monster)
        {
            _monster = monster;
        }

        //NOTE: Timer 클래스에 등록하게 될 예정.
        public void StartAIRoutine()
        {
            if (_monster.IsDead())
            {
                return;
            }

            if (_aiMode == EAIMode.FREE)
            {
                if (SelectTarget())
                {
                    if (BattleCalculator.CanAttackDistance(_monster, CurrentTarget()))
                    {
                        ClearLastMove();
                        AttackRoutine();
                    }
                    else
                    {
                        MoveToCurrentTarget();
                    }
                }
                else
                {
                    if (_monster.currentPos.IsDifferent(_monster.spawnPos))
                    {
                        ReturnToRespawnArea();
                    }
                }
            }

            MoveRoutine();

            JobTimer.Instance.Push(() =>
            {
                StartAIRoutine();
            }, DEFINE.AI_CHECK_MS);
        }

        public void UpdateAggro(Entity target)
        {
            if (ExistAggro())
            {
                if (CurrentTarget().id == target.id)
                {
                    return;
                }

                _aggroList.Remove(target);
            }

            Logger.GetInstance().Debug($" self : {_monster.id}, aggroid : {target.id}");
            _aggroList.Add(target);
        }

        public void UpdateNextMove(MoveParam? moveParam)
        {
            _lastMoveInfo = moveParam;
        }

        public bool ExistAggro()
        {
            return _aggroList.Count > 0;
        }

        public void Clear()
        {
            ClearAggro();
            ClearLastMove();
        }

        private void AttackRoutine()
        {
            if (!CanAttack())
            {
                return;
            }

            var currentTarget = CurrentTarget();
            Logger.GetInstance().Debug($"playerTarget : {currentTarget.id}, Pos : {currentTarget.currentPos.Print()}");
            Logger.GetInstance().Debug($"monster : {_monster.id}, Pos : {_monster.currentPos.Print()}");
            var damagedVal = BattleCalculator.ComputeDamagedValue(_monster.stat, currentTarget.stat);

            currentTarget.ReduceHP(damagedVal);
            currentTarget.OnDamaged(_monster);

            _lastAttackTime = ServerTime.GetCurrentTimestamp();
            _lastAnimTime = ServerTime.GetCurrentTimestamp();

            _monster.OnAttackSuccess(currentTarget, damagedVal);
        }

        private void MoveRoutine()
        {
            if (_lastMoveInfo == null)
            {
                return;
            }

            if (_lastMoveInfo.Value.timestamp < _lastMovetime)
            {
                return;
            }

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

        private bool SelectTarget()
        {
            if (BattleCalculator.IsOutOfSpawnArea(_monster))
            {
                ClearAggro();
                return false;
            }

            var currentTarget = CurrentTarget();
            if (IsValidTarget(currentTarget))
            {
                return true;
            }

            while (ExistAggro())
            {
                currentTarget = GetNextTarget();
                if (IsValidTarget(currentTarget))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsValidTarget(in Entity target)
        {
            return target != null && !target.IsDead();
        }

        private void MoveToCurrentTarget()
        {
            if (!CanMove())
            {
                return;
            }

            var currentTarget = CurrentTarget();
            if (IsValidTarget(currentTarget))
            {
                _monster.MoveToTarget(currentTarget.currentPos);
            }
        }

        private void ReturnToRespawnArea()
        {
            if (!CanMove())
            {
                return;
            }

            _aiMode = EAIMode.RETURN_TO_RESPAWN_AREA;
            _monster.MoveToRespawn();
            Logger.GetInstance().Debug("ReturnToRespawnArea");
        }

        private bool CanMove()
        {
            return ServerTime.GetCurrentTimestamp() - _lastAnimTime > DEFINE.MONSTER_ATTACK_ANIM_TIME;
        }

        private bool CanAttack()
        {
            return ServerTime.GetCurrentTimestamp() - _lastAttackTime > _monster.stat.AttackSpeed * DEFINE.SEC_TO_MS;
        }

        private Entity CurrentTarget()
        {
            if (ExistAggro())
            {
                return _aggroList[_aggroList.Count - 1];
            }

            return null;
        }

        private Entity GetNextTarget()
        {
            if (ExistAggro())
            {
                _aggroList.RemoveAt(_aggroList.Count - 1);

                if (ExistAggro())
                {
                    return _aggroList[_aggroList.Count - 1];
                }
            }

            return null;
        }

        private void ClearAggro()
        {
            _aggroList.Clear();
        }

        private void ClearLastMove()
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
