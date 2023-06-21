using System;
using System.Collections.Generic;
using System.Text;
using Server.Helper;
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
            var smdic = new Dictionary<EStatus, IState<PlayerEntity, IStateParam>>();
            smdic[EStatus.IDLE] = (IState<PlayerEntity, IStateParam>)new IdleState();
            smdic[EStatus.ATTACK] = (IState<PlayerEntity, IStateParam>)new AttackState();
            smdic[EStatus.MOVE] = (IState<PlayerEntity, IStateParam>)new MoveState();
            smdic[EStatus.USE_SKILL] = (IState<PlayerEntity, IStateParam>)new UseSkillState();
            smdic[EStatus.DEAD] = (IState<PlayerEntity, IStateParam>)new DeadState();

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

        protected override void ChangeState<P>(EStatus inStatue, P inParam)
        {
            base.ChangeState(inStatue, inParam);

            _stateMachine.ChangeState(inStatue, this, inParam);
        }


        //-----------------
        // functions
        //-----------------

        public void OnSendRespawn()
        {
            JobTimer.Instance.Push(() =>
            {
                stat.AddCurrHp(stat.hp);
                OnIdle();

                S_Respawn respawn = new S_Respawn()
                {
                    targetId = targetId,
                    isPlayer = true,
                    stat = stat.ConvertStat(),
                };

                broadcaster.Broadcast(respawn);
            }, 15000);
        }

    }



    //-----------------------
    // Player StateMachine
    //-----------------------


    public partial class PlayerEntity
    {
        public class IdleState : IState<PlayerEntity, IdleParam>
        {
            public void Enter(PlayerEntity inSelf, in IdleParam inParam = default)
            {

            }

            public void Exit(PlayerEntity inSelf, in IdleParam inParam = default)
            {
            }

            public void Update(PlayerEntity inSelf, in IdleParam inParam = default)
            {
            }
        }

        public class AttackState : IState<PlayerEntity, AttackParam>
        {
            public void Enter(PlayerEntity inSelf, in AttackParam inParam = default)
            {
                if (inParam is AttackParam attack)
                {
                    // 플레이어 -> 몬스터 공격
                    if (attack.target is MonsterEntity monster)
                    {
                        // 몬스터는 공격을 당했음으로, 플레이어를 공격한다.
                        // monster.OnAttack();

                        monster.OnDamaged(inSelf);

                        // 여기서 함수가 필요한데.. 타겟이 맞았으면, 그 맞은 타겟의 몬스터들이 모두
                        // 플레이어를 공격해야함.

                    }

                    // 플레이어 - 플레이어일 경우에는 그대로 대기
                }
                else
                {
                    Logger.GetInstance().Warn("AttackState has invalid param");
                }
            }

            public void Exit(PlayerEntity inSelf, in AttackParam inParam = default)
            {
                
            }

            public void Update(PlayerEntity inSelf, in AttackParam inParam = default)
            {
                
            }
        }

        public class MoveState : IState<PlayerEntity, MoveParam>
        {
            private const int UPDATE_TIME = 125;

            private bool _isRunning = false;

            public void Enter(PlayerEntity inSelf, in MoveParam inParam = default)
            {
                _isRunning = true;

                updateMove(inSelf, inParam);
            }

            public void Update(PlayerEntity inSelf, in MoveParam inParam = default)
            {
                updateMove(inSelf,  inParam);
            }

            public void Exit(PlayerEntity inSelf, in MoveParam inParam = default)
            {
                _isRunning = false;
            }

            private void updateMove(PlayerEntity inSelf, MoveParam inParam)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    //inSelf.OnMoveAndBroadcast(inSelf.dir, UPDATE_TIME * 0.001f);
                    inSelf.OnMove(inParam);
                    JobTimer.Instance.Push(() => { updateMove(inSelf, inParam); }, UPDATE_TIME);
                }
            }
        }

        public class UseSkillState : IState<PlayerEntity, UseSkillParam>
        {
            public void Enter(PlayerEntity inSelf, in UseSkillParam inParam = default)
            {

            }

            public void Exit(PlayerEntity inSelf, in UseSkillParam inParam = default)
            {

            }

            public void Update(PlayerEntity inSelf, in UseSkillParam inParam = default)
            {

            }
        }

        public class DeadState : IState<PlayerEntity, DeadParam>
        {
            public void Enter(PlayerEntity inSelf, in DeadParam inParam = default)
            {
                inSelf.OnSendRespawn();
            }

            public void Exit(PlayerEntity inSelf, in DeadParam inParam = default)
            {

            }

            public void Update(PlayerEntity inSelf, in DeadParam inParam = default)
            {

            }
        }
    }

    public class PlayerItem
    {
        public int itemType;
        public int level;

        public int str;
        public int def;
        public int hp;

        public int cooltime;

        public bool canUse;
    }
}
