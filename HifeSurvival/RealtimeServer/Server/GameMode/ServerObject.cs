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

        public Vec3 position;
        public Vec3 dir;
        public Stat stat;

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

        public void Update(double deltaTime)
        {
            if (_state is IUpdate<T> update)
                update.Update((T)this, deltaTime);
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
                { EStatus.DAMAGED, new DamagedState()},
                { EStatus.BACK_TO_SPAWN, new BackToSpawnState() }
            };
        }


        public void OnAttack<T>(in AttackParam<T> other) where T : Entity<T>
        {
            ChangeState(EStatus.ATTACK, other);
        }


        public void OnFollowTarget<T>(in FollowTargetParam<T> other) where T : Entity<T>
        {
            ChangeState(EStatus.FOLLOW_TARGET, other);
        }


        public void OnIdle(in IdleParam other)
        {
            ChangeState(EStatus.IDLE, other);
        }

        public void OnDamaged(in DamagedParam<MonsterEntity> other)
        {
            ChangeState(EStatus.DAMAGED, other);
        }


        public bool CanAttack()
        {
            // 나중에 수정할것이며 임시로 만들어 놓음
            return false;
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
                {EStatus.ATTACK,  new IdleState()},
                {EStatus.DAMAGED, new DamagedState()},
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
                heroType = this.heroType
            };
        }


        public void OnAttack(in AttackParam<PlayerEntity> inParam)
        {
            ChangeState(EStatus.ATTACK, inParam);
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