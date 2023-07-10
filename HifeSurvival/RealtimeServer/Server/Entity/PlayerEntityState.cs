
namespace Server
{
    public partial class PlayerEntity
    {
        public class IdleState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity self, in IStateParam param = default)
            {
                if (param is IdleParam idleParam)
                {
                    var locationBroadcast = new UpdateLocationBroadcast()
                    {
                        id = self.ID,
                        currentPos = idleParam.currentPos,
                        targetPos = idleParam.currentPos,
                        speed = self.Stat.MoveSpeed,
                        timestamp = idleParam.timestamp,
                    };

                    self.Room.Broadcast(locationBroadcast);
                }
            }

            public void Update(PlayerEntity self, in IStateParam param = default)
            {

            }

            public void Exit(PlayerEntity self, in IStateParam param = default)
            {

            }
        }

        public class MoveState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity self, in IStateParam param = default)
            {
                var moveParam = (MoveParam)param;
                UpdateMove(self, moveParam);
            }

            public void Update(PlayerEntity self, in IStateParam param = default)
            {
                var moveParam = (MoveParam)param;
                UpdateMove(self, moveParam);
            }

            public void Exit(PlayerEntity self, in IStateParam param = default)
            {
                Logger.Instance.Debug("Move Exit");
            }

            private void UpdateMove(PlayerEntity self, MoveParam param)
            {
                var updateBroadcast = new UpdateLocationBroadcast()
                {
                    id = self.ID,
                    currentPos = param.currentPos,
                    targetPos = param.targetPos,
                    speed = param.speed,
                    timestamp = param.timestamp,
                };

                self.Room.Broadcast(updateBroadcast);
            }
        }

        public class AttackState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity self, in IStateParam param = default)
            {
                if (param is AttackParam attackParam)
                {
                    var target = attackParam.target;
                    if (target == null)
                    {
                        Logger.Instance.Warn($"Target is null");
                        return;
                    }

                    var damagedVal = BattleCalculator.ComputeDamagedValue(self.Stat, target.Stat); 
                    target.ReduceHP(damagedVal);
                    target.OnDamaged(self);
                }
            }

            public void Update(PlayerEntity self, in IStateParam param = default)
            {
                if (param is AttackParam attackParam)
                {
                    var target = attackParam.target;
                    if (target == null)
                    {
                        Logger.Instance.Warn($"Target is null");
                        return;
                    }

                    var damagedVal = BattleCalculator.ComputeDamagedValue(self.Stat, target.Stat);
                    target.ReduceHP(damagedVal);
                    target.OnDamaged(self);
                }
            }

            public void Exit(PlayerEntity self, in IStateParam param = default)
            {

            }
        }

        public class UseSkillState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity self, in IStateParam param = default)
            {

            }

            public void Update(PlayerEntity self, in IStateParam param = default)
            {

            }

            public void Exit(PlayerEntity self, in IStateParam param = default)
            {

            }
        }

        public class DeadState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity self, in IStateParam param = default)
            {
                if (param is DeadParam deadParam)
                {
                    S_Dead deadPacket = new S_Dead()
                    {
                        id = self.ID,
                        fromId = deadParam.killerTarget.ID,
                        respawnTime = DEFINE.MONSTER_RESPAWN_SEC,
                    };
                    self.Room.Broadcast(deadPacket);

                    self.RegistRespawnTimer();
                }
            }

            public void Update(PlayerEntity self, in IStateParam param = default)
            {

            }

            public void Exit(PlayerEntity self, in IStateParam param = default)
            {

            }

        }
    }
}
