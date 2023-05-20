using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class GameMode
{
    private static GameMode _instance = new GameMode();
    public static GameMode Instance { get => _instance; }

    public Dictionary<int, PlayerEntity> PlayerEntitysDic { get; private set; } = new Dictionary<int, PlayerEntity>();

    private SimpleTaskCompletionSource<S_JoinToGame> _joinCompleted = new SimpleTaskCompletionSource<S_JoinToGame>();

    private Action<PlayerEntity> _onRecvJoinCB;
    private Action<PlayerEntity> _onRecvSelectCB;
    private Action<int> _onRecvLeaveCB;

    public PlayerEntity EntitySelf { get; private set;}


    public void AddPlayerEntity(PlayerEntity inEntity)
    {
        if (PlayerEntitysDic.ContainsKey(inEntity.playerId) == false)
            PlayerEntitysDic.Add(inEntity.playerId, inEntity);
    }

    public void RemovePlayerEntity(int inPlayerId)
    {
        if (PlayerEntitysDic.ContainsKey(inPlayerId) == false)
            PlayerEntitysDic.Remove(inPlayerId);
    }

    public void AddEvent(Action<PlayerEntity> inRecvJoinOther,
                         Action<int> inRecvOther,
                         Action<PlayerEntity> inRecvSelect)
    {
        _onRecvJoinCB = inRecvJoinOther;
        _onRecvLeaveCB = inRecvOther;
        _onRecvSelectCB = inRecvSelect;
    }


    //---------------
    // 서버 관련
    //---------------

    public async Task<bool> JoinAsync()
    {
        C_JoinToGame joinToGame = new C_JoinToGame();
        joinToGame.userId = ServerData.Instance.UserData.user_id;
        joinToGame.userName = ServerData.Instance.UserData.nickname;

        NetworkManager.Instance.Send(joinToGame);

        var waitResult = await _joinCompleted.Wait(10000);

        if (waitResult.isSuccess == false)
            return false;

        var joinPlayerList = waitResult.result.joinPlayerList;

        foreach (var joinPlayer in joinPlayerList)
        {
            PlayerEntity entity = new PlayerEntity()
            {
                userId = joinPlayer.userId,
                userName = joinPlayer.userName,
                playerId = joinPlayer.playerId,
                heroId = joinPlayer.heroId,
            };

            // 내 자신일 경우 따 갖고 있는다.
            if(joinPlayer.userId == ServerData.Instance.UserData.user_id)
                EntitySelf = entity;

            PlayerEntitysDic.Add(joinPlayer.playerId, entity);
        }

        return true;
    }

    public void OnRecvJoin(S_JoinToGame inPacket)
    {
        _joinCompleted.Signal(inPacket);
    }

    public void OnRecvAddJoinOther(S_JoinOther inPacket)
    {
        PlayerEntity entity = new PlayerEntity()
        {
            userId = inPacket.userId,
            userName = inPacket.userName,
            playerId = inPacket.plaeyrId,
            heroId = inPacket.heroId,
        };

        AddPlayerEntity(entity);
        _onRecvJoinCB?.Invoke(entity);
    }

    public void OnRecvLeaveOther(S_LeaveOther inPacket)
    {
        RemovePlayerEntity(inPacket.playerId);
        _onRecvLeaveCB?.Invoke(inPacket.playerId);
    }

    public void OnRecvSelectHero(SelectHero inPacket)
    {
        if (PlayerEntitysDic.TryGetValue(inPacket.playerId, out var entity) == true)
        {
            entity.heroId = inPacket.heroId;
            _onRecvSelectCB?.Invoke(entity);
        }
    }

    public void OnSendSelectHero(int inPlayerId, int inHeroId)
    {
        SelectHero packet = new SelectHero()
        {
            playerId = inPlayerId,
            heroId = inHeroId,
        };

        NetworkManager.Instance.Send(packet);
    }
}


public class PlayerEntity
{
    public string userId;
    public string userName;
    public int playerId;
    public int heroId;
}