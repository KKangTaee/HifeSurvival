using System.Collections.Generic;

namespace Server
{
    public partial class MonsterEntity : Entity
    {
        private int _monsterKey;
        private string _rewardIds;
        private MonsterGroup _group;
        private StateMachine<MonsterEntity> _stateMachine;

        public MonsterAIController AIController { get; private set; }
        public EMonsterGrade Grade { get; private set; }


        public MonsterEntity(GameRoom room, int mId, MonsterGroup group, MonsterData data, in PVec3 startPos)
            : base(room)
        {
            ID = mId;

            currentPos = startPos;
            targetPos = startPos;
            spawnPos = startPos;

            _monsterKey = data.key;
            Grade = (EMonsterGrade)data.grade;
            _rewardIds = data.rewardIds;
            _group = group;

            var smDict = new Dictionary<EEntityStatus, IState<MonsterEntity, IStateParam>>();
            smDict[EEntityStatus.IDLE] = new IdleState();
            smDict[EEntityStatus.ATTACK] = new AttackState();
            smDict[EEntityStatus.MOVE] = new MoveState();
            smDict[EEntityStatus.USESKILL] = new UseSkillState();
            smDict[EEntityStatus.DEAD] = new DeadState();
            _stateMachine = new StateMachine<MonsterEntity>(smDict);

            AIController = new MonsterAIController(this);
            DefaultStat = new EntityStat(data);
        }

        public MonsterSpawn MakeSpawnData()
        {
            return new MonsterSpawn()
            {
                id = ID,
                monstersKey = _monsterKey,
                groupId = _group.GroupId,
                grade = (int)Grade,
                pos = spawnPos,
            };
        }


        protected override void ChangeState<P>(EEntityStatus status, P param)
        {
            _stateMachine?.OnChangeState(status, this, param);
        }

        public override void UpdateStat()
        {
            Stat = new EntityStat();
            Stat += DefaultStat;

            OnStatChange();     //변화 감지가 필요함. 일단 생략. 
        }

        public override PStat GetDefaultPStat()
        {
            return this.DefaultStat?.ConvertToPStat() ?? new PStat();
        }

        public override PStat GetAdditionalPStat()
        {
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

                    _group.OnAttack(ID, playerAttacker);
                }
            }
        }

        public void OnAttackSuccess(in Entity target, int damageValue)
        {
            var attackPacket = new CS_Attack()
            {
                id = ID,
                targetId = target.ID,
                attackValue = damageValue,
            };

            Room.Broadcast(attackPacket);
        }

        public void DropItem()
        {
            var broadcast = Room.Mode.DropItem(_rewardIds, currentPos);
            if (broadcast == null)
            {
                Logger.Instance.Warn("Reward Drop Failed");
            }
            else
            {
                Logger.Instance.Debug($"Reward Drop worldid : {broadcast.worldId}, pos : {broadcast.pos.Print()}");
                Room.Broadcast(broadcast);
            }

            return;
        }
        public void ExecuteAI()
        {
            AIController.StartAIRoutine();
        }

        public bool ExistAggro()
        {
            return AIController.ExistAggro();
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