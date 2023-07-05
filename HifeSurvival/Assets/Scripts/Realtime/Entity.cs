using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public abstract class Entity
{
    public enum EEntityType
    {
        NONE,
        
        PLAYER,
        MOSNTER,
    }

    public int id;

    public EntityStat stat;

    public PVec3 pos;
    public PVec3 dir;

    public int GetAttackValue()
    {
        var str = GetTotalStr();

        return (int)UnityEngine.Random.Range(str * 0.8f, str * 1.3f);
    }

    public int GetDamagedValue(int inAttackValue)
    {
        var def = GetTotalDef();

        return (int)(inAttackValue - UnityEngine.Random.Range(def * 0.2f, def * 0.4f));
    }

    public virtual int GetTotalStr()
    {
        return stat.str;
    }

    public virtual int GetTotalDef()
    {
        return stat.def;
    }

    public virtual int GetTotalHp()
    {
        return stat.hp;
    }


    //-----------------
    // static
    //-----------------

    public const int PLAYER_ID_MAX = 100;
    public const int MONSTER_ID_MAX = 10000;

    public static EEntityType GetEntityType(int inId)
    {
        if (inId < PLAYER_ID_MAX)
            return EEntityType.PLAYER;
        else if (inId < MONSTER_ID_MAX)
            return EEntityType.MOSNTER;

        return EEntityType.NONE;
    }
}


public class PlayerEntity : Entity
{
    public string userId;
    public string userName;

    public int heroId;
    public int gold;

    public bool isSelf;
    public bool isReady;

    public EntityItem[] itemSlot = new EntityItem[4];

    public override int GetTotalDef()
    {
        return base.GetTotalDef() + itemSlot.Sum(x => x == null ? 0 : x.Def);
    }

    public override int GetTotalHp()
    {
        return base.GetTotalHp() + itemSlot.Sum(x => x == null ? 0 : x.Hp);
    }

    public override int GetTotalStr()
    {
        return base.GetTotalStr() + itemSlot.Sum(x => x == null ? 0 : x.Str);
    }

    public void AddGold(int gold)
    {
        this.gold += gold;
    }
}


public class MonsterEntity : Entity
{
    public int monsterId;
    public int grade;
}

public class EntityStat
{
    public int str { get; private set; }
    public int def { get; private set; }
    public int hp { get; private set; }
    public int currHP { get; private set; }
    public float detectRange { get; private set; }
    public float attackRange { get; private set; }
    public float bodyRange { get; private set; }
    public float moveSpeed { get; private set; }
    public float attackSpeed { get; private set; }


    public EntityStat(PStat stat)
    {
        str = stat.str;
        def = stat.def;

        currHP = hp = stat.hp;

        detectRange = stat.detectRange;
        attackRange = stat.attackRange;
        moveSpeed = stat.moveSpeed;
        attackSpeed = stat.attackSpeed;
        bodyRange = stat.bodyRange;
    }

    public void AddStr(int str)
    {
        this.str += str;
    }

    public void AddDef(int def)
    {
        this.def += def;
    }

    public void AddHp(int hp)
    {
        this.hp += hp;
    }

    public void AddCurrHp(int hp)
    {
        currHP += hp;
    }

    public void IncreaseStat(PStat pStat)
    {
        str += pStat.str;
        def += pStat.def;
        hp += pStat.hp;
        currHP += pStat.hp;
    }

    public void IncreaseStat(EStatType statType, int val)
    {
        switch (statType)
        {
            case EStatType.STR:
                AddStr(val);
                break;

            case EStatType.DEF:
                AddDef(val);
                break;

            case EStatType.HP:
                AddHp(val);
                AddCurrHp(val);
                break;
        }
    }
}


public class EntityItem
{
    public int Slot { get; private set; }
    public int ItemKey { get; private set; }
    public int MaxStack { get; private set; }
    public int CurrentStack { get; private set; }
    public int ItemLevel { get; private set; }
    public int Str { get; private set; }
    public int Def { get; private set; }
    public int Hp { get; private set; }
    public EntitySkill Skill { get; private set; }


    public EntityItem(PInvenItem invenItem)
    {
        Slot = invenItem.slot;
        ItemKey = invenItem.itemKey;
        MaxStack = invenItem.maxStack;
        CurrentStack =invenItem.currentStack;
        ItemLevel = invenItem.itemLevel;
        Str = invenItem.str;
        Def = invenItem.def;
        Hp = invenItem.hp;
        Skill = new EntitySkill(invenItem.skill);
    }
}

public class EntitySkill
{
    public int SkillKey { get; private set; }
    public int Sort { get; private set; }
    public int Cooltime { get; private set; }

    public EntitySkill(PItemSkill itemSkill)
    {
        SkillKey = itemSkill.skillKey;
        Sort = itemSkill.sort;
        Cooltime = itemSkill.coolTime;
    }
}