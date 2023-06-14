using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;
using System.Linq;

namespace Server
{
    public partial class PlayerEntity : Entity
    {
        public string userId;
        public string userName;
        
        public int  heroId;
        public bool isReady;

        public int  gold;

        public EntityItem[] _itemSlot = new EntityItem[4];

        public override bool IsPlayer => true;

        StateMachine<PlayerEntity> _stateMachine;

        public PlayerEntity()
        {
            _stateMachine = new StateMachine<PlayerEntity>(new Dictionary<EStatus, IState<PlayerEntity>>()
            {
                {EStatus.IDLE, new IdleState()},
                {EStatus.ATTACK,  new AttackState()},
                {EStatus.MOVE, new MoveState() },
                {EStatus.USE_SKILL, new UseSkillState()},
                {EStatus.DEAD, new DeadState()}
            });
        }

        public S_JoinToGame.JoinPlayer CreateJoinPlayerPacket()
        {
            return new S_JoinToGame.JoinPlayer()
            {
                userId  = this.userId,
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

        public override int GetAttackValue()
        {
            var extraStr = _itemSlot.Sum(x=>
            {   
                return x == null ? 0 : x.str; 
            });

            // TODO@taeho.kang 여기도 나중에 정책으로 수정합시다..
            return base.GetAttackValue() + extraStr;
        }


        public override int GetDamagedValue(int inAttackValue)
        {
            var extraDef = _itemSlot.Sum(x=>
            {   
                return x == null ? 0 : x.def; 
            });

            // TODO@taeho.kang 여기도 나중에 정책을 정해서 수정합시다.
            return base.GetDamagedValue(inAttackValue) - extraDef;
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

        public void EquipItem(EntityItem inItem)
        {
            if(_itemSlot.Length < 0 || _itemSlot.Length >= inItem.itemSlotId || _itemSlot[inItem.itemSlotId] != null)
            {
                HSLogger.GetInstance().Error($"[{nameof(EquipItem)}] itemSlot is wrong! : {inItem.itemSlotId}");
                return;
            }

            _itemSlot[inItem.itemSlotId] = inItem;
        }
    }



    //-----------------------
    // Player StateMachine
    //-----------------------


    public partial class PlayerEntity
    {
        public class IdleState : IState<PlayerEntity>
        {
            public void Enter<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
            }

            public void Exit<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
            }

            public void Update<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
            }
        }

        public class AttackState : IState<PlayerEntity>
        {
            public void Enter<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
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
            }


            public void Exit<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {

            }


            public void Update<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {

            }
        }

        public class MoveState : IState<PlayerEntity>
        {
            private const int UPDATE_TIME = 125;

            private bool _isRunning = false;

            public void Enter<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                _isRunning = true;
                UpdateMove(inSelf);
            }

            public void Update<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                
            }

            public void Exit<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                _isRunning = false;
            }

            public void UpdateMove(PlayerEntity inSelf)
            {
                if (this != null && _isRunning == true && inSelf != null)
                {
                    inSelf.OnMoveAndBroadcast(inSelf.dir, UPDATE_TIME * 0.001f);
                    JobTimer.Instance.Push(() => { UpdateMove(inSelf); }, UPDATE_TIME);
                }
            }
        }

        public class UseSkillState : IState<PlayerEntity>
        {

            public void Enter<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                
            }

            public void Exit<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                
            }

            public void Update<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                
            }
        }

        public class DeadState : IState<PlayerEntity>
        {
            public void Enter<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
                inSelf.OnSendRespawn();
            }

            public void Exit<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {
            }

            public void Update<P>(PlayerEntity inSelf, in P inParam = default) where P : struct, IStateParam
            {

            }
        }
    }
}
