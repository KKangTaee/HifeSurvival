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
        public EntityStat defaultStat;

        public EEntityStatus Status;

        #region StatusAction
        protected abstract void ChangeState<P>(EEntityStatus inStatue, P inParam) where P : struct, IStateParam;
        public virtual void Attack(AttackParam inParam)
        {
            Status = EEntityStatus.ATTACK;
            ChangeState(EEntityStatus.ATTACK, inParam);
        }

        public virtual void Idle(in IdleParam inParam = default)
        {
            Status = EEntityStatus.IDLE;
            ChangeState(EEntityStatus.IDLE, inParam);
        }

        public virtual void MoveStop(in IdleParam inParam = default)
        {
            Status = EEntityStatus.IDLE;
            ChangeState(EEntityStatus.IDLE, inParam);
        }

        public virtual void Move(in MoveParam inParam = default)
        {
            Status = EEntityStatus.MOVE;
            ChangeState(EEntityStatus.MOVE, inParam);
        }

        public virtual void Dead(in DeadParam inParam = default)
        {
            Status = EEntityStatus.DEAD;
            ChangeState(EEntityStatus.DEAD, inParam);
        }
        #endregion


        public abstract void UpdateStat();
        public abstract void GetStat(out EntityStat defaultStat, out EntityStat additionalStat);



        #region Callback
        public abstract void OnDamaged(in Entity attacker);

        public virtual void OnStatChange()
        {
            var broadcast = new UpdateStatBroadcast();
            broadcast.id = id;

            GetStat(out var originStat, out var addStat);
            broadcast.originStat = originStat.ConvertToPStat();
            broadcast.addStat = addStat.ConvertToPStat();

            broadcaster.Broadcast(broadcast);
        }
        #endregion


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
            return stat.curHp < 0 || Status == EEntityStatus.DEAD;
        }
    }
}

