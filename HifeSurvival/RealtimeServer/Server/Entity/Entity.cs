using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Helper;
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


        [Obsolete] public PVec3 pos;
        [Obsolete] public PVec3 dir;
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
        public abstract void OnDamaged(in Entity entity);
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

        [Obsolete]
        public void OnMoveAndBroadcast(in PVec3 inDir, float deltaTime)
        {
            var addSpeed = inDir.MulitflyPVec3(stat.moveSpeed * deltaTime);
            pos = pos.AddPVec3(addSpeed);

            CS_Move move = new CS_Move()
            {
                pos = this.pos,
                dir = this.dir,
                targetId = this.targetId,
                isPlayer = IsPlayer,
                speed = this.stat.moveSpeed,
            };

            broadcaster.Broadcast(move);
        }

        [Obsolete]
        public void OnMoveLerpAndBroadcast(in PVec3 inStartPos, in PVec3 inEndPos, float inRaio)
        {
            pos = PacketExtensionHelper.Lerp(inStartPos, inEndPos, inRaio);

            CS_Move move = new CS_Move()
            {
                pos = this.pos,
                dir = this.dir,
                targetId = this.targetId,
                isPlayer = IsPlayer,
                speed = this.stat.moveSpeed,
            };

            broadcaster.Broadcast(move);
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


        public EntityStat(StaticData.Heros heros)
        {
            str = heros.str;
            def = heros.def;
            currHp = hp = heros.hp;
            detectRange = heros.detectRange;
            attackRange = heros.attackRange;
            moveSpeed = heros.moveSpeed;
            attackSpeed = heros.attackSpeed;
        }

        public EntityStat(StaticData.Monsters monsters)
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