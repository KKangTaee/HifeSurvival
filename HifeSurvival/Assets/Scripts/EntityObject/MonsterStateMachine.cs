using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------
// state machine
//--------------------

public partial class Monster
{
    public const int DISTANCE_DIFF = 2;

    public class AttackState : IState<Monster>
    {
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                TryAttack(attack, inSelf, attack.target);
        }

        public void Exit()
        {

        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is AttackParam attack)
                TryAttack(attack, inSelf, attack.target);
        }


        void TryAttack(AttackParam param, Monster fromMonster, EntityObject toEntity)
        {
            var currPos = fromMonster.GetPos();
            var distPos = fromMonster.TargetEntity.pos.ConvertUnityVector3();

            if(Vector3.Distance(currPos,  distPos) > DISTANCE_DIFF)
            {
                fromMonster.MoveLerpExpect(currPos, 
                                           distPos,fromMonster.TargetEntity.stat.moveSpeed, 
                                           default, 
                                           ()=>
                                           {
                                                Attack();
                                           });
            }
            else
            {
                fromMonster.SetPos(distPos);
                Attack();
            }


            void Attack()
            {
                var dir = Vector3.Normalize(toEntity.GetPos() - fromMonster.GetPos());
            
                fromMonster.OnAttack(dir);
                toEntity.OnDamaged(param.attackValue);
            }
        }
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
                Debug.Log($"HJ_SETIDLESTATE res.currpos {idleParam.pos}), current client pos {inSelf.transform.position}");
                Debug.DrawLine(idleParam.pos, idleParam.pos, Color.red, 1.0f);

                // 현재 서버좌표의 절대위치와 클라 동기화 위치의 간격이 꽤나 차이가 심하다면, 보간 후 정지 시킨다.
                if (Vector3.Distance(inSelf.GetPos(), idleParam.pos) > DISTANCE_DIFF)
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
            {
                inSelf.OnMoveLerp(move.currPos,
                  move.destPos,
                  move.speed,
                  move.timeStamp);

                Debug.Log($"HJ_SETMOVESTATE res.currpos {move.currPos}), res.targetPos {move.destPos}, current client pos {inSelf.transform.position}");
                Debug.DrawLine(move.currPos, move.destPos, Color.cyan, 1.0f);
            }
        }

        public void Exit()
        {

        }

        public void Update<P>(Monster inSelf, in P inParam) where P : struct
        {
            if (inParam is MoveParam move)
            {
                inSelf.OnMoveLerp(move.currPos,
                  move.destPos,
                  move.speed,
                  move.timeStamp);

                Debug.Log($"HJ_SETMOVESTATE res.currpos {move.currPos}), res.targetPos {move.destPos}, current client pos {inSelf.transform.position}");
                Debug.DrawLine(move.currPos, move.destPos, Color.cyan, 1.0f);
            }
        }
    }



}
