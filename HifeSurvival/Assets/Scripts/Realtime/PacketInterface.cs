using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//-----------------
// Response
//-----------------

public interface IResponsePickReward
{
    public void OnResponsePickReward(PickRewardResponse packet);
}

public interface IResponseIncreaseStat
{
    public void OnResponseIncreaseStat(IncreaseStatResponse packet);
}



//-------------------
// Update Broadcast
//-------------------

public interface IUpdateDeadBroadcast
{
    public void OnUpdateDeadBroadcast(S_Dead packet);
}

public interface IUpdateAttackBroadcast
{
    public void OnUpdateAttackBroadcast(CS_Attack packet);
}

public interface IUpdateRewardBroadcast
{
    public void OnUpdateRewardBroadcast(UpdateRewardBroadcast packet);
}

public interface IUpdateLocationBroadcast
{
    public void OnUpdateLocationBroadcast(UpdateLocationBroadcast packet);
}

public interface IUpdateGameModeStatusBroadcast
{
    void OnUpdateGameModeStatusBroadcast(UpdateGameModeStatusBroadcast packet);
}

public interface IUpdateStatBroadcast
{
    void OnUpdateStatBroadcast(UpdateStatBroadcast packet);
}

public interface IUpdateSpawnMonsterBroadcast
{
    void OnUpdateSpwanMonsterBroadcast(UpdateSpawnMonsterBroadcast packet);   
}

public interface IUpdateRespawn
{
    void OnUpdateRespawnBroadcast(S_Respawn packet);
}


public interface IUpdateSelectHero
{
    void OnUpdateSelectHeroBroadcast(CS_SelectHero packet);
}

public interface IUpdateReadyToGame
{
    void UpdateReadyToGameBroadcast(CS_ReadyToGame packet);
}

public interface IUpdateStartGame
{
    void UpdateStartGameBroadcast(S_StartGame packet);
}



//------------------
// Update Single
//------------------

public interface IUpdateInvenItemSingle
{
    void OnUpdateInvenItemSingle(UpdateInvenItem packet);
}

public interface IUpdatePlayerCurrencySingle
{
    void OnUpdatePlayerCurrencySingle(UpdatePlayerCurrency packet);
}