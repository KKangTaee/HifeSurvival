
namespace Server
{
    public class CheatExecuter
    {
        private PlayerEntity _player;
        private GameRoom _room;

        public CheatExecuter(GameRoom room, PlayerEntity player)
        {
            _room = room;
            _player = player;
        }

        public bool Execute(CheatRequest req)
        {
            bool isSuccess = true;
            switch (req.type)
            {
                case "equipitem":
                    {
                        isSuccess &= GameData.Instance.ItemDict.TryGetValue(req.arg1, out var itemdata);
                        if (isSuccess)
                        {
                            isSuccess &= (_player.EquipItem(itemdata) >= 0);
                        }
                    }
                    break;
                case "dropitem":
                    {
                        var dropPos = _player.currentPos.AddPVec3(new PVec3() { y = 2.0f });
                        int itemKey = req.arg1;
                        if (itemKey == 0)
                        {
                            itemKey = 1;
                        }

                        int itemCount = req.arg2;
                        if (itemCount == 0)
                        {
                            itemCount = 1;
                        }

                        for (int i = 0; i < itemCount; i++)
                        {
                            float fixedX = i  * 1.25f;
                            PVec3 fixedPos;
                            if( i == 0)
                            {
                                fixedPos = dropPos;
                            }
                            else if (i % 2 == 0)
                            {
                                fixedPos = dropPos.AddPVec3(new PVec3() { x =  fixedX });
                            }
                            else
                            {
                                fixedPos = dropPos.AddPVec3(new PVec3() { x = -fixedX });
                            }

                            var broadcast = _room.DropItem($"2:{itemKey}:100", fixedPos);
                            if (broadcast == null)
                            {
                                Logger.Instance.Warn("Reward Drop Failed");
                            }
                            else
                            {
                                Logger.Instance.Debug($"Reward Drop worldid : {broadcast.worldId}, pos : {broadcast.pos.Print()}");
                                _room.Broadcast(broadcast);
                            }
                        }
                    }
                    break;
                default:
                    Logger.Instance.Warn("invalid cheat type");
                    isSuccess = false;
                    break;
            }

            return isSuccess;
        }
    }
}
