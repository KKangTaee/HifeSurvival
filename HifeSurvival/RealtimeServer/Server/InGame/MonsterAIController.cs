using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Server
{
    public class MonsterAIController
    {
        private MonsterEntity monster = null;

        private List<Entity> aggroStack = new List<Entity>();
        private long lastAttackTime = 0;

        private MoveParam? lastMoveInfo = null;
        private long lastMovetime = 0;

        private EAIMode aiMode = EAIMode.FREE;

        public MonsterAIController(MonsterEntity monster)
        {
            this.monster = monster;
        }

        //NOTE: Timer 클래스에 등록하게 될 예정.
        public void StartAIRoutine()
        {
            if (monster.IsDead())
                return;

            if (aiMode == EAIMode.FREE)
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
                        monster.MoveToTarget(CurrentTarget().currentPos);
                    }
                }
                else
                {
                    if (monster.currentPos.IsSame(monster.spawnPos) == false)
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

            if (ServerTime.GetCurrentTimestamp() - lastAttackTime >= monster.stat.AttackSpeed * DEFINE.SEC_TO_MS)
            {
                var damagedVal = BattleCalculator.ComputeDamagedValue(monster.stat, currentTarget.stat);

                currentTarget.ReduceHP(damagedVal);
                currentTarget.OnDamaged(monster);
                lastAttackTime = ServerTime.GetCurrentTimestamp();

                monster.OnAttackSuccess(currentTarget, damagedVal);
            }

            return;
        }

        private void MoveRoutine()
        {
            if (lastMoveInfo == null)
                return;

            if (lastMovetime > lastMoveInfo.Value.timestamp)
                return;

            lastMovetime = lastMoveInfo.Value.timestamp;

            var currentPos = monster.currentPos;
            var targetPos = lastMoveInfo.Value.targetPos;

            var normalizedVec = currentPos.NormalizeToTargetPVec3(targetPos);
            float ratio = monster.stat.MoveSpeed * DEFINE.AI_CHECK_MS * DEFINE.MS_TO_SEC;

            monster.currentPos.x = currentPos.x + normalizedVec.x * ratio;
            monster.currentPos.y = currentPos.y + normalizedVec.y * ratio;

            if (monster.currentPos.IsSame(targetPos))
            {
                Logger.GetInstance().Debug($"Arrived !! id : {monster.id}");

                if (aiMode == EAIMode.RETURN_TO_RESPAWN_AREA)
                {
                    aiMode = EAIMode.FREE;
                    ClearAggro();
                }

                ClearLastMove();
            }
        }

        private bool CheckAttackRange()
        {
            return BattleCalculator.CanAttackTarget(monster, CurrentTarget());
        }

        private bool SelectTarget()
        {
            if (BattleCalculator.IsOutOfSpawnArea(monster))
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
            lastMoveInfo = moveParam;
        }

        public void ReturnToRespawnArea()
        {
            aiMode = EAIMode.RETURN_TO_RESPAWN_AREA;
            monster.MoveToRespawn();
            Logger.GetInstance().Debug("ReturnToRespawnArea");
        }

        public void UpdateAggro(Entity target)
        {
            Logger.GetInstance().Debug($"aggroid : {target.id}, self : {monster.id}");
            if (ExistAggro())
            {
                if (CurrentTarget().id == target.id)
                    return;

                aggroStack.Remove(target);
            }

            aggroStack.Add(target);
        }

        public bool ExistAggro()
        {
            return aggroStack.Count > 0;
        }

        private Entity CurrentTarget()
        {
            if(ExistAggro())
                return aggroStack[aggroStack.Count - 1];

            return null;
        }

        private Entity GetNextTarget()
        {
            if (ExistAggro())
            {
                aggroStack.RemoveAt(aggroStack.Count - 1);

                if(ExistAggro())
                    return aggroStack[aggroStack.Count - 1];
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
            aggroStack.Clear();
        }

        public void ClearLastMove()
        {
            lastMoveInfo = null;

            monster.MoveStop(new IdleParam()
            {
                currentPos = monster.currentPos,
                timestamp = ServerTime.GetCurrentTimestamp()
            });
        }
    }
}
