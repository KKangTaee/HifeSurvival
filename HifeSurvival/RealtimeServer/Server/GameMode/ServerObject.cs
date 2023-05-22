using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Entity<T> where T : Entity<T>
    {
        public enum EStatus
        {
            IDLE,

            FOLLOW_TARGET,

            ATTACK,

            DAMAGED,

            BACK_TO_SPAWN,

            USE_SKILL,

            MOVE
        }

        protected EStatus _status;
        protected IState<T> _state;


        protected Dictionary<EStatus, IState<T>> _stateMachine;

        public Vec3  pos;
        public Vec3  dir;
        public float speed;

        public Stat stat;

        public IBroadcaster broadcaster;

        private void ChangeState<P>(IState<T> newState, P inParam) where P : IStateParam
        {
            _state?.Exit((T)this, inParam);
            _state = newState;
            _state?.Enter((T)this, inParam);
        }

        public void ChangeState<P>(EStatus inStatus, in P inParam) where P : IStateParam
        {
            _status = inStatus;
            ChangeState(_stateMachine[_status], inParam);
        }    
    }

    public partial class MonsterEntity : Entity<MonsterEntity>
    {
        public int monsterId;
        public int monsterType;
        public int groupId;
        public int subId;

        public MonsterEntity()
        {
            _stateMachine = new Dictionary<EStatus, IState<MonsterEntity>>()
            {
                { EStatus.IDLE, new IdleState()},
                { EStatus.FOLLOW_TARGET, new FollowTargetState()},
                { EStatus.ATTACK, new AttackState()},
                // { EStatus.DAMAGED, new DamagedState()},
                { EStatus.BACK_TO_SPAWN, new BackToSpawnState() }
            };
        }


        public void OnAttack<T>(in AttackParam<T> inParam) where T : Entity<T>
        {
            ChangeState(EStatus.ATTACK, inParam);
        }


        public void OnFollowTarget<T>(in FollowTargetParam<T> inParam) where T : Entity<T>
        {
            ChangeState(EStatus.FOLLOW_TARGET, inParam);
        }


        public void OnIdle(in IdleParam other)
        {
            ChangeState(EStatus.IDLE, other);
        }

        public void OnDamaged<T>(in DamagedParam<T> inParam)
        {
            ChangeState(EStatus.DAMAGED, inParam);
        }

        public bool CanAttack()
        {
            // 나중에 수정할것이며 임시로 만들어 놓음
            return false;
        }

        public int Attack()
        {
            return 100;
        }
    }

    public partial class PlayerEntity : Entity<PlayerEntity>
    {
        public string userId;
        public string userName;

        public int playerId;
        public int heroType;
        public bool isReady;

        public PlayerEntity()
        {
            _stateMachine = new Dictionary<EStatus, IState<PlayerEntity>>()
            {
                {EStatus.IDLE, new IdleState()},
                {EStatus.ATTACK,  new IdleState()},
                // {EStatus.DAMAGED, new DamagedState()},
                {EStatus.MOVE, new MoveState() },
                {EStatus.USE_SKILL, new UseSkillState()}
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

        public void OnAttack<T>(in AttackParam<T> inParam) where T : Entity<T>
        {
            ChangeState(EStatus.ATTACK, inParam);
        }

        public void OnIdle(in IdleParam other)
        {
            ChangeState(EStatus.IDLE, other);
        }

        public void OnDamaged<T>(in DamagedParam<T> inParam) where T : Entity<T>
        {
            ChangeState(EStatus.DAMAGED, inParam);
        }

        public void OnMove(in MoveParam inParam)
        {
            ChangeState(EStatus.MOVE, inParam);
        }
    }


    public struct Stat
    {
        public readonly int maxHP;
        public readonly int maxEXP;
        public readonly int maxSTR;

        public int hp;
        public int exp;
        public int str;

        public Stat(int inMaxHP, int inMaxEXP, int inMaxSTR)
        {
            hp = maxHP = inMaxHP;
            exp = maxEXP = inMaxEXP;
            str = maxSTR = inMaxSTR;
        }
    }
}