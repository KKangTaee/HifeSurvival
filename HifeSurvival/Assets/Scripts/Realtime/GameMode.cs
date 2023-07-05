using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;

public class GameMode
{
    private static GameMode _instance = new GameMode();
    public static GameMode Instance { get => _instance; }

    public Dictionary<int, PlayerEntity>  PlayerEntitysDict { get; private set; } = new Dictionary<int, PlayerEntity>();
    public Dictionary<int, MonsterEntity> MonsterEntityDict { get; private set; } = new Dictionary<int, MonsterEntity>();
    
    private SimpleTaskCompletionSource<S_JoinToGame> _joinCompleted = new SimpleTaskCompletionSource<S_JoinToGame>();

    private IngamePacketEventHandler    _ingameEventHandler;
    private GameReadyPacketEventHandler _gameReadyEventHandler;


    /// <summary>
    /// 입장 관련
    /// </summary>
    private Action<PlayerEntity> _onRecvJoinCB;
    private Action<int>          _onRecvLeaveCB;
    public int RoomId { get; private set; }
    
    public EGameModeStatus Status     { get; private set; }
    public PlayerEntity    EntitySelf { get; private set; }


    public GameMode()
    {
         _ingameEventHandler     = new IngamePacketEventHandler(this);
         _gameReadyEventHandler  = new GameReadyPacketEventHandler(this);
    }


    public T GetEventHandler<T>() where T : PacketEventHandlerBase
    {
        if(typeof(T) == typeof(IngamePacketEventHandler))
        {
            return _ingameEventHandler as T;
        }
        else if(typeof(T) == typeof(GameReadyPacketEventHandler))
        {
            return _gameReadyEventHandler as T;
        }

        return null;
    }

    public void SetStatus(EGameModeStatus gameModeStatus)
    {
        Status = gameModeStatus;
    }

    public void RemovePlayerEntity(int inPlayerId)
    {
        if (PlayerEntitysDict.ContainsKey(inPlayerId) == false)
            PlayerEntitysDict.Remove(inPlayerId);
    }

    public void AddEvent(Action<PlayerEntity> inRecvJoin,
                         Action<int> inRecvLeave)
    {
        _onRecvJoinCB  = inRecvJoin;
        _onRecvLeaveCB = inRecvLeave;
    }

    public bool IsSelf(int inPlayerId)
    {
        return EntitySelf.id == inPlayerId;
    }

    public void Leave()
    {
        PlayerEntitysDict.Clear();
    }

    public PlayerEntity GetPlayerEntity(int inTargetId)
    {
        if (PlayerEntitysDict.TryGetValue(inTargetId, out var player) && player != null)
        {
            return player;
        }

        Debug.LogError($"[{nameof(GetPlayerEntity)}] playerEntity is null or empty!");
        return null;
    }

    public MonsterEntity GetMonsterEntity(int inTargetId)
    {
         if (MonsterEntityDict.TryGetValue(inTargetId, out var monster) && monster != null)
        {
            return monster;
        }

        Debug.LogError($"[{nameof(GetMonsterEntity)}] playerEntity is null or empty!");
        return null;
    }

    public void AddPlayerEntity(S_JoinToGame.JoinPlayer joinPlayer)
    {
        // 이미 참가중인 유저에 대해서는 패스처리한다.
        if (PlayerEntitysDict.ContainsKey(joinPlayer.id) == true)
            return;

        PlayerEntity entity = new PlayerEntity()
        {
            userId = joinPlayer.userId,
            userName = joinPlayer.userName,
            id = joinPlayer.id,
            heroId = joinPlayer.heroKey,
        };

        // 내 자신일 경우 캐싱처리한다
        if (joinPlayer.userId == ServerData.Instance.UserData.user_id)
            EntitySelf = entity;

        PlayerEntitysDict.Add(joinPlayer.id, entity);
    }


    public MonsterEntity CreateMonsterEntity(MonsterSpawn m)
    {
        if (StaticData.Instance.MonstersDict.TryGetValue(m.monstersKey.ToString(), out var monster) == false)
        {
            Debug.LogError($"[{nameof(CreateMonsterEntity)}] monster static is null or empty!");
            return null;
        }

        var monsterEntity = new MonsterEntity()
        {
            id = m.id,
            monsterId = m.monstersKey,
            grade = m.grade,
            pos = m.pos,
        };

        MonsterEntityDict.Add(m.id, monsterEntity);
        return monsterEntity;
    }


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


        return true;
    }



    //-----------------
    // Send
    //-----------------

    public void OnSendMoveRequest(in Vector3 inCurrPos, in Vector3 inDestPos)
    {
        MoveRequest moveRequest = new MoveRequest()
        {
            id = EntitySelf.id,
            currentPos = inCurrPos.ConvertPVec3(),
            targetPos  = inDestPos.ConvertPVec3(),
            speed = EntitySelf.stat.moveSpeed,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        NetworkManager.Instance.Send(moveRequest);
    }


    public void OnSendAttack(int toId, int damageValue)
    {
        CS_Attack attack = new CS_Attack()
        {
            id = EntitySelf.id,
            targetId = toId,
            attackValue = damageValue,
        };

        NetworkManager.Instance.Send(attack);
    }

    public void OnSendSelectHero(int inHeroKey)
    {
        CS_SelectHero packet = new CS_SelectHero()
        {
            id = EntitySelf.id,
            heroKey = inHeroKey,
        };

        NetworkManager.Instance.Send(packet);
    }

    public void OnSendJoinToGame()
    {
        C_JoinToGame joinToGame = new C_JoinToGame()
        {
            userId  = ServerData.Instance.UserData.user_id,
            userName = ServerData.Instance.UserData.nickname,
        };

        NetworkManager.Instance.Send(joinToGame);
    }

    public void OnSendReadyToGame()
    {
        CS_ReadyToGame readyToGame = new CS_ReadyToGame()
        {
            id = EntitySelf.id,
        };

        NetworkManager.Instance.Send(readyToGame);
    }

    public void OnSendPickReward(int inWorldId)
    {
        PickRewardRequest getItem = new PickRewardRequest()
        {
            id = EntitySelf.id,
            worldId = inWorldId,
        };

        NetworkManager.Instance.Send(getItem);
    }

    public void OnSendPlayStart()
    {
        PlayStartRequest request = new PlayStartRequest()
        {
            id = EntitySelf.id,
        };

        NetworkManager.Instance.Send(request);
    }

    public void OnSendIncreaseStat(int type, int increase)
    {
        IncreaseStatRequest increaseStat = new IncreaseStatRequest()
        {
            id = EntitySelf.id,
            type = type,
            increase = increase
        };

        NetworkManager.Instance.Send(increaseStat);
    }


    //-----------------
    // Receive
    //-----------------

    public void OnRecvJoin(S_JoinToGame inPacket)
    {
        if (Status != EGameModeStatus.None)
        {
            // 이미 내가 참가 중이라면, 내려온 데이터에서 처리해야할 것들만 처리해주면 된다.
            foreach (var joinPlayer in inPacket.joinPlayerList)
            {
                if (PlayerEntitysDict.ContainsKey(joinPlayer.id) == false)
                {
                    AddPlayerEntity(joinPlayer);
                    _onRecvJoinCB?.Invoke(PlayerEntitysDict[joinPlayer.id]);
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
        RemovePlayerEntity(inPacket.id);
        _onRecvLeaveCB?.Invoke(inPacket.id);
    }
}