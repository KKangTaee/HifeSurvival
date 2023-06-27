using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class EntityStat
    {
        public int str { get; private set; }
        public int def { get; private set; }

        public int hp { get; private set; }
        public int currHp { get; private set; }

        public float detectRange { get; private set; }
        public float attackRange { get; private set; }
        public float moveSpeed { get; private set; }
        public float attackSpeed { get; private set; }


        public EntityStat(GameDataLoader.Heros heros)
        {
            str = heros.str;
            def = heros.def;
            currHp = hp = heros.hp;
            detectRange = heros.detectRange;
            attackRange = heros.attackRange;
            moveSpeed = heros.moveSpeed;
            attackSpeed = heros.attackSpeed;
        }

        public EntityStat(GameDataLoader.Monsters monsters)
        {
            str = monsters.str;
            def = monsters.def;
            currHp = hp = monsters.hp;
            detectRange = monsters.detectRange;
            attackRange = monsters.attackRange;
            moveSpeed = monsters.moveSpeed;
            attackSpeed = monsters.attackSpeed;
        }

        public void AddStr(int inStr) =>
            str += inStr;

        public void AddDef(int inDef) =>
            def += inDef;

        public void AddMaxHp(int inHp) =>
            hp += inHp;

        public void AddCurrHp(int inHp) =>
            currHp += inHp;
    }
}
