﻿using System.Collections.Generic;

namespace Server
{
    public partial class PlayerEntity : Entity
    {
        public string userId;
        public string userName;
        
        public int  heroKey;
        public EClientStatus clientStatus;

        public EntityStat itemStat = new EntityStat();
        public EntityStat upgradeStat = new EntityStat();

        public PlayerInventory Inventory { get; private set; }

        StateMachine<PlayerEntity> _stateMachine;

        public CheatExecuter CheatExecuter { get ; private set; }

        public PlayerEntity(GameRoom room, HeroData data)
            : base(room)
        {
            var smDict = new Dictionary<EEntityStatus, IState<PlayerEntity, IStateParam>>();
            smDict[EEntityStatus.IDLE] = new IdleState();
            smDict[EEntityStatus.ATTACK] = new AttackState();
            smDict[EEntityStatus.MOVE] = new MoveState();
            smDict[EEntityStatus.USESKILL] = new UseSkillState();
            smDict[EEntityStatus.DEAD] = new DeadState();

            _stateMachine = new StateMachine<PlayerEntity>(smDict);
            Inventory = new PlayerInventory(this);
            DefaultStat = new EntityStat(data);
            CheatExecuter = new CheatExecuter(this);
        }

        public void SelectReady()
        {
            clientStatus = EClientStatus.SELECT_READY;
        }

        public void PlayReady()
        {
            clientStatus = EClientStatus.PLAY_READY;
        }

        protected override void ChangeState<P>(EEntityStatus status, P param)
        {
            _stateMachine.OnChangeState(status, this, param);
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
            Stat += upgradeStat;
            Stat += itemStat;

            OnStatChange();     //변화 감지가 필요함. 일단 생략. 
        }

        public override PStat GetDefaultPStat()
        {
            return this.DefaultStat.ConvertToPStat();
        }

        public override PStat GetAdditionalPStat()
        {
            return (this.upgradeStat + this.itemStat).ConvertToPStat();
        }

        public void UpdateItemStat()
        {
            itemStat = Inventory.TotalItemStat();
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

            Room.Send(id, updateInvenItem);
        }

        public int EquipItem(in ItemData item)
        {
            var invenItem = Inventory.EquipItem(item);
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
            JobTimer.Instance.Push(() =>
            {
                Stat.AddCurrHp(Stat.MaxHp);
                Idle();

                S_Respawn respawn = new S_Respawn()
                {
                    id = id,
                    stat = Stat.ConvertToPStat(),
                };

                Room.Broadcast(respawn);
            }, DEFINE.PLAYER_RESPAWN_MS);
        }
    }
}
