using System;
using System.Collections.Generic;
using System.Linq;
using ServerCore;

namespace Server
{
    public partial class MonsterEntity : Entity
    {
        public int monsterId;
        public int groupId;
        public int grade;
        public string rewardDatas;

        StateMachine<MonsterEntity> _stateMachine;

        public override bool IsPlayer => false;

        
        private MonsterAIController AIController { get; set; }
        private event Action<string, PVec3> dropItemDelegate;
        private MonsterGroup group;
        

        public MonsterEntity(MonsterGroup group, Action<string, PVec3> dropItem)
        {
            var smdic = new Dictionary<EStatus, IState<MonsterEntity, IStateParam>>();
            smdic[EStatus.IDLE] = new IdleState();
            smdic[EStatus.ATTACK] = new AttackState();
            smdic[EStatus.MOVE] = new MoveState();
            smdic[EStatus.USE_SKILL] = new UseSkillState();
            smdic[EStatus.DEAD] = new DeadState();

            _stateMachine = new StateMachine<MonsterEntity>(smdic);

            if(group != null)
            {
                this.group = group;
            }

            AIController = new MonsterAIController(this);
            dropItemDelegate += dropItem;
        }


        protected override void ChangeState<P>(EStatus inStatue, P inParam)
        {
            _stateMachine.OnChangeState(inStatue, this, inParam);
        }

        public override void OnDamaged(in Entity attacker)
        {
            Logger.GetInstance().Debug($"Monster OnDamaged !  id{targetId}, dead? {IsDead()}");
            if(IsDead())
            {
                Dead(new DeadParam()
                {
                    killerTarget = attacker,
                });

                return;
            }

            if (attacker.IsPlayer)
            {
                Attack(new AttackParam()
                {
                    target = attacker,
                });

                foreach (var monster in group.GetMonsterGroupIter())
                {
                    if(monster.Value.targetId == targetId)
                        continue;

                    if (monster.Value.AIController.ExistAggro())
                        continue;

                    monster.Value.Attack(new AttackParam()
                    {
                        target = attacker,
                    });
                }
            }
        }

        public void ExecuteAI()
        {
            AIController.StartAIRoutine();
        }

        //battle
        public bool CanAttack(in Entity target)
        {
            return currentPos.DistanceTo(target.currentPos) < stat.attackRange;
        }

        //battle
        public bool OutOfSpawnRange()
        {
            return currentPos.DistanceTo(spawnPos) > 10;
        }

        public void OnAttackSuccess(in Entity target, int damageValue)
        {
            CS_Attack attackPacket = new CS_Attack()
            {
                toIsPlayer = true,
                toId = target.targetId,
                fromIsPlayer = false,
                fromId = this.targetId,
                attackValue = damageValue,
            };

            broadcaster.Broadcast(attackPacket);
        }

        public bool IsGroupAllDead()
        {
            return group.IsAllDead();
        }

        public void StartRespawning()
        {
            group.SendRespawnGroup();
        }

        public void DropItem()
        {
            if (dropItemDelegate == null)
                return;

            dropItemDelegate(rewardDatas, currentPos);
            dropItemDelegate = null;
            return;
        }
    }
}