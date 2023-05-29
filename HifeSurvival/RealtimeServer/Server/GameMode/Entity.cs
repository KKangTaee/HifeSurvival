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
        public Stat stat;


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


        public bool CanAttack()
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


    public class Stat
    {
        public readonly int maxHP;
        public readonly int maxEXP;
        public readonly int maxSTR;

        public int hp;
        public int exp;
        public int str;
        public float attackRange;
        public float speed;

        public Stat(int inMaxHP, int inMaxEXP, int inMaxSTR, float inAttackRange, float inSpeed)
        {
            hp = maxHP = inMaxHP;
            exp = maxEXP = inMaxEXP;
            str = maxSTR = inMaxSTR;
            attackRange = inAttackRange;
            speed = inSpeed;
        }

        public int GetAttackValue()
        {
            return new Random().Next(str - 15, str + 15);
        }
    }
}