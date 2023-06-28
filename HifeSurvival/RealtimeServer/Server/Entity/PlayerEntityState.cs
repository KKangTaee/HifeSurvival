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
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is IdleParam idleParam)
                {
                    UpdateLocationBroadcast broadcast = new UpdateLocationBroadcast()
                    {
                        id = inSelf.id,
                        currentPos = idleParam.currentPos,
                        targetPos = idleParam.currentPos,
                        speed = inSelf.stat.moveSpeed,
                        timestamp = idleParam.timestamp,
                    };

                    inSelf.broadcaster.Broadcast(broadcast);
                }
            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class MoveState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                var param = (MoveParam)inParam;
                updateMove(inSelf, param);
            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                var param = (MoveParam)inParam;
                updateMove(inSelf, param);
            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                Logger.GetInstance().Debug("Move Exit");
            }

            private void updateMove(PlayerEntity inSelf, MoveParam inParam)
            {
                UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                {
                    id = inSelf.id,
                    currentPos = inParam.currentPos,
                    targetPos = inParam.targetPos,
                    speed = inParam.speed,
                    timestamp = inParam.timestamp,
                };

                inSelf.broadcaster.Broadcast(move);
            }
        }

        public class AttackState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is AttackParam attackParam)
                {
                    var target = attackParam.target;
                    if (target == null)
                    {
                        Logger.GetInstance().Warn($"Target is null");
                        return;
                    }

                    var damagedVal = BattleCalculator.ComputeDamagedValue(inSelf.stat, target.stat); 
                    target.ReduceHP(damagedVal);
                    target.OnDamaged(inSelf);
                }
            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is AttackParam attackParam)
                {
                    var target = attackParam.target;
                    if (target == null)
                    {
                        Logger.GetInstance().Warn($"Target is null");
                        return;
                    }

                    var damagedVal = BattleCalculator.ComputeDamagedValue(inSelf.stat, target.stat);
                    target.ReduceHP(damagedVal);
                    target.OnDamaged(inSelf);
                }
            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class UseSkillState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class DeadState : IState<PlayerEntity, IStateParam>
        {
            public void Enter(PlayerEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is DeadParam deadParam)
                {
                    S_Dead deadPacket = new S_Dead()
                    {
                        id = inSelf.id,
                        fromId = deadParam.killerTarget.id,
                        respawnTime = DEFINE.MONSTER_RESPAWN_SEC,
                    };
                    inSelf.broadcaster.Broadcast(deadPacket);

                    inSelf.RegistRespawnTimer();
                }
            }

            public void Update(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }
            public void Exit(PlayerEntity inSelf, in IStateParam inParam = default)
            {

            }

        }
    }
}
