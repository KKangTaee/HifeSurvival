using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    public abstract class Entity
    {
        public int id;

        public PVec3 spawnPos;
        public PVec3 currentPos;
        public PVec3 targetPos;

        public EntityStat Stat { get; protected set; }
        public EntityStat DefaultStat { get; protected set; }

        public EEntityStatus status;

        public GameRoom Room { get; private set; }

        public Entity(GameRoom room)
        {
            this.Room = room;
        }

        #region StatusAction
        protected abstract void ChangeState<P>(EEntityStatus status, P param) where P : struct, IStateParam;
        public virtual void Attack(AttackParam param)
        {
            status = EEntityStatus.ATTACK;
            ChangeState(EEntityStatus.ATTACK, param);
        }

        public virtual void Idle(in IdleParam param = default)
        {
            status = EEntityStatus.IDLE;
            ChangeState(EEntityStatus.IDLE, param);
        }

        public virtual void MoveStop(in IdleParam param = default)
        {
            status = EEntityStatus.IDLE;
            ChangeState(EEntityStatus.IDLE, param);
        }

        public virtual void Move(in MoveParam param = default)
        {
            status = EEntityStatus.MOVE;
            ChangeState(EEntityStatus.MOVE, param);
        }

        public virtual void Dead(in DeadParam param = default)
        {
            status = EEntityStatus.DEAD;
            ChangeState(EEntityStatus.DEAD, param);
        }
        #endregion


        public abstract void UpdateStat();

        public abstract PStat GetDefaultPStat();
        public abstract PStat GetAdditionalPStat();

        public abstract void OnDamaged(in Entity attacker);

        public virtual void OnStatChange()
        {
            var broadcast = new UpdateStatBroadcast();
            broadcast.id = id;
            broadcast.originStat = GetDefaultPStat();
            broadcast.addStat = GetAdditionalPStat();

            Room.Broadcast(broadcast);
        }


        public virtual void MoveToRespawn()
        {
            Move(new MoveParam()
            {
                currentPos = currentPos,
                targetPos = spawnPos,
                speed = Stat.MoveSpeed,
                timestamp = ServerTime.GetCurrentTimestamp()
            });
        }

        public virtual void MoveToTarget(in PVec3 targetPos)
        {
            Move(new MoveParam()
            {
                currentPos = currentPos,
                targetPos = targetPos,
                speed = Stat.MoveSpeed,
                timestamp = ServerTime.GetCurrentTimestamp()
            });
        }

        public virtual void ReduceHP(int hpValue)
        {
            Stat.AddCurrHp(-hpValue);
        }

        public virtual bool IsDead()
        {
            return Stat.CurHp < 0 || status == EEntityStatus.DEAD;
        }
    }
}

