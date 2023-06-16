using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public abstract class Entity
{
    public int targetId;

    public EntityStat stat;

    public Vec3 pos;
    public Vec3 dir;

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
}


public class PlayerEntity : Entity
{
    public string userId;
    public string userName;
    
    public int    heroId;
    public int    gold;

    public bool   isSelf;
    public bool   isReady;

    public EntityItem[] itemSlot = new EntityItem[4];

    public override int GetTotalDef()
    {
        return base.GetTotalDef() + itemSlot.Sum(x => x == null ? 0 : x.def);
    }

    public override int GetTotalHp()
    {
        return base.GetTotalHp() + itemSlot.Sum(x => x == null ? 0 : x.hp);
    }

    public override int GetTotalStr()
    {
        return base.GetTotalStr() + itemSlot.Sum(x => x == null ? 0 : x.str);
    }

    public void AddGold(int inGold) =>
        gold += inGold;
}


public class MonsterEntity : Entity
{
    public int monsterId;
    public int grade;
}

public class EntityStat
{
    public int str    { get; private set; }
    public int def    { get; private set; }   
    public int hp     { get; private set; }

    public int currHP { get; private set; }


    public float detectRange { get; private set; }
    public float attackRange { get; private set; }
    public float moveSpeed   { get; private set; }
    public float attackSpeed { get; private set; }


    public EntityStat(StaticData.Heros heros)
    {
        str = heros.str;
        def = heros.def;
        
        currHP = hp  = heros.hp;
        
        detectRange = heros.detectRange;
        attackRange = heros.attackRange;
        moveSpeed   = heros.moveSpeed;
        attackSpeed = heros.attackSpeed;
    }

    public EntityStat(StaticData.Monsters monsters)
    {
        str = monsters.str;
        def = monsters.def;

        currHP = hp = monsters.hp;

        detectRange = monsters.detectRange;
        attackRange = monsters.attackRange;
        moveSpeed   = monsters.moveSpeed;
        attackSpeed = monsters.attackSpeed;
    }

    public void AddStr(int inStr) =>
        str += inStr;

    public void AddDef(int inDef) =>
        def += inDef;

    public void AddCurrHp(int inHp) =>
        currHP += inHp;

    public void IncreaseStat(Stat inStat)
    {
        str += inStat.str;
        def += inStat.def;
        hp  += inStat.hp;
        currHP += inStat.hp;
    }
}


public class EntityItem
{
    public int slotId;

    public int itemKey_static;
    public int level;
    public int str;
    public int def;
    public int hp;
    public int cooltime;
    public bool canUse;

    public EntityItem(Item inItem)
    {
        itemKey_static = inItem.itemKey;
        level = inItem.level;
        str = inItem.str;
        def = inItem.def;
        hp = inItem.hp;
        cooltime = inItem.cooltime;
        canUse = inItem.canUse;
    }
}