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
        private MonsterGroup _group;
        private WorldMap _worldMap;

        public MonsterEntity(GameRoom room, MonsterGroup group, WorldMap worldMap)
            : base(room)
        {
            var smDict = new Dictionary<EEntityStatus, IState<MonsterEntity, IStateParam>>();
            smDict[EEntityStatus.IDLE] = new IdleState();
            smDict[EEntityStatus.ATTACK] = new AttackState();
            smDict[EEntityStatus.MOVE] = new MoveState();
            smDict[EEntityStatus.USESKILL] = new UseSkillState();
            smDict[EEntityStatus.DEAD] = new DeadState();

            this._group = group;

            _stateMachine = new StateMachine<MonsterEntity>(smDict);
            AIController = new MonsterAIController(this);

            _worldMap = worldMap;
        }


        protected override void ChangeState<P>(EEntityStatus status, P param)
        {
            _stateMachine.OnChangeState(status, this, param);
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

        public override PStat GetDefaultPStat()
        {
            return this.defaultStat.ConvertToPStat();
        }

        public override PStat GetAdditionalPStat()
        {
            //임시
            return (new EntityStat()).ConvertToPStat();
        }

        public override void OnDamaged(in Entity attacker)
        {
            if (IsDead())
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

                    _group.OnAttack(id, playerAttacker);
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

            Room.Broadcast(attackPacket);
        }


        public void ExecuteAI()
        {
            AIController.StartAIRoutine();
        }

        public bool ExistAggro() => AIController.ExistAggro();

        public void DropItem()
        {
            if (_worldMap == null)
            {
                return;
            }

            var broadcast = _worldMap.DropItem(rewardDatas, currentPos);
            if (broadcast == null)
            {
                Logger.GetInstance().Warn("Reward Drop Failed");
            }
            else
            {
                Logger.GetInstance().Debug($"Reward Drop worldid : {broadcast.worldId}, pos : {broadcast.pos.Print()}");
                Room.Broadcast(broadcast);
            }
            return;
        }


        public bool IsGroupAllDead()
        {
            return _group.IsAllDead();
        }

        public void StartRespawning()
        {
            _group.SendRespawnGroup(); 
        }
    }
}