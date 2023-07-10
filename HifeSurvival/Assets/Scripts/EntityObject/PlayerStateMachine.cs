using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//------------------
// state machine
//------------------

public partial class Player
{
    public class FollowTargetState : IState<Player>
    {
        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is FollowTargetParam followTarget)
            {
                inSelf.OnFollowTarget(followTarget.target,
                                      followTarget.followDirCallback,
                                      followTarget.followDoneCallback);
            }
        }

        public void Exit()
        {

        }

        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {

        }
    }

    public class AttackState : IState<Player>
    {
        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                OnAttack(inSelf, attack);
        }

        public void Exit()
        {

        }

        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                OnAttack(inSelf, attack);
        }

        private void OnAttack(Player inFrom, AttackParam inParam)
        {
            EntityObject inTo = inParam.target;

            // 내 클라의 전송값과 서버의 위치값이 크게 다르다면..?
            Attack(inParam, inFrom, inTo);
        }

        #region  Local Func
        void Attack(AttackParam param, Player fromPlayer, EntityObject toEntity)
        {
            var dir = Vector3.Normalize(toEntity.GetPos() - fromPlayer.GetPos());

            fromPlayer.OnAttack(dir);
            toEntity.OnDamaged(param.attackValue);

            if (fromPlayer.IsSelf == true)
                ActionDisplayUI.Show(ActionDisplayUI.ESpawnType.ATTACK, param.attackValue, toEntity.GetPos() + Vector3.up);

            ControllerManager.Instance.GetController<FXController>().Play(EFX_ID.HIT_A_DIRECTIONAL_RED, toEntity.GetPos() + Vector3.up);
        }
        #endregion
    }

    public class DeadState : IState<Player>
    {
        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is DeadParam dead)
            {
                // TODO@taeho.kang 나중에 처리.
                inSelf.OnDead();
            }
        }

        public void Exit()
        {

        }

        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {

        }
    }

    public class IdleState : IState<Player>
    {
        const float DISTANCE_DIFF = 2f;

        public void Enter<P>(Player inSelf, in P inParam) where P : struct
        {
            if (inParam is IdleParam idleParam)
            {
                 // 현재 서버좌표의 절대위치와 클라 동기화 위치의 간격이 꽤나 차이가 심하다면, 보간 후 정지 시킨다.
                if(inSelf.IsSelf == false && Vector3.Distance(inSelf.GetPos(), idleParam.pos) > DISTANCE_DIFF)
                {
                    inSelf.MoveLerpExpect(inSelf.GetPos(), 
                                        idleParam.pos, 
                                        idleParam.speed, 
                                        0,
                                        ()=>
                                        {
                                            inSelf.OnIdle(idleParam.pos);
                                        });
                }
                else
                {
                    inSelf.OnIdle(idleParam.pos);
                }
            }
        }

        public void Update<P>(Player inSelf, in P inParam) where P : struct
        {

        }

        public void Exit()
        {

        }
    }

    public class MoveState : IState<Player>
    {
        public void Enter<P>(Player inTarget, in P inParam) where P : struct
        {
            if(inParam is MoveParam move)
                Move(move, inTarget);
        }


        public void Update<P>(Player inTarget, in P inParam) where P : struct
        {
            if(inParam is MoveParam move)
                Move(move, inTarget);
        }


        public void Exit()
        {

        }

        public void Move(MoveParam inMoveParam, Player inPlayer)
        {
            if (inPlayer.IsSelf == false)
                inPlayer.OnMoveLerp(inMoveParam.currPos,
                                    inMoveParam.destPos,
                                    inMoveParam.speed,
                                    inMoveParam.timeStamp);

            else
                inPlayer.OnMove(inMoveParam.dir);
        }
    }

}