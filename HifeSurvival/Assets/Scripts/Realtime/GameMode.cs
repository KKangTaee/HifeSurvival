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
    private Action<PlayerEntity> _onRecvReadyCB;
    private Action<int> _onRecvLeaveCB;
    private Action<int> _onRecvCountdownCB;
    private Action      _onRecvStartGameCB;


    public PlayerEntity EntitySelf { get; private set; }
    public int RoomId { get; private set; }

    public enum EStatus
    {
        JOIN,

        NOT_JOIN,
    }

    public EStatus Status { get; private set; } = EStatus.NOT_JOIN;

    public void RemovePlayerEntity(int inPlayerId)
    {
        if (PlayerEntitysDic.ContainsKey(inPlayerId) == false)
            PlayerEntitysDic.Remove(inPlayerId);
    }

    public void AddEvent(Action<PlayerEntity> inRecvJoin,
                         Action<PlayerEntity> inRecvSelect,
                         Action<PlayerEntity> inRecvReady,
                         Action<int> inRecvLeave,
                         Action<int> inRecvCountdown,
                         Action  inRecvGameStart)
    {
        _onRecvJoinCB = inRecvJoin;
        _onRecvLeaveCB = inRecvLeave;
        _onRecvSelectCB = inRecvSelect;
        _onRecvReadyCB = inRecvReady;
        _onRecvCountdownCB = inRecvCountdown;
        _onRecvStartGameCB = inRecvGameStart;
    }


    //---------------
    // 서버 관련
    //---------------

    public async Task<bool> JoinAsync()
    {
        _joinCompleted.Reset();

        OnSendJoinToGame();

        var waitResult = await _joinCompleted.Wait(10000);

        if (waitResult.isSuccess == false)
            return false;

        // 룸번호
        RoomId = waitResult.result.roomId;
        var joinPlayerList = waitResult.result.joinPlayerList;

        foreach (var joinPlayer in joinPlayerList)
            AddPlayerEntity(joinPlayer);

        Status = EStatus.JOIN;

        return true;
    }

    public void Leave()
    {
        Status = EStatus.NOT_JOIN;
        PlayerEntitysDic.Clear();
    }

    public void AddPlayerEntity(S_JoinToGame.JoinPlayer joinPlayer)
    {
        // 이미 참가중인 유저에 대해서는 패스처리한다.
        if (PlayerEntitysDic.ContainsKey(joinPlayer.playerId) == true)
            return;

        PlayerEntity entity = new PlayerEntity()
        {
            userId = joinPlayer.userId,
            userName = joinPlayer.userName,
            playerId = joinPlayer.playerId,
            heroId = joinPlayer.heroId,
        };

        // 내 자신일 경우 따 갖고 있는다.
        if (joinPlayer.userId == ServerData.Instance.UserData.user_id)
            EntitySelf = entity;

        PlayerEntitysDic.Add(joinPlayer.playerId, entity);
    }

    public void OnRecvJoin(S_JoinToGame inPacket)
    {
        if (Status == EStatus.JOIN)
        {
            // 이미 내가 참가 중이라면, 내려온 데이터에서 처리해야할 것들만 처리해주면 된다.
            foreach (var joinPlayer in inPacket.joinPlayerList)
            {
                if (PlayerEntitysDic.ContainsKey(joinPlayer.playerId) == false)
                {
                    AddPlayerEntity(joinPlayer);
                    _onRecvJoinCB?.Invoke(PlayerEntitysDic[joinPlayer.playerId]);
                    break;
                }
            }
        }
        else
        {
            _joinCompleted.Signal(inPacket);
        }
    }

    public void OnRecvLeave(S_LeaveToGame inPacket)
    {
        RemovePlayerEntity(inPacket.playerId);
        _onRecvLeaveCB?.Invoke(inPacket.playerId);
    }

    public void OnRecvSelectHero(CS_SelectHero inPacket)
    {
        if (PlayerEntitysDic.TryGetValue(inPacket.playerId, out var entity) == true)
        {
            entity.heroId = inPacket.heroId;

            if (EntitySelf.playerId != inPacket.playerId)
                _onRecvSelectCB?.Invoke(entity);
        }
    }

    public void OnRecvReadyToGame(CS_ReadyToGame inPacket)
    {
        if (PlayerEntitysDic.TryGetValue(inPacket.playerId, out var entity) == true)
        {
            entity.isReady = true;

            if (EntitySelf.playerId != inPacket.playerId)
                _onRecvReadyCB?.Invoke(entity);
        }
    }



    public void OnSendSelectHero(int inPlayerId, int inHeroId)
    {
        CS_SelectHero packet = new CS_SelectHero()
        {
            playerId = inPlayerId,
            heroId = inHeroId,
        };

        NetworkManager.Instance.Send(packet);
    }

    public void OnSendJoinToGame()
    {
        C_JoinToGame joinToGame = new C_JoinToGame();

        joinToGame.userId = ServerData.Instance.UserData.user_id;
        joinToGame.userName = ServerData.Instance.UserData.nickname;

        NetworkManager.Instance.Send(joinToGame);
    }

    public void OnSendReadyToGame()
    {
        CS_ReadyToGame readyToGame = new CS_ReadyToGame();
        readyToGame.playerId = EntitySelf.playerId;

        NetworkManager.Instance.Send(readyToGame);
    }

    public void OnRecvCountdown(S_Countdown inPacket)
    {
        _onRecvCountdownCB?.Invoke(inPacket.countdownSec);
    }

    public void OnRecvStartGame(S_StartGame inPacket)
    {
        _onRecvStartGameCB?.Invoke();
    }
}


public class PlayerEntity
{
    public string userId;
    public string userName;
    public int playerId;
    public int heroId;
    public bool isReady;
}