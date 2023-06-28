using System;
using System.Collections.Generic;
using System.Text;


static class DEFINE
{
    //Util
    public const float MS_TO_SEC = 0.001f;
    public const int SEC_TO_MS = 1000;

    //System
    public const int SEND_TICK_MS = 125;
    public const int TIMER_SEQ_MAX_PER_GAME = 300;
    public const int MONSTER_ID = 1000;

    //ResponseResult
    public const int SUCCESS = 0;
    public const int ERROR = 1;

    //Player
    public const int PLAYER_RESPAWN_MS = 15000;

    //Monster
    public const int AI_CHECK_MS = 50;
    public const int MONSTER_RESPAWN_SEC = 15;
    public const int MONSTER_RESPAWN_AREA_RANGE = 10;

    //Game
    public const int PLAYER_MAX_COUNT = 4;
    public const int START_COUNTDOWN_SEC = 10;
}


public enum GameModeStatus
{
    None,
    Ready,
    Countdown,
    GameStart,
    RunningGame,
    FinishGame,
}

public enum EntityStatus
{
    Idle,
    Move,
    Attack,
    UseSkill,
    Dead,
}

public enum AIMode
{
    Free,
    ReturnToRespawnArea,
}

public enum WorldMapSpawnType
{
    Player,
    Monster,
    Item,
}

public enum RewardType
{
    Gold = 1,
    Item = 2,
}

public enum RewardState
{
    Drop,
    Pick,
}

public enum StatType
{
    Str,
    Def,
    Hp,
}