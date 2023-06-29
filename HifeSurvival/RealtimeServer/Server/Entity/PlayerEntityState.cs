using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

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
                    UpdateLocationBroadcast broadcast = new UpdateLocationBroadcast()
                    {
                        id = self.id,
                        currentPos = idleParam.currentPos,
                        targetPos = idleParam.currentPos,
                        speed = self.stat.MoveSpeed,
                        timestamp = idleParam.timestamp,
                    };

                    self.broadcaster.Broadcast(broadcast);
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
                updateMove(self, moveParam);
            }

            public void Update(PlayerEntity self, in IStateParam param = default)
            {
                var moveParam = (MoveParam)param;
                updateMove(self, moveParam);
            }

            public void Exit(PlayerEntity self, in IStateParam param = default)
            {
                Logger.GetInstance().Debug("Move Exit");
            }

            private void updateMove(PlayerEntity self, MoveParam param)
            {
                UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                {
                    id = self.id,
                    currentPos = param.currentPos,
                    targetPos = param.targetPos,
                    speed = param.speed,
                    timestamp = param.timestamp,
                };

                self.broadcaster.Broadcast(move);
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
                        Logger.GetInstance().Warn($"Target is null");
                        return;
                    }

                    var damagedVal = BattleCalculator.ComputeDamagedValue(self.stat, target.stat); 
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
                        Logger.GetInstance().Warn($"Target is null");
                        return;
                    }

                    var damagedVal = BattleCalculator.ComputeDamagedValue(self.stat, target.stat);
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
                        id = self.id,
                        fromId = deadParam.killerTarget.id,
                        respawnTime = DEFINE.MONSTER_RESPAWN_SEC,
                    };
                    self.broadcaster.Broadcast(deadPacket);

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
