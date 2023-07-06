
namespace Server
{
    public class CheatExecuter
    {
        private PlayerEntity _player;

        public CheatExecuter(PlayerEntity player)
        {
            _player = player;
        }

        public bool Execute(CheatRequest req)
        {
            bool bSuccess = true;
            switch(req.type)
            {
                case "equipitem":
                    {
                        bSuccess &= GameData.Instance.ItemDict.TryGetValue(req.arg1, out var itemdata);
                        if(bSuccess)
                        {
                            bSuccess &= (_player.EquipItem(itemdata) >= 0);
                        }
                    }
                    break;
                default:
                    Logger.Instance.Warn("invalid cheat type");
                    bSuccess = false;
                    break;
            }

            return bSuccess;
        }
    }
}
