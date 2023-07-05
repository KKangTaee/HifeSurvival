using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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