using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public abstract class Entity
    {
        public IBroadcaster broadcaster;

        public int id;

        public PVec3 spawnPos;

        public PVec3 currentPos;
        public PVec3 targetPos;

        public EntityStat stat;
        public EntityStatus Status;

        #region StatusAction
        protected abstract void ChangeState<P>(EntityStatus inStatue, P inParam) where P : struct, IStateParam;
        public virtual void Attack(AttackParam inParam)
        {
            Status = EntityStatus.Attack;
            ChangeState(EntityStatus.Attack, inParam);
        }

        public virtual void Idle(in IdleParam inParam = default)
        {
            Status = EntityStatus.Idle;
            ChangeState(EntityStatus.Idle, inParam);
        }

        public virtual void MoveStop(in IdleParam inParam = default)
        {
            Status = EntityStatus.Idle;
            ChangeState(EntityStatus.Idle, inParam);
        }

        public virtual void Move(in MoveParam inParam = default)
        {
            Status = EntityStatus.Move;
            ChangeState(EntityStatus.Move, inParam);
        }

        public virtual void Dead(in DeadParam inParam = default)
        {
            Status = EntityStatus.Dead;
            ChangeState(EntityStatus.Dead, inParam);
        }
        #endregion


        public abstract void OnDamaged(in Entity attacker);


        public virtual void MoveToRespawn()
        {
            Move(new MoveParam()
            {
                currentPos = currentPos,
                targetPos = spawnPos,
                speed = stat.moveSpeed,
                timestamp = ServerTime.GetCurrentTimestamp()
            });
        }

        public virtual void MoveToTarget(in PVec3 targetPos)
        {
            Move(new MoveParam()
            {
                currentPos = currentPos,
                targetPos = targetPos,
                speed = stat.moveSpeed,
                timestamp = ServerTime.GetCurrentTimestamp()
            });
        }

        public virtual void ReduceHP(int hpValue)
        {
            stat.AddCurrHp(-hpValue);
        }

        public virtual bool IsDead()
        {
            return stat.curHp < 0 || Status == EntityStatus.Dead;
        }
    }
}

