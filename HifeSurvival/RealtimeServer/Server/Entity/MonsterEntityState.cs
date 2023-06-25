using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public partial class MonsterEntity
    {
        public class IdleState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                inSelf.AIController.UpdateNextMove(null);
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class AttackState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                inSelf.AIController.AttackRoutine();
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }


            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
            }
        }

        public class DeadState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is DeadParam deadParam)
                {
                    inSelf.AIController.OnMonsterDead();

                    S_Dead deadPacket = new S_Dead()
                    {
                        toIsPlayer = true,
                        toId = inSelf.targetId,
                        fromIsPlayer = false,
                        fromId = deadParam.killerTarget.targetId,
                        respawnTime = 15,
                    };
                    inSelf.broadcaster.Broadcast(deadPacket);

                    inSelf.DropItem();

                    if (inSelf.IsGroupAllDead())
                    {
                        inSelf.StartRespawning();
                    }
                }
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class MoveState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                Logger.GetInstance().Debug("UpdateMove");
                var param = (MoveParam)inParam;
                updateMove(inSelf, param);
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                Logger.GetInstance().Debug("UpdateMove");
                var param = (MoveParam)inParam;
                updateMove(inSelf, param);
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            private void updateMove(MonsterEntity inSelf, MoveParam inParam)
            {
                inSelf.AIController.UpdateNextMove(inParam);
                inSelf.AIController.MoveRoutine();

                UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                {
                    targetId = inSelf.targetId,
                    isPlayer = inSelf.IsPlayer,
                    currentPos = inParam.currentPos,
                    targetPos = inParam.targetPos,
                    speed = inParam.speed,
                    timestamp = inParam.timestamp,
                };

                inSelf.broadcaster.Broadcast(move);
            }
        }

        public class UseSkillState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }
    }
}
