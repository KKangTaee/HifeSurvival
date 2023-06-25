using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.GameData;
using ServerCore;

namespace Server
{
    public abstract class Entity
    {
        public enum EStatus
        {
            IDLE,

            ATTACK,

            DEAD,

            USE_SKILL,

            MOVE
        }

        public IBroadcaster broadcaster;

        public int targetId;

        public PVec3 spawnPos;

        public PVec3 currentPos;
        public PVec3 targetPos;

        public EntityStat stat;

        public EStatus Status;

        public abstract bool IsPlayer { get; }

        #region StatusAction
        protected abstract void ChangeState<P>(Entity.EStatus inStatue, P inParam) where P : struct, IStateParam;
        public virtual void Attack(AttackParam inParam)
        {
            Status = EStatus.ATTACK;
            ChangeState(EStatus.ATTACK, inParam);
        }

        public virtual void Idle(in IdleParam inParam = default)
        {
            Status = EStatus.IDLE;
            ChangeState(EStatus.IDLE, inParam);
        }

        public virtual void MoveStop(in IdleParam inParam = default)
        {
            Status = EStatus.IDLE;
            ChangeState(EStatus.IDLE, inParam);
        }

        public virtual void Move(in MoveParam inParam = default)
        {
            Status = EStatus.MOVE;
            ChangeState(EStatus.MOVE, inParam);
        }

        public virtual void Dead(in DeadParam inParam = default)
        {
            Status = EStatus.DEAD;
            ChangeState(EStatus.DEAD, inParam);
        }

        #endregion


        #region Callback Event
        public abstract void OnDamaged(in Entity attacker);
        #endregion

        public virtual void MoveToRespawn()
        {
            Move(new MoveParam()
            {
                currentPos = currentPos,
                targetPos = spawnPos,
                speed = stat.moveSpeed,
                timestamp = HTimer.GetCurrentTimestamp()
            }) ;
        }

        public virtual void MoveToTarget(in PVec3 targetPos)
        {
            Move(new MoveParam()
            {
                currentPos = currentPos,
                targetPos = targetPos,
                speed = stat.moveSpeed,
                timestamp = HTimer.GetCurrentTimestamp()
            });
        }

        //Battle
        public virtual int GetAttackValue()
        {
            return new Random().Next(stat.str - 15, (int)(stat.str * 1.2f));
        }

        //Battle
        public virtual int GetDamagedValue(int inAttackValue)
        {
            return (int)(inAttackValue - stat.def * 0.1f);
        }

        public virtual void ReduceHP(int hpValue)
        {
            stat.AddCurrHp(-hpValue);
        }

        public virtual bool IsDead()
        {
            return stat.currHp < 0 || Status == EStatus.DEAD;
        }
    }


    public class EntityStat
    {
        public int str { get; private set; }
        public int def { get; private set; }

        public int hp { get; private set; }
        public int currHp { get; private set; }

        public float detectRange { get; private set; }
        public float attackRange { get; private set; }
        public float moveSpeed { get; private set; }
        public float attackSpeed { get; private set; }


        public EntityStat(GameDataLoader.Heros heros)
        {
            str = heros.str;
            def = heros.def;
            currHp = hp = heros.hp;
            detectRange = heros.detectRange;
            attackRange = heros.attackRange;
            moveSpeed = heros.moveSpeed;
            attackSpeed = heros.attackSpeed;
        }

        public EntityStat(GameDataLoader.Monsters monsters)
        {
            str = monsters.str;
            def = monsters.def;
            currHp = hp = monsters.hp;
            detectRange = monsters.detectRange;
            attackRange = monsters.attackRange;
            moveSpeed = monsters.moveSpeed;
            attackSpeed = monsters.attackSpeed;
        }

        public void AddStr(int inStr) =>
            str += inStr;

        public void AddDef(int inDef) =>
            def += inDef;

        public void AddMaxHp(int inHp) =>
            hp += inHp;

        public void AddCurrHp(int inHp) =>
            currHp += inHp;
    }
}