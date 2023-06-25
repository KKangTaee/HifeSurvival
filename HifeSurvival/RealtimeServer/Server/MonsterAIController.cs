using Server.Helper;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Server
{
    public class MonsterAIController
    {
        private Stack<Entity> aggroStack = new Stack<Entity>();
        private Entity currentTarget = null;
        private MoveParam? lastMoveInfo = null;
        private long lastMovetime = 0;
        private long lastAttackTime = 0;
        private MonsterEntity monster = null;

        public MonsterAIController(MonsterEntity monster)
        {
            this.monster = monster;
        }

        public void AttackRoutine()
        {
            if (monster.IsDead())
            {
                return;
            }

            if (monster.OutOfSpawnRange())
            {
                ClearAggro();
                monster.MoveToRespawn();
                return;
            }

            if (currentTarget == null)
            {
                currentTarget = GetNextTarget();
                if (currentTarget == null)
                {
                    return;
                }
            }

            bool isTargetDead = false;
            if (currentTarget.IsDead())
            {
                isTargetDead = true;
            }
            else if (monster.CanAttack(currentTarget))
            {
                var elapseTime = HTimer.GetCurrentTimestamp() - lastAttackTime;
                if (elapseTime >= monster.stat.attackSpeed * 1000)
                {
                    var damageValue = currentTarget.GetDamagedValue(monster.GetAttackValue());

                    currentTarget.ReduceHP(damageValue);
                    currentTarget.OnDamaged(monster);
                    lastAttackTime = HTimer.GetCurrentTimestamp();

                    if (currentTarget.IsDead())
                    {
                        isTargetDead = true;
                    }

                    CS_Attack attackPacket = new CS_Attack()
                    {
                        toIsPlayer = true,
                        toId = currentTarget.targetId,
                        fromIsPlayer = false,
                        fromId = monster.targetId,
                        attackValue = damageValue,
                    };

                    monster.broadcaster.Broadcast(attackPacket);
                }
                else
                {
                    Logger.GetInstance().Debug($"Attack elapseTime {elapseTime}");
                }
            }
            else
            {
                monster.MoveToTarget(currentTarget.currentPos);
            }


            if (isTargetDead)
            {
                var nextTarget = GetNextTarget();
                if (nextTarget != null)
                {
                    monster.Attack(new AttackParam()
                    {
                        target = nextTarget,
                    });
                }
                else
                {
                    monster.MoveToRespawn();
                }
            }

            JobTimer.Instance.Push(() => {
                AttackRoutine();
            }, 250);
        }

        public Entity GetNextTarget()
        {
            if (ExistAggro())
            {
                currentTarget = PopBackAggroTarget();
                return currentTarget;
            }

            return null;
        }

        public void MoveRoutine()
        {
            if(lastMoveInfo == null)
            {
                return;
            }

            if(lastMovetime > lastMoveInfo.Value.timestamp)
            {
                return;
            }

            lastMovetime = lastMoveInfo.Value.timestamp;

            var currentPos = monster.currentPos;
            var targetPos = lastMoveInfo.Value.targetPos;
            var totalDist = monster.currentPos.DistanceTo(targetPos);

            float dirX = (targetPos.x - currentPos.x) / totalDist;
            float dirY = (targetPos.y - currentPos.y) / totalDist;
            float ratio = monster.stat.moveSpeed * 250 * 0.001f;

            monster.currentPos.x = currentPos.x + dirX * ratio;
            monster.currentPos.y = currentPos.y + dirY * ratio;

            if (monster.currentPos.IsSame(targetPos))
            {
                StopMove();
            }
            else
            {
                JobTimer.Instance.Push(() => {
                    MoveRoutine();
                }, 250);
            }
        }

        public void UpdateNextMove(MoveParam? moveParam)
        {
            lastMoveInfo = moveParam;
        }

        public void StopMove()
        {
            UpdateNextMove(null);
        }

        public void AddAggro(Entity target)
        {
            aggroStack.Push(target);
            currentTarget = target;
        }

        public void ClearAggro()
        {
            aggroStack.Clear();
            currentTarget = null;
        }

        public Entity PopBackAggroTarget()
        {
            return aggroStack.Pop();
        }

        public bool ExistAggro()
        {
            return aggroStack.Count > 0;
        }


        public void OnMonsterDead()
        {
            ClearAggro();
            lastMoveInfo = null;
        }
    }
}
