
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
            bool isSuccess = true;
            switch(req.type)
            {
                case "equipitem":
                    {
                        isSuccess &= GameData.Instance.ItemDict.TryGetValue(req.arg1, out var itemdata);
                        if(isSuccess)
                        {
                            isSuccess &= (_player.EquipItem(itemdata) >= 0);
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
