
static class DEFINE
{
    //Util
    public const float MS_TO_SEC = 0.001f;
    public const int SEC_TO_MS = 1000;

    //System
    public const int SERVER_PORT = 7777;
    public const int MAIN_THREAD_STACK_SIZE = 4 * 1024 * 1024;
    public const int SERVER_TICK = 50;
    public const int TIMER_SEQ_MAX_PER_GAME = 300;
    public const int PC_BEGIN_ID = 10000;
    public const float POS_EPSILON = 0.25f;

    //ResponseResult
    public const int SUCCESS = 0;
    public const int ERROR = 1;

    //Player
    public const int PLAYER_RESPAWN_SEC = 15;
    public const int PLAYER_ITEM_SLOT = 4;

    //Monster
    public const int MONSTER_RESPAWN_AREA_RANGE = 10;
    public const int MONSTER_ATTACK_ANIM_TIME = 300;   //TODO : (임시) 유니티에서 추출해야할 anim 데이터 읽어야 할 듯. -> entity 가 들고 있어야 함. 

    //Game
    public const int PLAYER_MAX_COUNT = 4;
    public const int START_COUNTDOWN_SEC = 5;
    public const int SPAWN_PHASE_MAX = 4;

    //Item
    public const int MAX_ITEM_LEVEL = 4;
    public const int GOLD_WHEN_MAX_ITEM_LEVEL = 50;
}


public enum EGameRoomStatus
{
    NONE,
    READY,
    COUNT_DOWN,
    LOAD_GAME,
    PLAY_START,
    PLAY_FINISH,
    FINISH_GAME,
    REALEASED_ROOM,
}

public enum EClientStatus
{
    NONE,
    ENTERED_ROOM,
    SELECT_READY,
    PLAY_READY,
    PLAYING,
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
    NONE,
    GOLD = 1,
    ITEM = 2,
}

public enum ERewardState
{
    NONE,
    DROP,
    PICK,
}

public enum EStatType
{
    NONE,
    STR,
    DEF,
    HP,
}

public enum ESkillSort
{
    NONE,
    DEALT,
    HEAL,
    BUFF,
    DEBUFF,
}

public enum ECurrency
{
    NONE,
    GOLD,
}

public enum EMonsterGrade
{
    NONE,
    NORMAL,
    ELITE,
    BOSS,

}