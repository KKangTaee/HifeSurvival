using System;
using System.Collections.Generic;
using System.Text;

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

        public int  gold;
        
        public PlayerInventory inven = new PlayerInventory();

        StateMachine<PlayerEntity> _stateMachine;

        public PlayerEntity(GameRoom room)
            : base(room)
        {
            var smDict = new Dictionary<EEntityStatus, IState<PlayerEntity, IStateParam>>();
            smDict[EEntityStatus.IDLE] = new IdleState();
            smDict[EEntityStatus.ATTACK] = new AttackState();
            smDict[EEntityStatus.MOVE] = new MoveState();
            smDict[EEntityStatus.USESKILL] = new UseSkillState();
            smDict[EEntityStatus.DEAD] = new DeadState();

            _stateMachine = new StateMachine<PlayerEntity>(smDict);
        }

        public void SelectReady() => clientStatus = EClientStatus.SELECT_READY;
        public void PlayReady() => clientStatus = EClientStatus.PLAY_READY;


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
            bool bChanged = true;   //변화 감지가 필요함. 일단 생략. 

            UpdateItem();

            stat = new EntityStat();
            stat += defaultStat;
            stat += upgradeStat;
            stat += itemStat;

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
            return (this.upgradeStat + this.itemStat).ConvertToPStat();
        }

        public void UpdateItem()
        {
            itemStat = inven.TotalItemStat();
        }

        public int EquipItem(in PItem item)
        {
            int slot = inven.EquipItem(item);
            if (slot >= 0)
            {
                UpdateItem();
            }
            else
            {
                Logger.GetInstance().Warn($"Equip Failed! ");
            }

            return slot;
        }

        public void RegistRespawnTimer()
        {
            //중복해서 들어오는 것을 막아야함. 
            //TODO : Timer 클래스로 모두 빼야함. (취소 가능 기능 필요)
            JobTimer.Instance.Push(() =>
            {
                stat.AddCurrHp(stat.MaxHp);
                Idle();

                S_Respawn respawn = new S_Respawn()
                {
                    id = id,
                    stat = stat.ConvertToPStat(),
                };

                Room.Broadcast(respawn);
            }, DEFINE.PLAYER_RESPAWN_MS);
        }
    }
}
