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
    public int playerId;
    public int heroId;
    public bool isReady;

    public Vec3 pos;
    public Vec3 dir;
    public Stat stat;
}

public class Stat
{
    public readonly int maxHP;
    public readonly int maxEXP;
    public readonly int maxSTR;

    public int hp;
    public int exp;
    public int str;
    public int attackRange;
    public float speed;

    public Stat(int inMaxHP, int inMaxEXP, int inMaxSTR, int inAttackRange, float inSpeed)
    {
        hp = maxHP = inMaxHP;
        exp = maxEXP = inMaxEXP;
        str = maxSTR = inMaxSTR;
        attackRange = inAttackRange;
        speed = inSpeed;
    }

    public int GetAttackValue()
    {
        return 100;
    }
}