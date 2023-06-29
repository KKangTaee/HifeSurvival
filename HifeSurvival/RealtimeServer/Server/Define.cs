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
    public const int PLAYER_ITEM_SLOT = 4;

    //Monster
    public const int AI_CHECK_MS = 50;
    public const int MONSTER_RESPAWN_SEC = 15;
    public const int MONSTER_RESPAWN_AREA_RANGE = 10;
    public const int MONSTER_ATTACK_ANIM_TIME = 300;   //TODO : (임시) 유니티에서 추출해야할 anim 데이터 읽어야 할 듯. -> entity 가 들고 있어야 함. 

    //Game
    public const int PLAYER_MAX_COUNT = 4;
    public const int START_COUNTDOWN_SEC = 10;
}


public enum EGameModeStatus
{
    NONE,
    READY,
    COUNT_DOWN,
    LOAD_GAME,
    PLAY_START,
    FINISH_GAME,
}

public enum EClientStatus
{
    SELECT_READY,
    PLAY_READY,
}


public enum EEntityStatus
{
    IDLE,
    MOVE,
    ATTACK,
    USESKILL,
    DEAD,
}

public enum EAIMode
{
    FREE,
    RETURN_TO_RESPAWN_AREA,
}

public enum EWorldMapSpawnType
{
    PLAYER,
    MONSTER,
    ITEM,
}

public enum ERewardType
{
    GOLD = 1,
    ITEM = 2,
}

public enum ERewardState
{
    DROP,
    PICK,
}

public enum EStatType
{
    STR,
    DEF,
    HP,
}