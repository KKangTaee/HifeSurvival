using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;

public class GameMode
{
    public enum EStatus
    {
        JOIN,

        NOT_JOIN,

        GAME_RUNIING,
    }

    private static GameMode _instance = new GameMode();
    public static GameMode Instance { get => _instance; }

    public Dictionary<int, PlayerEntity> PlayerEntitysDict { get; private set; } = new Dictionary<int, PlayerEntity>();
    public Dictionary<int, MonsterEntity> MonsterEntityDict { get; private set; } = new Dictionary<int, MonsterEntity>();


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


    //---------------
    // 인게임 진행 관련
    //---------------

    public event Action<Entity> OnRecvMoveHandler;
    public event Action<Entity> OnRecvStopMoveHandler;
    public event Action<S_Dead> OnRecvDeadHandler;
    public event Action<CS_Attack> OnRecvAttackHandler;
    public event Action<Entity> OnRecvRespawnHandler;
    public event Action<PlayerEntity> OnRecvUpdateStatHandler;
    public event Action<S_DropReward> OnRecvDropRewardHandler;
    public event Action<S_GetItem> OnRecvGetItemHandler;
    public event Action<S_GetGold> OnRecvGetGoldHandler;


    public int RoomId { get; private set; }
    public EStatus Status { get; private set; } = EStatus.NOT_JOIN;
    public PlayerEntity EntitySelf { get; private set; }


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
        return EntitySelf.targetId == inPlayerId;
    }

    public void Leave()
    {
        Status = EStatus.NOT_JOIN;
        PlayerEntitysDict.Clear();
    }

    public void AddPlayerEntity(S_JoinToGame.JoinPlayer joinPlayer)
    {
        // 이미 참가중인 유저에 대해서는 패스처리한다.
        if (PlayerEntitysDict.ContainsKey(joinPlayer.targetId) == true)
            return;


        if (StaticData.Instance.HerosDict.TryGetValue(joinPlayer.heroId.ToString(), out var heros) == false)
        {
            Debug.LogError("heros static data is null or empty!");
            return;
        }

        PlayerEntity entity = new PlayerEntity()
        {
            userId = joinPlayer.userId,
            userName = joinPlayer.userName,
            targetId = joinPlayer.targetId,
            heroId = joinPlayer.heroId,
            stat = new EntityStat(heros),
        };

        // 내 자신일 경우 캐싱처리한다
        if (joinPlayer.userId == ServerData.Instance.UserData.user_id)
            EntitySelf = entity;

        PlayerEntitysDict.Add(joinPlayer.targetId, entity);
    }


    public PlayerEntity GetPlayerEntity(int inPlayerId)
    {
        if (PlayerEntitysDict.TryGetValue(inPlayerId, out var player) && player != null)
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




    //-----------------
    // Send
    //-----------------

    public void OnSendMove(in Vector3 inPos, in Vector3 inDir)
    {
        CS_Move move = new CS_Move()
        {
            dir = inDir.ConvertPVec3(),
            pos = inPos.ConvertPVec3(),
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
            pos = inPos.ConvertPVec3(),
            dir = inDir.ConvertPVec3(),
            isPlayer = true,
            targetId = EntitySelf.targetId,
        };

        NetworkManager.Instance.Send(stopMove);
    }


    public void OnSendAttack(in Vector3 inPos, in Vector3 inDir, bool toIsPlayer, int toId, int damageValue)
    {
        CS_Attack attack = new CS_Attack()
        {
            toIsPlayer = toIsPlayer,
            toId = toId,
            fromIsPlayer = true,
            fromId = EntitySelf.targetId,
            fromPos = inPos.ConvertPVec3(),
            fromDir = inDir.ConvertPVec3(),
            attackValue = damageValue,
        };

        NetworkManager.Instance.Send(attack);
    }

    public void OnSendSelectHero(int inHeroId)
    {
        CS_SelectHero packet = new CS_SelectHero()
        {
            targetId = EntitySelf.targetId,
            heroId = inHeroId,
        };

        NetworkManager.Instance.Send(packet);
    }

    public void OnSendJoinToGame()
    {
        C_JoinToGame joinToGame = new C_JoinToGame()
        {
            userId = ServerData.Instance.UserData.user_id,
            userName = ServerData.Instance.UserData.nickname,
        };

        NetworkManager.Instance.Send(joinToGame);
    }

    public void OnSendReadyToGame()
    {
        CS_ReadyToGame readyToGame = new CS_ReadyToGame()
        {
            targetId = EntitySelf.targetId,
        };

        NetworkManager.Instance.Send(readyToGame);
    }

    public void OnSendUpdateStat(int inUsedGold, in Stat inStat)
    {
        CS_UpdateStat updateStat = new CS_UpdateStat()
        {
            targetId = EntitySelf.targetId,
            usedGold = inUsedGold,
            updateStat = inStat,
        };

        NetworkManager.Instance.Send(updateStat);
    }

    public void OnSendPickReward(int inWorldId)
    {
        C_PickReward getItem = new C_PickReward()
        {
            targetId = EntitySelf.targetId,
            worldId = inWorldId,
        };

        NetworkManager.Instance.Send(getItem);
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
                if (PlayerEntitysDict.ContainsKey(joinPlayer.targetId) == false)
                {
                    AddPlayerEntity(joinPlayer);
                    _onRecvJoinCB?.Invoke(PlayerEntitysDict[joinPlayer.targetId]);
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
        RemovePlayerEntity(inPacket.targetId);

        _onRecvLeaveCB?.Invoke(inPacket.targetId);
    }


    public void OnRecvSelectHero(CS_SelectHero inPacket)
    {
        var player = GetPlayerEntity(inPacket.targetId);

        if (player == null)
            return;

        player.heroId = inPacket.heroId;
        player.stat = new EntityStat(StaticData.Instance.HerosDict[player.heroId.ToString()]);


        if (IsSelf(inPacket.targetId) == false)
            _onRecvSelectCB?.Invoke(player);
    }


    public void OnRecvReadyToGame(CS_ReadyToGame inPacket)
    {
        var player = GetPlayerEntity(inPacket.targetId);

        if (player == null)
            return;

        player.isReady = true;

        if (IsSelf(inPacket.targetId) == false)
            _onRecvReadyCB?.Invoke(player);
    }


    public void OnRecvCountdown(S_Countdown inPacket)
    {
        _onRecvCountdownCB?.Invoke(inPacket.countdownSec);
    }

    public void OnRecvStartGame(S_StartGame inPacket)
    {
        Status = EStatus.GAME_RUNIING;

        var playerList = inPacket.playerList;
        var monsterList = inPacket.monsterList;

        foreach (PlayerSpawn p in playerList)
        {
            var playerEntity = GetPlayerEntity(p.targetId);
            playerEntity.heroId = p.herosKey;
            playerEntity.pos = p.pos;
        }

        foreach (MonsterSpawn m in monsterList)
        {
            var monsterEntity = CreateMonsterEntity(m);
            MonsterEntityDict.Add(monsterEntity.targetId, monsterEntity);
        }
        
        _onRecvStartGameCB?.Invoke();
    }

    public void OnRecvSpawnMonster(S_SpawnMonster inPacket)
    {
        var monsterList = inPacket.monsterList;

        foreach (MonsterSpawn m in monsterList)
        {
            var monsterEntity = CreateMonsterEntity(m);
            MonsterEntityDict.Add(monsterEntity.targetId, monsterEntity);
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
            targetId = m.targetId,
            monsterId = m.monstersKey,
            grade = m.grade,
            pos = m.pos,
            stat = new EntityStat(monster),
        };
        return monsterEntity;
    }

    public void OnRecvMove(CS_Move inPacket)
    {
        if (inPacket.isPlayer == true)
        {
            if (PlayerEntitysDict.TryGetValue(inPacket.targetId, out var player) == true)
            {
                player.dir = inPacket.dir;
                player.pos = inPacket.pos;

                if (IsSelf(inPacket.targetId) == false)
                    OnRecvMoveHandler?.Invoke(player);
            }
        }
        else
        {
            if (MonsterEntityDict.TryGetValue(inPacket.targetId, out var monster) == true)
            {
                monster.dir = inPacket.dir;
                monster.pos = inPacket.pos;

                OnRecvMoveHandler?.Invoke(monster);
            }
        }
    }


    public void OnRecvStopMove(CS_StopMove inPacket)
    {
        if (inPacket.isPlayer == true)
        {
            if (PlayerEntitysDict.TryGetValue(inPacket.targetId, out var player) == true)
            {
                player.pos = inPacket.pos;
                player.dir = inPacket.dir;

                if (IsSelf(inPacket.targetId) == false)
                    OnRecvStopMoveHandler?.Invoke(player);
            }
        }
        else
        {
            if (MonsterEntityDict.TryGetValue(inPacket.targetId, out var monster) == true)
            {
                monster.pos = inPacket.pos;
                monster.dir = inPacket.dir;

                OnRecvStopMoveHandler?.Invoke(monster);
            }
        }
    }

    public void OnRecvAttack(CS_Attack inPacket)
    {
        Entity toEntity = null;

        if (inPacket.toIsPlayer == true)
        {
            if (PlayerEntitysDict.TryGetValue(inPacket.toId, out var player) == true)
                toEntity = player;
        }
        else
        {
            if (MonsterEntityDict.TryGetValue(inPacket.toId, out var monster) == true)
                toEntity = monster;
        }

        // 공격
        toEntity.stat.AddCurrHp(-inPacket.attackValue);

        if (IsSelf(inPacket.fromId) == false)
            OnRecvAttackHandler?.Invoke(inPacket);
    }


    public void OnRecvDead(S_Dead inPacket)
    {
        OnRecvDeadHandler?.Invoke(inPacket);
    }


    public void OnRecvRespawn(S_Respawn inPacket)
    {
        if (inPacket.isPlayer == true)
        {
            var player = GetPlayerEntity(inPacket.targetId);

            if (player == null)
                return;

            player.pos = inPacket.pos;

            OnRecvRespawnHandler?.Invoke(player);
        }
        else
        {

        }
    }


    public void OnRecvUpdateStat(CS_UpdateStat inPacket)
    {
        var player = GetPlayerEntity(inPacket.targetId);

        if (player == null)
            return;

        player.AddGold(-inPacket.usedGold);
        player.stat.IncreaseStat(inPacket.updateStat);
    }


    public void OnRecvDropItem(S_DropReward inPacket)
    {
        OnRecvDropRewardHandler?.Invoke(inPacket);
    }


    public void OnRecvGetItem(S_GetItem inPacket)
    {
        var player = GetPlayerEntity(inPacket.targetId);

        player.itemSlot[inPacket.itemSlotId] = new EntityItem(inPacket.item);

        OnRecvGetItemHandler?.Invoke(inPacket);
    }


    public void OnRecvGetGold(S_GetGold inPacket)
    {
        var player = GetPlayerEntity(inPacket.targetId);

        player.AddGold(inPacket.gold);

        OnRecvGetGoldHandler?.Invoke(inPacket);
    }
}