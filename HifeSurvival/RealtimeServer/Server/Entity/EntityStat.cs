using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class EntityStat
    {
        public int str { get; private set; } = 0;
        public int def { get; private set; } = 0;

        public int maxHp { get; private set; } = 0;
        public int curHp { get; private set; } = 0;

        public float detectRange { get; private set; } = 0f;
        public float attackRange { get; private set; } = 0f;
        public float moveSpeed { get; private set; } = 0f;
        public float attackSpeed { get; private set; } = 0f;

        public EntityStat() { }
        public EntityStat(GameDataLoader.Heros heros)
        {
            str = heros.str;
            def = heros.def;
            curHp = maxHp = heros.hp;
            detectRange = heros.detectRange;
            attackRange = heros.attackRange;
            moveSpeed = heros.moveSpeed;
            attackSpeed = heros.attackSpeed;
        }

        public EntityStat(GameDataLoader.Monsters monsters)
        {
            str = monsters.str;
            def = monsters.def;
            curHp = maxHp = monsters.hp;
            detectRange = monsters.detectRange;
            attackRange = monsters.attackRange;
            moveSpeed = monsters.moveSpeed;
            attackSpeed = monsters.attackSpeed;
        }

        //스탯이 관리하기 충분하여 수동으로 처리했으나, 많은 양의 스탯이 추가될 경우 Generic과 Macro 를 활용해 다시 구현 할 수 있음.
        public static EntityStat operator +(EntityStat a, EntityStat b)
        {
            var resultStat = new EntityStat();

            resultStat.str = a.str + b.str;
            resultStat.def = a.def + b.def;
            resultStat.maxHp = a.maxHp + b.maxHp;
            resultStat.curHp = a.curHp + b.curHp;

            resultStat.detectRange = a.detectRange + b.detectRange;
            resultStat.attackRange = a.attackRange + b.attackRange;
            resultStat.moveSpeed = a.moveSpeed + b.moveSpeed;
            resultStat.attackSpeed = a.attackSpeed + b.attackSpeed;     //증가할 수록 빨라진다고 가정하고.. 

            return resultStat;
        }


        public void AddStr(int inStr) =>
            str += inStr;

        public void AddDef(int inDef) =>
            def += inDef;

        public void AddMaxHp(int inHp, bool currHp = false)
        {
            maxHp += inHp;
            if (currHp)
                AddCurrHp(inHp);
        }

        public void AddCurrHp(int inHp) =>
            curHp += inHp;
    }
}
