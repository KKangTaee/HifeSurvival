using System;


namespace GameDataAO
{
    [Serializable]
    public class Systems
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class Heros
    {
        public int key;
        public string name;
        public int str;
        public int def;
        public int hp;
        public float attackSpeed;
        public float moveSpeed;
        public float attackRange;
        public float detectRange;
        public float bodyRange;
        public string desc;
    }

    [Serializable]
    public class Monsters
    {
        public int key;
        public string name;
        public int grade;
        public int str;
        public int def;
        public int hp;
        public float attackSpeed;
        public float moveSpeed;
        public float attackRange;
        public float detectRange;
        public float bodyRange;
        public string rewardIds;
        public string desc;
    }

    [Serializable]
    public class MonstersGroup
    {
        public int key;
        public int groupId;
        public string monsterGroups;
        public int respawnTime;
        public int enabled;
    }

    [Serializable]
    public class Item
    {
        public int key;
        public string name;
        public int grade;
    }

    [Serializable]
    public class ItemUpgrade
    {
        public int key;
        public int itemKey;
        public int level;
        public int needStack;
        public int str;
        public int def;
        public int hp;
        public int skillKey;
    }

    [Serializable]
    public class ItemSkill
    {
        public int key;
        public string name;
        public int coolTime;
        public int formulaKey;
    }

    [Serializable]
    public class FormulaData
    {
        public int key;
        public string name;
        public int sort;
        public float ratioStr;
        public float ratioDef;
        public float ratioHp;
    }

    [Serializable]
    public class ChapterData
    {
        public int key;
        public string name;
        public string mapData;
        public string phase1;
        public string phase2;
        public string phase3;
        public string phase4;
        public int playTimeSec;


        public int[] phase1Array;
        public int[] phase2Array;
        public int[] phase3Array;
        public int[] phase4Array;
    }
}

