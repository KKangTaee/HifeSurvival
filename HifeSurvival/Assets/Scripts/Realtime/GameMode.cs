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


    /// <summary>
    /// 입장 관련
    /// </summary>
    private Action<PlayerEntity> _onRecvJoinCB;
    private Action<PlayerEntity> _onRecvSelectCB;
    private Action<PlayerEntity> _onRecvReadyCB;

    private Action<int> _onRecvLeaveCB;
    private Action<int> _onRecvCountdownCB;
    private Action _onRecvStartGameCB;

    public event Action<PlayerEntity>   OnRecvMoveCB;
    public event Action<PlayerEntity>   OnRecvStopMoveCB;

    public event Action<S_Dead>         OnRecvDeadCB;
    public event Action<CS_Attack>      OnRecvAttackCB;
    public event Action<PlayerEntity>   OnRecvRespawnCB;

    public PlayerEntity EntitySelf { get; private set; }
    public int RoomId { get; private set; }

    public enum EStatus
    {
        JOIN,

        NOT_JOIN,

        GAME_RUNIING,
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
                         Action inRecvGameStart)
    {
        _onRecvJoinCB = inRecvJoin;
        _onRecvLeaveCB = inRecvLeave;
        _onRecvSelectCB = inRecvSelect;
        _onRecvReadyCB = inRecvReady;
        _onRecvCountdownCB = inRecvCountdown;
        _onRecvStartGameCB = inRecvGameStart;
    }

    public bool IsSelf(int inPlayerId)
    {
        return EntitySelf.targetId == inPlayerId;
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


        if (StaticData.Instance.HeroDic.TryGetValue(joinPlayer.heroId.ToString(), out var heros) == false)
        {
            Debug.LogError("heros static data is null or empty!");
            return;
        }


        PlayerEntity entity = new PlayerEntity()
        {
            userId   = joinPlayer.userId,
            userName = joinPlayer.userName,
            targetId = joinPlayer.playerId,
            heroId   = joinPlayer.heroId,
            stat = new EntityStat(heros),
        };

        // 내 자신일 경우 캐싱처리한다
        if (joinPlayer.userId == ServerData.Instance.UserData.user_id)
            EntitySelf = entity;

        PlayerEntitysDic.Add(joinPlayer.playerId, entity);
    }


    public PlayerEntity GetPlayerEntity(int inPlayerId)
    {
        if (PlayerEntitysDic.TryGetValue(inPlayerId, out var player) && player != null)
        {
            return player;
        }

        Debug.LogError($"[{nameof(GetPlayerEntity)}] playerEntity is null or empty!");
        return null;
    }



    //---------------
    // 서버 관련
    //---------------

    public async Task<bool> JoinAsync()
    {
        _joinCompleted.Reset();

        OnSendJoinToGame();

        var waitResult = await _joinCompleted.Wait(7000);

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




    //-----------------
    // Send
    //-----------------

    public void OnSendMove(in Vector3 inPos, in Vector3 inDir)
    {
        CS_Move move = new CS_Move()
        {
            dir = inDir.ConvertVec3(),
            pos = inPos.ConvertVec3(),
            isPlayer = true,
            speed = EntitySelf.stat.moveSpeed,
            targetId = EntitySelf.targetId,
        };

        NetworkManager.Instance.Send(move);
    }


    public void OnSendStopMove(in Vector3 inPos, in Vector3 inDir)
    {
        CS_StopMove stopMove = new CS_StopMove()
        {
            pos = inPos.ConvertVec3(),
            dir = inDir.ConvertVec3(),
            isPlayer = true,
            targetId = EntitySelf.targetId,
        };

        NetworkManager.Instance.Send(stopMove);
    }


    public void OnSendAttack(bool toIdIsPlayer, int toId, int fromId, int damageValue)
    {
        CS_Attack attack = new CS_Attack()
        {
            toIdIsPlayer = toIdIsPlayer,
            toId = toId,
            fromId = fromId,
            damageValue = damageValue,
        };

        NetworkManager.Instance.Send(attack);
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
        readyToGame.playerId = EntitySelf.targetId;

        NetworkManager.Instance.Send(readyToGame);
    }




    //-----------------
    // Receive
    //-----------------


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
        var player = GetPlayerEntity(inPacket.playerId);

        if (player == null)
            return;

        player.heroId = inPacket.heroId;
        player.stat = new EntityStat(StaticData.Instance.HeroDic[player.heroId.ToString()]);


        if (IsSelf(inPacket.playerId) == false)
            _onRecvSelectCB?.Invoke(player);
    }


    public void OnRecvReadyToGame(CS_ReadyToGame inPacket)
    {
        var player = GetPlayerEntity(inPacket.playerId);

        if (player == null)
            return;

        player.isReady = true;

        if (IsSelf(inPacket.playerId) == false)
            _onRecvReadyCB?.Invoke(player);
    }


    public void OnRecvCountdown(S_Countdown inPacket)
    {
        _onRecvCountdownCB?.Invoke(inPacket.countdownSec);
    }


    public void OnRecvStartGame(S_StartGame inPacket)
    {
        Status = EStatus.GAME_RUNIING;
        _onRecvStartGameCB?.Invoke();
    }


    public void OnRecvMove(CS_Move inPacket)
    {
        if (inPacket.isPlayer == true)
        {
            if (PlayerEntitysDic.TryGetValue(inPacket.targetId, out var player) == true)
            {
                player.dir = inPacket.dir;
                player.pos = inPacket.pos;

                if (IsSelf(inPacket.targetId) == false)
                    OnRecvMoveCB?.Invoke(player);
            }
        }
        else
        {

        }
    }


    public void OnRecvStopMove(CS_StopMove inPacket)
    {
        if (inPacket.isPlayer == true)
        {
            if (PlayerEntitysDic.TryGetValue(inPacket.targetId, out var player) == true)
            {
                player.pos = inPacket.pos;
                player.dir = inPacket.dir;

                if (IsSelf(inPacket.targetId) == false)
                    OnRecvStopMoveCB?.Invoke(player);
            }
        }
        else
        {

        }
    }


    public void OnRecvAttack(CS_Attack inPacket)
    {
        if (inPacket.toIdIsPlayer == true)
        {
            if (PlayerEntitysDic.TryGetValue(inPacket.toId, out var player) == true)
            {
                player.stat.AddHp(-inPacket.damageValue);

                if(IsSelf(inPacket.fromId) == false)
                   OnRecvAttackCB?.Invoke(inPacket);
            }
        }
        else
        {

        }
    }


    public void OnRecvDead(S_Dead inPacket)
    {
        if(inPacket.toIdIsPlayer == true)
        {
            OnRecvDeadCB?.Invoke(inPacket);
        }
        else
        {

        }
    }


    public void OnRecvRespawn(S_Respawn inPacket)
    {
        if(inPacket.isPlayer == true)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            player.pos = inPacket.pos;

            OnRecvRespawnCB?.Invoke(player);       
        }
        else
        {

        }
    }
}