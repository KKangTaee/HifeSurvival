using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Server
{
    public class MonsterAIController
    {
        private const int AI_CHECK_MS = 50;

        private MonsterEntity monster = null;

        private List<Entity> aggroStack = new List<Entity>();
        private long lastAttackTime = 0;

        private MoveParam? lastMoveInfo = null;
        private long lastMovetime = 0;
        private bool isReturningToRespawnArea = false;

        public MonsterAIController(MonsterEntity monster)
        {
            this.monster = monster;
        }

        //NOTE: Timer 클래스에 등록하게 될 예정.
        public void StartAIRoutine()
        {
            if (!monster.IsDead())
            {
                if(!isReturningToRespawnArea)
                {
                    if (SelectTarget())
                    {
                        if (AttackRoutine())
                        {
                            ClearLastMove();
                        }
                        else
                        {
                            monster.MoveToTarget(CurrentTarget().currentPos);
                        }
                    }
                    else
                    {
                        if (monster.currentPos.IsSame(monster.spawnPos) == false)
                        {
                            ReturnToRespawnArea();
                        }
                    }
                }

                MoveRoutine();
            }


            JobTimer.Instance.Push(() =>
            {
                StartAIRoutine();
            }, AI_CHECK_MS);
        }

        public void ReturnToRespawnArea()
        {
            isReturningToRespawnArea = true;
            monster.MoveToRespawn();
            Logger.GetInstance().Debug("ReturnToRespawnArea");
        }

        private bool SelectTarget()
        {
            if (monster.OutOfSpawnRange())
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

        public void UpdateAggro(Entity target)
        {
            Logger.GetInstance().Debug($"aggroid : {target.targetId}, self : {monster.targetId}");
            aggroStack.Add(target);
        }

        public void UpdateNextMove(MoveParam? moveParam)
        {
            lastMoveInfo = moveParam;
        }

        private bool AttackRoutine()
        {
            var currentTarget = CurrentTarget();
            var bAttackSuccess = monster.CanAttack(currentTarget)
                && HTimer.GetCurrentTimestamp() - lastAttackTime >= monster.stat.attackSpeed * 1000;

            if (bAttackSuccess)
            {
                var damageValue = currentTarget.GetDamagedValue(monster.GetAttackValue());

                currentTarget.ReduceHP(damageValue);
                currentTarget.OnDamaged(monster);
                lastAttackTime = HTimer.GetCurrentTimestamp();

                monster.OnAttackSuccess(currentTarget, damageValue);
            }

            return bAttackSuccess;
        }


        private void MoveRoutine()
        {
            if (lastMoveInfo == null)
            {
                return;
            }

            if (lastMovetime > lastMoveInfo.Value.timestamp)
            {
                return;
            }

            lastMovetime = lastMoveInfo.Value.timestamp;

            var currentPos = monster.currentPos;
            var targetPos = lastMoveInfo.Value.targetPos;

            var normalizedVec = targetPos.SubtractPVec3(currentPos).NormalizePVec3();
            float ratio = monster.stat.moveSpeed * AI_CHECK_MS * 0.001f;

            monster.currentPos.x = currentPos.x + normalizedVec.x * ratio;
            monster.currentPos.y = currentPos.y + normalizedVec.y * ratio;

            if (monster.currentPos.IsSame(targetPos))
            {
                Logger.GetInstance().Debug($"Arrived !! id : {monster.targetId}");

                if (isReturningToRespawnArea)
                {
                    isReturningToRespawnArea = false;
                    ClearAggro();
                }

                ClearLastMove();
            }
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

        private void ClearAggro()
        {
            aggroStack.Clear();
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
    }
}
