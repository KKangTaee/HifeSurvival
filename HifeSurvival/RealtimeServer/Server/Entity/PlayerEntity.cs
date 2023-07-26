using System.Collections.Generic;

namespace Server
{
    public partial class PlayerEntity : Entity
    {
        private string _userId;
        private string _userName;
        private int _heroKey;

        private StateMachine<PlayerEntity> _stateMachine;

        public EClientStatus ClientStatus;
        public EntityStat ItemStat { get; private set; } = new EntityStat();
        public EntityStat UpgradedStat { get; private set; } = new EntityStat();
        public PlayerInventory Inventory { get; private set; }
        public CheatExecuter CheatExecuter { get; private set; }

        public PlayerEntity(GameRoom room, int playerId, string userId, string userName, int heroKey)
            : base(room)
        {
            ID = playerId;

            _userId = userId;
            _userName = userName;
            _heroKey = heroKey;

            Inventory = new PlayerInventory(this);
            CheatExecuter = new CheatExecuter(room, this);
        }

        public void InitGamePlayer(in PVec3 startPos)
        {
            if (!GameData.Instance.HerosDict.TryGetValue(_heroKey, out var heroData))
            {
                Logger.Instance.Error($"Spawn Hero Key is Invalid - key {_heroKey}");
                return;
            }

            currentPos = startPos;
            targetPos = startPos;
            spawnPos = startPos;

            var smDict = new Dictionary<EEntityStatus, IState<PlayerEntity, IStateParam>>();
            smDict[EEntityStatus.IDLE] = new IdleState();
            smDict[EEntityStatus.ATTACK] = new AttackState();
            smDict[EEntityStatus.MOVE] = new MoveState();
            smDict[EEntityStatus.USESKILL] = new UseSkillState();
            smDict[EEntityStatus.DEAD] = new DeadState();
            _stateMachine = new StateMachine<PlayerEntity>(smDict);

            DefaultStat = new EntityStat(heroData);
        }

        public void TerminateGamePlayer()
        {
            _stateMachine = null;
        }

        public void ChangeHeroKey(int heroKey)
        {
            if (ClientStatus != EClientStatus.ENTERED_ROOM)
            {
                return;
            }

            _heroKey = heroKey;
        }

        public S_JoinToGame.JoinPlayer MakeJoinPlayer()
        {
            return new S_JoinToGame.JoinPlayer()
            {
                userId = _userId,
                userName = _userName,
                id = ID,
                heroKey = _heroKey,
            };
        }

        public PlayerSpawn MakePlayerSpawn()
        {
            return new PlayerSpawn()
            {
                id = ID,
                herosKey = _heroKey,
                pos = spawnPos
            };
        }

        protected override void ChangeState<P>(EEntityStatus status, P param)
        {
            _stateMachine?.OnChangeState(status, this, param);
        }


        public override void OnDamaged(in Entity attacker)
        {
            if (IsDead())
            {
                Dead(new DeadParam()
                {
                    killerTarget = attacker,
                });

                return;
            }
        }

        public override void UpdateStat()
        {
            UpdateItemStat();

            Stat = new EntityStat();
            Stat += DefaultStat;
            Stat += UpgradedStat;
            Stat += ItemStat;

            OnStatChange();     //변화 감지가 필요함. 일단 생략. 
        }

        public override PStat GetDefaultPStat()
        {
            return this.DefaultStat.ConvertToPStat();
        }

        public override PStat GetAdditionalPStat()
        {
            return (this.UpgradedStat + this.ItemStat).ConvertToPStat();
        }

        public void UpdateItemStat()
        {
            ItemStat = Inventory?.TotalItemStat();
        }

        public void UpdateItem(InvenItem updateItem)
        {
            UpdateStat();

            var updateInvenItem = new UpdateInvenItem()
            {
                invenItem = new PInvenItem()
                {
                    slot = updateItem.Slot,
                    itemKey = updateItem.ItemKey,
                    itemLevel = updateItem.Level,
                    currentStack = updateItem.CurrentStack,
                    maxStack = updateItem.MaxStack,
                    str = updateItem.Stat.Str,
                    def = updateItem.Stat.Def,
                    hp = updateItem.Stat.MaxHp,
                }
            };

            Room.Send(ID, updateInvenItem);
        }

        public int EquipItem(in ItemData item)
        {
            var invenItem = Inventory?.EquipItem(item);
            if (invenItem == null)
            {
                Logger.Instance.Warn($"Equip Failed! ");
                return -1;
            }

            UpdateItem(invenItem);
            return invenItem.Slot;
        }

        public void RegistRespawnTimer()
        {
            //중복해서 들어오는 것을 막아야함. 
            //TODO : Timer 클래스로 모두 빼야함. (취소 가능 기능 필요)
            Room.Push(() =>
            {
                Stat.AddCurrHp(Stat.MaxHp);
                Idle();

                S_Respawn respawn = new S_Respawn()
                {
                    id = ID,
                    stat = Stat.ConvertToPStat(),
                };

                Room.Broadcast(respawn);
            }, DEFINE.PLAYER_RESPAWN_SEC);
        }
    }
}
