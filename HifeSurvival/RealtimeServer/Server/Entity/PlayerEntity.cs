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
            smdic[EStatus.IDLE] = new IdleState();
            smdic[EStatus.ATTACK] = new AttackState();
            smdic[EStatus.MOVE] = new MoveState();
            smdic[EStatus.USE_SKILL] = new UseSkillState();
            smdic[EStatus.DEAD] = new DeadState();

            //임시 코드.. Follow_target, Back_to_spawn은 유니크한 상태가 아님. 
            smdic[EStatus.FOLLOW_TARGET] = new FollowTargetState();
            smdic[EStatus.BACK_TO_SPAWN] = new BackToSpawnState();

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
            Logger.GetInstance().Debug($"state : {inStatue}");
            _stateMachine.OnChangeState(inStatue, this, inParam);
        }


        //-----------------
        // functions
        //-----------------

        public void OnSendRespawn()
        {
            JobTimer.Instance.Push(() =>
            {
                stat.AddCurrHp(stat.hp);
                Idle();

                S_Respawn respawn = new S_Respawn()
                {
                    targetId = targetId,
                    isPlayer = true,
                    stat = stat.ConvertStat(),
                };

                broadcaster.Broadcast(respawn);
            }, 15000);
        }

        public override void OnDamaged(in Entity entity)
        {
            if(entity.IsPlayer == false)
            {
                //do something
            }
        }
    }



    //-----------------------
    // Player StateMachine
    //-----------------------


    public partial class PlayerEntity
    {
        public class IdleState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                if(inParam is IdleParam idleParam)
                {
                    UpdateLocationBroadcast broadcast = new UpdateLocationBroadcast()
                    {
                        targetId = inSelf.targetId,
                        isPlayer = inSelf.IsPlayer,
                        status = (int)EStatus.IDLE,
                        currentPos = idleParam.currentPos,
                        timestamp = idleParam.timestamp,
                    };

                    inSelf.broadcaster.Broadcast(broadcast);
                }
            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {
            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {
            }
        }

        public class AttackState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is AttackParam attackParam)
                {
                    var target = attackParam.target;
                    if(target == null)
                    {
                        Logger.GetInstance().Warn($"Target is null");
                        return;
                    }

                    // 몬스터는 공격을 당했음으로, 플레이어를 공격한다.
                    // monster.OnAttack();

                    target.OnDamaged(inSelf);

                    // 여기서 함수가 필요한데.. 타겟이 맞았으면, 그 맞은 타겟의 몬스터들이 모두
                    // 플레이어를 공격해야함.
                }
                else
                {
                    Logger.GetInstance().Warn("AttackState has invalid param");
                }
            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                
            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                
            }
        }

        public class MoveState : IState<PlayerEntity, IStateParam>
        {
            private const int UPDATE_TIME = 125;


            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                var param = (MoveParam)inParam;
                Logger.GetInstance().Debug($"Move current : {param.currentPos.PrintPVec3()}, target : {param.targetPos.PrintPVec3()}");

                updateMove(inSelf, param);
            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                var param = (MoveParam)inParam;
                Logger.GetInstance().Debug($"Move current : {param.currentPos.PrintPVec3()}, target : {param.targetPos.PrintPVec3()}");

                updateMove(inSelf, param);
            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                Logger.GetInstance().Debug("Move Exit");
            }

            private void updateMove(PlayerEntity inSelf, MoveParam inParam)
            {
                UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                {
                    targetId = inSelf.targetId,
                    isPlayer = inSelf.IsPlayer,
                    status = (int)EStatus.MOVE,
                    currentPos = inParam.currentPos,
                    targetPos = inParam.targetPos,
                    speed = inParam.speed,
                    timestamp = inParam.timestamp,
                };

                inSelf.broadcaster.Broadcast(move);
            }
        }

        public class UseSkillState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class DeadState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                inSelf.OnSendRespawn();
            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        [Obsolete]
        public class FollowTargetState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        [Obsolete]
        public class BackToSpawnState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
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
