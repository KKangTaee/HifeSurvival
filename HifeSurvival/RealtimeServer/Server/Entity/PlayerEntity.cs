using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    public partial class PlayerEntity : Entity
    {
        public string userId;
        public string userName;
        
        public int  heroId;
        public bool isReady;

        public int  gold;
        public PlayerItem[] itemSlot = new PlayerItem[4];

        public override bool IsPlayer => true;

        StateMachine<PlayerEntity> _stateMachine;

        public PlayerEntity()
        {
            var smdic = new Dictionary<EntityStatus, IState<PlayerEntity, IStateParam>>();
            smdic[EntityStatus.Idle] = new IdleState();
            smdic[EntityStatus.Attack] = new AttackState();
            smdic[EntityStatus.Move] = new MoveState();
            smdic[EntityStatus.UseSkill] = new UseSkillState();
            smdic[EntityStatus.Dead] = new DeadState();

            _stateMachine = new StateMachine<PlayerEntity>(smdic);
        }

        public S_JoinToGame.JoinPlayer CreateJoinPlayerPacket()
        {
            return new S_JoinToGame.JoinPlayer()
            {
                userId = this.userId,
                userName = this.userName,
                targetId = this.targetId,
                heroId = this.heroId
            };
        }


        //----------------
        // overrides
        //----------------

        protected override void ChangeState<P>(EntityStatus inStatue, P inParam)
        {
            _stateMachine.OnChangeState(inStatue, this, inParam);
        }


        //-----------------
        // functions
        //-----------------

        public void RegistRespawnTimer()
        {
            //중복해서 들어오는 것을 막아야함. 
            //TODO : Timer 클래스로 모두 빼야함. (취소 가능 기능 필요)
            JobTimer.Instance.Push(() =>
            {
                stat.AddCurrHp(stat.maxHp);
                Idle();

                S_Respawn respawn = new S_Respawn()
                {
                    targetId = targetId,
                    isPlayer = true,
                    stat = stat.ConvertStat(),
                };

                broadcaster.Broadcast(respawn);
            }, DEFINE.PLAYER_RESPAWN_MS);
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
    }
}
