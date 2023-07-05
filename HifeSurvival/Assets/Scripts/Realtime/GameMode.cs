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


    private IngamePacketEvent _ingamePacketEvent;

    /// <summary>
    /// 입장 관련
    /// </summary>
    private Action<PlayerEntity> _onRecvJoinCB;
    private Action<PlayerEntity> _onRecvSelectCB;
    private Action<PlayerEntity> _onRecvReadyCB;

    private Action<int> _onRecvLeaveCB;
    private Action<int> _onRecvCountdownCB;
    private Action _onRecvStartGameCB;
    public event Action<UpdateGameModeStatusBroadcast>  OnUpdateGameModeStatusHandler;


    //---------------
    // 인게임 진행 관련
    //---------------


    public event Action<Entity>     OnRecvStopMoveHandler;
    public event Action<Entity>     OnRecvRespawnHandler;
    public event Action<PlayerEntity>           OnRecvUpdateStatHandler;
    
    // public event Action<S_Dead>     OnRecvDeadHandler;
    // public event Action<CS_Attack>  OnRecvAttackHandler;
    public event Action<PickRewardResponse>     OnRecvPickRewardHandler;
    public event Action<IncreaseStatResponse>   OnRecvIncreasStatHandler;
    
    // public event Action<UpdateRewardBroadcast>          OnRecvUpdateRewardHandler;
    // public event Action<UpdateLocationBroadcast>        OnUpdateLocationHandler;
    
    // public event Action<UpdateInvenItem>           OnUpdateInvenItemSingleHandler;
    // public event Action<UpdatePlayerCurrency>      OnUpdatePlayerCurrencySingleHandler;



    public int RoomId { get; private set; }
    public EGameModeStatus Status  { get; private set; } = EGameModeStatus.None;
    public PlayerEntity EntitySelf { get; private set; }


    public GameMode()
    {
         _ingamePacketEvent = new IngamePacketEvent(this);
    }


    public T GetEventHandler<T>() where T : PacketEventBase
    {
        if(typeof(T) == typeof(IngamePacketEvent))
            return _ingamePacketEvent as T;

        return null;
    }


    public void RemovePlayerEntity(int inPlayerId)
    {
        if (PlayerEntitysDict.ContainsKey(inPlayerId) == false)
            PlayerEntitysDict.Remove(inPlayerId);
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
        return EntitySelf.id == inPlayerId;
    }

    public void Leave()
    {
        PlayerEntitysDict.Clear();
    }

    public void AddPlayerEntity(S_JoinToGame.JoinPlayer joinPlayer)
    {
        // 이미 참가중인 유저에 대해서는 패스처리한다.
        if (PlayerEntitysDict.ContainsKey(joinPlayer.id) == true)
            return;


        if (StaticData.Instance.HerosDict.TryGetValue(joinPlayer.heroKey.ToString(), out var heros) == false)
        {
            Debug.LogError("heros static data is null or empty!");
            return;
        }

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

        // Status = EStatus.JOIN;

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

    public void OnSendUpdateStat(int inUsedGold, int inType, int inIncrease)
    {
        IncreaseStatRequest updateStat = new IncreaseStatRequest()
        {
            id   = EntitySelf.id,
            type = inUsedGold,
            increase = inIncrease
        };

        NetworkManager.Instance.Send(updateStat);
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


    public void OnRecvSelectHero(CS_SelectHero inPacket)
    {
        var player = GetPlayerEntity(inPacket.id);

        if (player == null)
            return;

        player.heroId = inPacket.heroKey;

        if (IsSelf(inPacket.id) == false)
            _onRecvSelectCB?.Invoke(player);
    }


    public void OnRecvReadyToGame(CS_ReadyToGame inPacket)
    {
        var player = GetPlayerEntity(inPacket.id);

        if (player == null)
            return;

        player.isReady = true;

        if (IsSelf(inPacket.id) == false)
            _onRecvReadyCB?.Invoke(player);
    }


    public void OnRecvStartGame(S_StartGame inPacket)
    {
        var playerList = inPacket.playerList;
        var monsterList = inPacket.monsterList;

        foreach (PlayerSpawn p in playerList)
        {
            var playerEntity = GetPlayerEntity(p.id);
            playerEntity.heroId = p.herosKey;
            playerEntity.pos = p.pos;
        }

        foreach (MonsterSpawn m in monsterList)
        {
            var monsterEntity = CreateMonsterEntity(m);
            MonsterEntityDict.Add(monsterEntity.id, monsterEntity);
        }

    }

    public void OnRecvSpawnMonster(S_SpawnMonster inPacket)
    {
        var monsterList = inPacket.monsterList;

        foreach (MonsterSpawn m in monsterList)
        {
            var monsterEntity = CreateMonsterEntity(m);
            MonsterEntityDict.Add(monsterEntity.id, monsterEntity);
        }
    }

    public MonsterEntity CreateMonsterEntity(MonsterSpawn m)
    {
        if (StaticData.Instance.MonstersDict.TryGetValue(m.monstersKey.ToString(), out var monster) == false)
        {
            Debug.LogError($"[{nameof(OnRecvStartGame)}] monster static is null or empty!");
            return null;
        }

        var monsterEntity = new MonsterEntity()
        {
            id = m.id,
            monsterId = m.monstersKey,
            grade = m.grade,
            pos = m.pos,
        };
        return monsterEntity;
    }


    // public void OnRecvAttack(CS_Attack inPacket)
    // {
    //     Entity toEntity = null;

    //     switch(Entity.GetEntityType(inPacket.targetId))
    //     {
    //         case Entity.EEntityType.PLAYER:
    //             toEntity = GetPlayerEntity(inPacket.targetId);
    //             break;
            
    //         case Entity.EEntityType.MOSNTER:
    //             toEntity =GetMonsterEntity(inPacket.targetId);
    //             break;
    //     }

    //     // 공격
    //     toEntity.stat.AddCurrHp(-inPacket.attackValue);

    //     if (IsSelf(inPacket.id) == false)
    //         OnRecvAttackHandler?.Invoke(inPacket);
    // }


    // public void OnRecvDead(S_Dead inPacket)
    // {
    //     OnRecvDeadHandler?.Invoke(inPacket);
    // }


    public void OnRecvRespawn(S_Respawn inPacket)
    {
        switch(Entity.GetEntityType(inPacket.id))
        {
            case Entity.EEntityType.PLAYER:
              
            var player = GetPlayerEntity(inPacket.id);

            if (player == null)
                return;

            player.pos = inPacket.pos;

            OnRecvRespawnHandler?.Invoke(player);
                break;
            
            case Entity.EEntityType.MOSNTER:
                break;
        }
    }


    public void OnRecvIncreasStat(IncreaseStatResponse packet)
    {
        var player = GetPlayerEntity(packet.id);

        if (player == null)
            return;

        player.stat.IncreaseStat((EStatType)packet.type, packet.increase);
        OnRecvIncreasStatHandler?.Invoke(packet);
    }



    public void OnRecvPickReward(PickRewardResponse packet)
    {
        OnRecvPickRewardHandler?.Invoke(packet);
    }



    //----------------------
    // UpdateBroadcast
    //----------------------

    // public void OnUpdateLocationBroadcast(UpdateLocationBroadcast inPacket)
    // {
    //     Entity entity = null;
    //     switch(Entity.GetEntityType(inPacket.id))
    //     {
    //         case Entity.EEntityType.PLAYER:
    //             entity = GetPlayerEntity(inPacket.id);
    //             break;
            
    //         case Entity.EEntityType.MOSNTER:
    //             entity = GetMonsterEntity(inPacket.id);
    //             break;
    //     }

    //     entity.pos = inPacket.currentPos;
    //     OnUpdateLocationHandler.Invoke(inPacket);
    // }


    public void OnUpdateGameModeStatusBroadcast(UpdateGameModeStatusBroadcast packet)
    {
        Status = (EGameModeStatus)packet.status;

        switch(Status)
        {
            // 게임 카운트다운
            case EGameModeStatus.Countdown:                
                // TODO@taeho.kang 임시
                int countdownSec = 10;
                _onRecvCountdownCB?.Invoke(countdownSec);
                break;

            // 씬 전환
            case EGameModeStatus.LoadGame:
                _onRecvStartGameCB?.Invoke();
                break;

            // 완전 게임시작
            case EGameModeStatus.PlayStart:
                OnUpdateGameModeStatusHandler?.Invoke(packet);
                break;
        }
    }

    public void OnUpdateStatBroadcast(UpdateStatBroadcast packet)
    {
        Entity entity = null;
        switch(Entity.GetEntityType(packet.id))
        {
            case Entity.EEntityType.PLAYER:
                entity = GetPlayerEntity(packet.id);
                break;
            
            case Entity.EEntityType.MOSNTER:
                entity = GetMonsterEntity(packet.id);
                break;
        }

        entity.stat = new EntityStat(packet.originStat);
    }


    // public void OnUpdateRewardBroadcast(UpdateRewardBroadcast inPacket)
    // {
    //     // 아이템 드랍 및 월드맵 오브젝트 제거
    //     OnRecvUpdateRewardHandler?.Invoke(inPacket);
    // }


    //---------------------
    // Update Single
    //---------------------

    // public void OnUpdateInvenItemSingle(UpdateInvenItem packet)
    // {
    //     var invenItem = new EntityItem(packet.invenItem);
    //     EntitySelf.itemSlot[packet.invenItem.slot] = invenItem;
        
    //     OnUpdateInvenItemSingleHandler?.Invoke(packet);
    // }

    // public void OnUpdatePlayerCurrencySingle(UpdatePlayerCurrency packet)
    // {
    //     foreach(var currency in packet.currencyList)
    //     {
    //         switch((ECurrency)currency.currencyType)
    //         {
    //             case ECurrency.GOLD:
    //             EntitySelf.SetGold(currency.count);
    //             break;
    //         }
    //     }

    //     OnUpdatePlayerCurrencySingleHandler.Invoke(packet);
    // }
}