using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class EntityStat
    {
        public int Str { get; private set; } = 0;
        public int Def { get; private set; } = 0;

        public int MaxHp { get; private set; } = 0;
        public int CurHp { get; private set; } = 0;

        public float DetectRange { get; private set; } = 0f;
        public float AttackRange { get; private set; } = 0f;
        public float MoveSpeed { get; private set; } = 0f;
        public float AttackSpeed { get; private set; } = 0f;

        public EntityStat() { }
        public EntityStat(GameDataLoader.Heros heros)
        {
            Str = heros.str;
            Def = heros.def;
            CurHp = MaxHp = heros.hp;
            DetectRange = heros.detectRange;
            AttackRange = heros.attackRange;
            MoveSpeed = heros.moveSpeed;
            AttackSpeed = heros.attackSpeed;
        }

        public EntityStat(GameDataLoader.Monsters monsters)
        {
            Str = monsters.str;
            Def = monsters.def;
            CurHp = MaxHp = monsters.hp;
            DetectRange = monsters.detectRange;
            AttackRange = monsters.attackRange;
            MoveSpeed = monsters.moveSpeed;
            AttackSpeed = monsters.attackSpeed;
        }

        public EntityStat(in PItem item)
        {
            Str = item.str;
            Def = item.def;
            MaxHp = item.hp;
        }

        //스탯이 관리하기 충분하여 수동으로 처리했으나, 많은 양의 스탯이 추가될 경우 Generic과 Macro 를 활용해 다시 구현 할 수 있음.
        public static EntityStat operator +(EntityStat a, EntityStat b)
        {
            var resultStat = new EntityStat();

            resultStat.Str = a.Str + b.Str;
            resultStat.Def = a.Def + b.Def;
            resultStat.MaxHp = a.MaxHp + b.MaxHp;
            resultStat.CurHp = a.CurHp + b.CurHp;

            resultStat.DetectRange = a.DetectRange + b.DetectRange;
            resultStat.AttackRange = a.AttackRange + b.AttackRange;
            resultStat.MoveSpeed = a.MoveSpeed + b.MoveSpeed;
            resultStat.AttackSpeed = a.AttackSpeed + b.AttackSpeed;     //증가할 수록 빨라진다고 가정하고.. 

            return resultStat;
        }


        public void AddStr(int inStr) =>
            Str += inStr;

        public void AddDef(int inDef) =>
            Def += inDef;

        public void AddMaxHp(int inHp, bool currHp = false)
        {
            MaxHp += inHp;
            if (currHp)
                AddCurrHp(inHp);
        }

        public void AddCurrHp(int inHp) =>
            CurHp += inHp;
    }
}
