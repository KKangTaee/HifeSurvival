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
                if(inParam is IdleParam idleParam)
                {
                    UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                    {
                        id = inSelf.id,
                        currentPos = idleParam.currentPos,
                        targetPos = idleParam.currentPos,
                        speed = inSelf.stat.moveSpeed,
                        timestamp = idleParam.timestamp,
                    };

                    inSelf.broadcaster.Broadcast(move);
                }
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class MoveState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is MoveParam moveParam)
                {
                    updateMove(inSelf, moveParam);
                }
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is MoveParam moveParam)
                {
                    updateMove(inSelf, moveParam);
                }
            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            private void updateMove(MonsterEntity inSelf, MoveParam inParam)
            {
                inSelf.AIController.UpdateNextMove(inParam);

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

        public class AttackState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if(inParam is AttackParam attackParam)
                {
                    inSelf.AIController.UpdateAggro(attackParam.target);
                }
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class UseSkillState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }

        public class DeadState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity inSelf, in IStateParam inParam = default)
            {
                if (inParam is DeadParam deadParam)
                {
                    inSelf.AIController.Clear();

                    S_Dead deadPacket = new S_Dead()
                    {
                        id = inSelf.id,
                        fromId = deadParam.killerTarget.id,
                        respawnTime = DEFINE.MONSTER_RESPAWN_SEC,
                    };
                    inSelf.broadcaster.Broadcast(deadPacket);

                    inSelf.DropItem();

                    if (inSelf.IsGroupAllDead())
                    {
                        inSelf.StartRespawning();
                    }
                }
            }

            public void Update(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }

            public void Exit(MonsterEntity inSelf, in IStateParam inParam = default)
            {

            }
        }
    }
}
