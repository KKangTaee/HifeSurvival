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