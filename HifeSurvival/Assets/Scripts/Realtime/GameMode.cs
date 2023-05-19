using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class GameMode
{
    private static GameMode _instance = new GameMode();
    public static GameMode Instance { get => _instance; }

    private SimpleTaskCompletionSource<S_JoinToGame> _joinCompleted = new SimpleTaskCompletionSource<S_JoinToGame>();

    public Dictionary<int, ServerPlayer> _playerDic = new Dictionary<int, ServerPlayer>();

    public async Task<bool> JoinAsync()
    {
        C_JoinToGame joinToGame = new C_JoinToGame();
        joinToGame.userId   = ServerData.Instance.UserData.user_id;
        joinToGame.userName = ServerData.Instance.UserData.nickname;

        NetworkManager.Instance.Send(joinToGame);
 
        var waitResult = await _joinCompleted.Wait(10000);

        if(waitResult.isSuccess == false)
            return false;

        var joinPlayerList = waitResult.result.joinPlayerList;

        foreach(var joinPlayer in joinPlayerList)
        {
            ServerPlayer player = new ServerPlayer()
            {
                userId = joinPlayer.userId,
                userName = joinPlayer.userName,
                playerId = joinPlayer.playerId,
                heroType = joinPlayer.heroType
            };
        }

        return true;
    }

    public void OnJoinResult(S_JoinToGame inPacket)
    {
        _joinCompleted.Signal(inPacket);
    }

    public void AddPlayer(ServerPlayer inPlayer)
    {
        if(_playerDic.ContainsKey(inPlayer.playerId) == false)
            _playerDic.Add(inPlayer.playerId, inPlayer);
    }

    public void RemovePlayer()
    {

    }

    public Dictionary<int, ServerPlayer> GetPlayerDic() =>
        _playerDic;
}

public class ServerPlayer
{
    public string userId;
    public string userName;
    public int playerId;
    public int heroType;
}