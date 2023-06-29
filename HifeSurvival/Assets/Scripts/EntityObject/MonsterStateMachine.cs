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
        public void Enter<P>(Monster inSelf, in P inParam) where P : struct
        {

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
