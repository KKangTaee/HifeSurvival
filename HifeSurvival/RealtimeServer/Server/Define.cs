﻿using System;
using System.Collections.Generic;
using System.Text;


static class DEFINE
{
    //Util
    public const float MS_TO_SEC = 0.001f;
    public const int SEC_TO_MS = 1000;

    //Server
    public const int SEND_TICK_MS = 125;

    //Player
    public const int PLAYER_RESPAWN_MS = 15000;

    //Monster
    public const int AI_CHECK_MS = 50;
    public const int MONSTER_RESPAWN_SEC = 15;

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
    Attack,
    Dead,
    UseSkill,
    Move
}