using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Entity
{

}

public class PlayerEntity : Entity
{
    public string userId;
    public string userName;
    public int    playerId;
    public int    heroId;

    public bool isReady;
    public bool isAlive;

    public EntityStat stat;

    public Vec3 pos;
    public Vec3 dir;
}


public class EntityStat
{
    public int str { get; private set; }
    public int def { get; private set; }
    public int hp  { get; private set; }

    public float detectRange { get; private set; }
    public float attackRange { get; private set; }
    public float moveSpeed   { get; private set; }
    public float attackSpeed { get; private set; }


    public EntityStat(StaticData.Heros heros)
    {
        str = heros.str;
        def = heros.def;
        hp  = heros.hp;
        detectRange = heros.detectRange;
        attackRange = heros.attackRange;
        moveSpeed   = heros.moveSpeed;
        attackSpeed = heros.attackSpeed;
    }


    public int GetAttackValue() =>
       (int)UnityEngine.Random.Range(str - 15, str + str * 0.2f);


    public int GetDamagedValue(int inAttackValue) =>
       (int)(inAttackValue - def * 0.1f);


    public void AddStr(int inStr) =>
        str += inStr;

    public void AddDef(int inDef) =>
        def += inDef;

    public void AddHp(int inHp) =>
        hp += inHp;
}