using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    public struct DropItem
    {
        public int type; // 1 : gold, 2 : item
        public int amount;
        public int key;
    }

    public class GameRoom
    {
        public Dictionary<int, DropItem> DropDict = new Dictionary<int, DropItem>();
    }
}
