
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
                    var locationBroadcast = new UpdateLocationBroadcast()
                    {
                        id = self.ID,
                        currentPos = idleParam.currentPos,
                        targetPos = idleParam.currentPos,
                        speed = self.Stat.MoveSpeed,
                        timestamp = idleParam.timestamp,
                    };

                    Logger.Instance.Debug($"IdleState monster : {self.ID}, param current/target Pos : {idleParam.currentPos.Print()}");

                    self.Room.Broadcast(locationBroadcast);
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
                    id = self.ID,
                    currentPos = param.currentPos,
                    targetPos = param.targetPos,
                    speed = param.speed,
                    timestamp = param.timestamp,
                };

                Logger.Instance.Debug($"monster : {self.ID}, param current Pos : {param.currentPos.Print()} , targetPos : {param.targetPos.Print()}");
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
                        id = self.ID,
                        fromId = deadParam.killerTarget.ID,
                    };

                    self.Room.Broadcast(deadPacket);

                    self.DropItem();


                    if (self.Grade == EMonsterGrade.BOSS)
                    {
                        self.Room.Mode.SetWinnerID(deadParam.killerTarget.ID);
                        return;
                    }

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
