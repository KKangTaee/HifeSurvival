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

        private AIMode aiMode = AIMode.Free;

        public MonsterAIController(MonsterEntity monster)
        {
            this.monster = monster;
        }

        //NOTE: Timer 클래스에 등록하게 될 예정.
        public void StartAIRoutine()
        {
            if (monster.IsDead())
                return;

            if (aiMode == AIMode.Free)
            {
                if (SelectTarget())
                {
                    if (AttackRoutine())
                        ClearLastMove();
                    else
                        monster.MoveToTarget(CurrentTarget().currentPos);
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

        private bool AttackRoutine()
        {
            var currentTarget = CurrentTarget();
            bool isAttackable = BattleCalculator.CanAttackTarget(monster, currentTarget)
                && HTimer.GetCurrentTimestamp() - lastAttackTime >= monster.stat.attackSpeed * DEFINE.SEC_TO_MS;

            if (isAttackable)
            {
                var damagedVal = BattleCalculator.ComputeDamagedValue(monster.stat, currentTarget.stat);

                currentTarget.ReduceHP(damagedVal);
                currentTarget.OnDamaged(monster);
                lastAttackTime = HTimer.GetCurrentTimestamp();

                monster.OnAttackSuccess(currentTarget, damagedVal);
            }

            return isAttackable;
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
            float ratio = monster.stat.moveSpeed * DEFINE.AI_CHECK_MS * DEFINE.MS_TO_SEC;

            monster.currentPos.x = currentPos.x + normalizedVec.x * ratio;
            monster.currentPos.y = currentPos.y + normalizedVec.y * ratio;

            if (monster.currentPos.IsSame(targetPos))
            {
                Logger.GetInstance().Debug($"Arrived !! id : {monster.targetId}");

                if (aiMode == AIMode.ReturnToRespawnArea)
                {
                    aiMode = AIMode.Free;
                    ClearAggro();
                }

                ClearLastMove();
            }
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
            aiMode = AIMode.ReturnToRespawnArea;
            monster.MoveToRespawn();
            Logger.GetInstance().Debug("ReturnToRespawnArea");
        }

        public void UpdateAggro(Entity target)
        {
            Logger.GetInstance().Debug($"aggroid : {target.targetId}, self : {monster.targetId}");
            if (ExistAggro())
            {
                if (CurrentTarget().targetId == target.targetId)
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
                timestamp = HTimer.GetCurrentTimestamp()
            });
        }
    }
}
