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
            public void Enter(MonsterEntity self, in IStateParam param = default)
            {
                if(param is IdleParam idleParam)
                {
                    UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                    {
                        id = self.id,
                        currentPos = idleParam.currentPos,
                        targetPos = idleParam.currentPos,
                        speed = self.Stat.MoveSpeed,
                        timestamp = idleParam.timestamp,
                    };

                    Logger.GetInstance().Debug($"IdleState monster : {self.id}, param current/target Pos : {idleParam.currentPos.Print()}");

                    self.Room.Broadcast(move);
                }
            }

            public void Update(MonsterEntity self, in IStateParam param = default)
            {

            }

            public void Exit(MonsterEntity self, in IStateParam param = default)
            {

            }
        }

        public class MoveState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity self, in IStateParam param = default)
            {
                if (param is MoveParam moveParam)
                {
                    UpdateMove(self, moveParam);
                }
            }

            public void Update(MonsterEntity self, in IStateParam param = default)
            {
                if (param is MoveParam moveParam)
                {
                    UpdateMove(self, moveParam);
                }
            }

            public void Exit(MonsterEntity self, in IStateParam param = default)
            {

            }

            private void UpdateMove(MonsterEntity self, MoveParam param)
            {
                self.AIController.UpdateNextMove(param);

                UpdateLocationBroadcast move = new UpdateLocationBroadcast()
                {
                    id = self.id,
                    currentPos = param.currentPos,
                    targetPos = param.targetPos,
                    speed = param.speed,
                    timestamp = param.timestamp,
                };

                Logger.GetInstance().Debug($"monster : {self.id}, param current Pos : {param.currentPos.Print()} , targetPos : {param.targetPos.Print()}");
                self.Room.Broadcast(move);
            }
        }

        public class AttackState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity self, in IStateParam param = default)
            {
                if(param is AttackParam attackParam)
                {
                    self.AIController.UpdateAggro(attackParam.target);
                }
            }

            public void Update(MonsterEntity self, in IStateParam param = default)
            {

            }

            public void Exit(MonsterEntity self, in IStateParam param = default)
            {

            }
        }

        public class UseSkillState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity self, in IStateParam param = default)
            {

            }

            public void Update(MonsterEntity self, in IStateParam param = default)
            {

            }

            public void Exit(MonsterEntity self, in IStateParam param = default)
            {

            }
        }

        public class DeadState : IState<MonsterEntity, IStateParam>
        {
            public void Enter(MonsterEntity self, in IStateParam param = default)
            {
                if (param is DeadParam deadParam)
                {
                    self.AIController.Clear();

                    S_Dead deadPacket = new S_Dead()
                    {
                        id = self.id,
                        fromId = deadParam.killerTarget.id,
                        respawnTime = DEFINE.MONSTER_RESPAWN_SEC,
                    };

                    self.Room.Broadcast(deadPacket);

                    self.DropItem();

                    if (self.IsGroupAllDead())
                    {
                        self.StartRespawning();
                    }
                }
            }

            public void Update(MonsterEntity self, in IStateParam param = default)
            {

            }

            public void Exit(MonsterEntity self, in IStateParam param = default)
            {

            }
        }
    }
}
