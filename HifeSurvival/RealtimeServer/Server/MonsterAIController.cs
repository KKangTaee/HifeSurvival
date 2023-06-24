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
        private MonsterEntity monster;

        public MonsterAIController(MonsterEntity monster)
        {
            this.monster = monster;
        }

        public bool ExecuteAttack(Entity target, out int damageValue)
        {
            damageValue = 0;
            if (monster.IsDead())
                return false;

            if (monster.OutOfSpawnRange())
            {
                Logger.GetInstance().Warn("out of range!!");
                ClearAggro();
                monster.MoveToRespawn();
                return false;
            }

            bool isTargetDead = false;
            if (target.IsDead())
            {
                isTargetDead = true;
            }
            else if (monster.CanAttack(target))
            {
                damageValue = target.GetDamagedValue(monster.GetAttackValue());

                target.ReduceHP(damageValue);
                target.OnDamaged(monster);

                if (target.IsDead())
                {
                    isTargetDead = true;
                }
                else
                {
                    return true;
                }

            }
            else
            {
                monster.MoveToTarget(target.currentPos);
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

            return false;
        }

        public Entity GetNextTarget()
        {
            if (ExistAggro())
            {
                return PopBackAggroTarget();
            }

            return null;
        }

        public void AddAggro(Entity target)
        {
            aggroStack.Push(target);
        }

        public void ClearAggro()
        {
            aggroStack.Clear();
        }

        public Entity PopBackAggroTarget()
        {
            return aggroStack.Pop();
        }

        public bool ExistAggro()
        {
            return aggroStack.Count > 0;
        }

    }
}
