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

        public Vec3 pos;
        public Vec3 dir;
        public EntityStat stat;
        
        public IBroadcaster broadcaster;

        protected EStatus _status;
        protected IState _state;

        protected Dictionary<EStatus, IState> _stateMachine;

        private void ChangeState<P>(EStatus inStatue, P inParam) where P : struct, IStateParam
        {
            if (_status == inStatue)
            {
                _state?.Update(inParam);
            }
            else
            {
                _status = inStatue;
                _state?.Exit(this, inParam);
                _state = _stateMachine[_status];
                _state?.Enter(this, inParam);
            }
        }


        public bool CanAttack()
        {
            // 나중에 수정할것이며 임시로 만들어 놓음
            return false;
        }

        public virtual void OnAttack(in AttackParam inParam)
        {
            ChangeState(EStatus.ATTACK, inParam);
        }

        public virtual void OnFollowTarget(in FollowTargetParam inParam)
        {
            ChangeState(EStatus.FOLLOW_TARGET, inParam);
        }

        public virtual void OnIdle(in IdleParam inParam)
        {
            ChangeState(EStatus.IDLE, inParam);
        }

        public virtual void OnMove(in MoveParam inParam)
        {
            ChangeState(EStatus.MOVE, inParam);
        }

        public virtual void OnDead(in DeadParam inParam)
        {
            ChangeState(EStatus.DEAD, inParam);
        }

    }

    public partial class MonsterEntity : Entity
    {
        public int monsterId;
        public int monsterType;
        public int groupId;
        public int subId;

        public MonsterEntity()
        {
            _stateMachine = new Dictionary<EStatus, IState>()
            {
                { EStatus.IDLE, new IdleState()},
                { EStatus.FOLLOW_TARGET, new FollowTargetState()},
                { EStatus.ATTACK, new AttackState()},
            };
        }


        //----------------
        // overrides
        //----------------

        public int GetAttackValue()
        {
            return 100;
        }


        public bool CanAttack(Vec3 inPos)
        {
            return false;
        }
    }


    public partial class PlayerEntity : Entity
    {
        public string userId;
        public string userName;

        public int playerId;
        public int heroType;
        public bool isReady;

        public PlayerEntity()
        {
            _stateMachine = new Dictionary<EStatus, IState>()
            {
                {EStatus.IDLE, new IdleState()},
                {EStatus.ATTACK,  new AttackState()},
                {EStatus.MOVE, new MoveState() },
                {EStatus.USE_SKILL, new UseSkillState()},
                {EStatus.DEAD, new DeadState()}
            };
        }


        public S_JoinToGame.JoinPlayer CreateJoinPlayerPacket()
        {
            return new S_JoinToGame.JoinPlayer()
            {
                userId = this.userId,
                userName = this.userName,
                playerId = this.playerId,
                heroId = this.heroType
            };
        }


        //----------------
        // overrides
        //----------------
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
            hp = heros.hp;
            detectRange = heros.detectRange;
            attackRange = heros.attackRange;
            moveSpeed = heros.moveSpeed;
            attackSpeed = heros.attackSpeed;
        }


        public int GetAttackValue()
        {
            return (int)new Random().Next(str - 15, (int)(str + str * 0.2f));
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