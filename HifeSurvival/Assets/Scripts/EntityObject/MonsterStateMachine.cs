using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------
// state machine
//--------------------

public partial class Monster
{
    public class AttackState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                Attack(attack, inSelf, attack.target);
        }

        public void Exit()
        {

        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                Attack(attack, inSelf, attack.target);
        }

        #region  Local Func
        void Attack(AttackParam param, Monster fromMonster, EntityObject toEntity)
        {
            var dir = Vector3.Normalize(toEntity.GetPos() - fromMonster.GetPos());

            fromMonster.OnAttack(dir);
            toEntity.OnDamaged(param.attackValue);
        }
        #endregion
    }

    public class DeadState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            inSelf.OnDead();
        }

        public void Exit()
        {

        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {

        }
    }

    public class IdleState : IState<Monster>
    {
        const float DISTANCE_DIFF = 2f;

        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is IdleParam idleParam)
            {
                 // 현재 서버좌표의 절대위치와 클라 동기화 위치의 간격이 꽤나 차이가 심하다면, 보간 후 정지 시킨다.
                if(Vector3.Distance(inSelf.GetPos(), idleParam.pos) > DISTANCE_DIFF)
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

        public void Exit()
        {

        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {

        }
    }

    public class MoveState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is MoveParam move)
                inSelf.OnMoveLerp(move.currPos,
                                  move.destPos,
                                  move.speed,
                                  move.timeStamp);
        }

        public void Exit()
        {

        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is MoveParam move)
                inSelf.OnMoveLerp(move.currPos,
                                  move.destPos,
                                  move.speed,
                                  move.timeStamp);
        }
    }



}
