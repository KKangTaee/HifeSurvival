using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Entity
    {
        public enum EStatus
        {
            IDLE,

            FOLLOW_TARGET,

            ATTACK,

            DEAD,

            BACK_TO_SPAWN,

            USE_SKILL,

            MOVE
        }

        public IBroadcaster broadcaster;

        public int targetId;

        public Vec3 pos;
        public Vec3 dir;
        public Vec3 spawnPos;

        public EntityStat stat;

        public EStatus Status;

        public abstract bool IsPlayer { get; }

        protected virtual void ChangeState<P>(Entity.EStatus inStatue,  P inParam) where P : struct, IStateParam
        {
            Status = inStatue;
        }

        public virtual void OnAttack(in AttackParam inParam = default)
        {
            ChangeState(EStatus.ATTACK, inParam);
        }

        public virtual void OnFollowTarget(in FollowTargetParam inParam = default)
        {
            ChangeState(EStatus.FOLLOW_TARGET, inParam);
        }

        public virtual void OnIdle(in IdleParam inParam = default)
        {
            ChangeState(EStatus.IDLE, inParam);
        }

        public virtual void OnMove(in MoveParam inParam = default)
        {
            ChangeState(EStatus.MOVE, inParam);
        }

        public virtual void OnDead(in DeadParam inParam = default)
        {
            ChangeState(EStatus.DEAD, inParam);
        }


        public void OnMoveAndBroadcast(in Vec3 inEndPos, float deltaTime)
        {
            var newDir = inEndPos.SubtractVec3(pos).NormalizeVec3();
            var addSpeed = newDir.MulitflyVec3(stat.moveSpeed * deltaTime);

            dir = newDir;
            pos = pos.AddVec3(addSpeed);

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

        public void OnMoveLerpAndBroadcast(in Vec3 inStartPos, in Vec3 inEndPos, float inRaio)
        {
            pos = Vec3Helper.Lerp(inStartPos, inEndPos, inRaio);

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

        public float detectRange { get; private set; }
        public float attackRange { get; private set; }
        public float moveSpeed { get; private set; }
        public float attackSpeed { get; private set; }


        public EntityStat(StaticData.Heros heros)
        {
            str = heros.str;
            def = heros.def;
            hp  = heros.hp;
            detectRange = heros.detectRange;
            attackRange = heros.attackRange;
            moveSpeed = heros.moveSpeed;
            attackSpeed = heros.attackSpeed;
        }


        public int GetAttackValue()
        {
            return (int)new Random().Next(str - 15, (int)(str * 1.2f));
        }


        public int GetDamagedValue(int inAttackValue) =>
           (int)(inAttackValue - def * 0.1f);


        public void AddStr(int inStr) =>
            str += inStr;

        public void AddDef(int inDef) =>
            def += inDef;

        public void AddHp(int inHp) =>
            hp += inHp;

    }
}