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

        private MonsterAIController AIController { get; set; }
        private event Action<string, PVec3> dropItemDelegate;
        private MonsterGroup group;

        public MonsterEntity(MonsterGroup group, Action<string, PVec3> dropItem)
        {
            var smdic = new Dictionary<EEntityStatus, IState<MonsterEntity, IStateParam>>();
            smdic[EEntityStatus.IDLE] = new IdleState();
            smdic[EEntityStatus.ATTACK] = new AttackState();
            smdic[EEntityStatus.MOVE] = new MoveState();
            smdic[EEntityStatus.USESKILL] = new UseSkillState();
            smdic[EEntityStatus.DEAD] = new DeadState();

            this.group = group;

            _stateMachine = new StateMachine<MonsterEntity>(smdic);
            AIController = new MonsterAIController(this);
            
            dropItemDelegate += dropItem;
        }


        protected override void ChangeState<P>(EEntityStatus inStatue, P inParam)
        {
            _stateMachine.OnChangeState(inStatue, this, inParam);
        }

        public override void UpdateStat()
        {
            bool bChanged = true;   //변화 감지가 필요함. 일단 생략. 

            stat = new EntityStat();
            stat += defaultStat;

            if (bChanged)
            {
                OnStatChange();
            }
        }

        public override void GetStat(out EntityStat defaultStat, out EntityStat additionalStat)
        {
            defaultStat = this.defaultStat;
            additionalStat = new EntityStat();
        }

        public override void OnDamaged(in Entity attacker)
        {
            if(IsDead())
            {
                Dead(new DeadParam()
                {
                    killerTarget = attacker,
                });
            }
            else
            {
                if (attacker is PlayerEntity playerAttacker)
                {
                    Attack(new AttackParam()
                    {
                        target = playerAttacker,
                    });

                    group.OnAttack(id, playerAttacker);
                }
            }
        }

        public void OnAttackSuccess(in Entity target, int damageValue)
        {
            CS_Attack attackPacket = new CS_Attack()
            {
                id = id,
                targetId = target.id,
                attackValue = damageValue,
            };

            broadcaster.Broadcast(attackPacket);
        }


        public void ExecuteAI()
        {
            AIController.StartAIRoutine();
        }

        public bool ExistAggro() => AIController.ExistAggro();

        public void DropItem()
        {
            if (dropItemDelegate == null)
                return;

            dropItemDelegate(rewardDatas, currentPos);
            dropItemDelegate = null;
            return;
        }


        public bool IsGroupAllDead() => group.IsAllDead();

        public void StartRespawning() => group.SendRespawnGroup();
    }
}