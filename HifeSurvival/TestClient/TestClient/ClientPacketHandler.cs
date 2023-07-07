using System;
using ServerCore;
using TestClient;
using System.Collections.Generic;
using System.Linq;

public class ClientPacketHandler : PacketHandler
{
    public override void C_JoinToGameHandler(Session session, IPacket packet)
    {
        
    }

    public override void S_JoinToGameHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if(packet is S_JoinToGame response)
        {
            var player = response.joinPlayerList.AsQueryable().Where( p => p.userId == DEFINE.TEST_USER_ID).FirstOrDefault();
            sesh.Player = new PlayerEntity()
            {
                Id = player.id,
                ClientStatus = 0,
                GameModeStatus = 0,
            };
        }

        sesh.RoomReady();
    }

    public override void S_LeaveToGameHandler(Session session, IPacket packet)
    {
        
    }

    public override void CS_SelectHeroHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if (packet is CS_SelectHero response)
        {
            sesh.Player.HeroKey = response.heroKey;
        }
    }

    public override void CS_ReadyToGameHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if(packet is CS_ReadyToGame res)
        {
            sesh.Player.GameModeStatus = 1;
        }
    }

    public override void S_CountdownHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if (packet is S_Countdown res)
        {
            sesh.Player.GameModeStatus = 2;
            sesh.Player.CountDownSec = res.countdownSec * 1000;
        }
    }

    public override void S_StartGameHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if (packet is S_StartGame res)
        {
            sesh.Player.GameModeStatus = 3;
            sesh.Player.HeroKey = res.playerList.AsQueryable().Where(p => p.id == sesh.Player.Id).FirstOrDefault().herosKey;
            sesh.PlayStart();
        }
    }

    public override void CS_AttackHandler(Session session, IPacket packet)
    {
        
    }

    public override void S_DeadHandler(Session session, IPacket packet)
    {
        
    }

    public override void S_RespawnHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdateSpawnMonsterBroadcastHandler(Session session, IPacket packet)
    {
        if (packet is UpdateSpawnMonsterBroadcast broadcast)
        {
            foreach(var mon in broadcast.monsterList)
            {
                Form1.LogMsgQ.Enqueue($"UpdateSpawn  gid {mon.groupId } key {mon.monstersKey} ");
            }
        }
    }

    public override void MoveRequestHandler(Session session, IPacket packet)
    {
        
    }
    
    public override void UpdateLocationBroadcastHandler(Session session, IPacket packet)
    {
        
    }

    public override void MoveResponseHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdateStatBroadcastHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if (packet is UpdateStatBroadcast res)
        {
            if (res.id == sesh.Player.Id)
            {
                sesh.Player.OriginStat = res.originStat;
                sesh.Player.AdditionalStat = res.addStat;
            }

            Form1.LogMsgQ.Enqueue($"Stat Update id :{res.id}");
        }
    }

    public override void IncreaseStatRequestHandler(Session session, IPacket packet)
    {
        
    }

    public override void IncreaseStatResponseHandler(Session session, IPacket packet)
    {
        
    }

    public override void PickRewardRequestHandler(Session session, IPacket packet)
    {
        
    }

    public override void PickRewardResponseHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdateRewardBroadcastHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdatePlayerCurrencyHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if (packet is UpdatePlayerCurrency res)
        {
            sesh.Player.CurrencyList = new List<PCurrency>();
            sesh.Player.CurrencyList = res.currencyList;
        }
    }

    public override void PlayStartRequestHandler(Session session, IPacket packet)
    {
        
    }

    public override void PlayStartResponseHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if (packet is PlayStartResponse res)
        {
            sesh.Player.GameModeStatus = 4;
        }
    }

    public override void UpdateGameModeStatusBroadcastHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdateInvenItemHandler(Session session, IPacket packet)
    {
        var sesh = session as ClientSession;
        if (sesh == null)
        {
            return;
        }

        if (packet is UpdateInvenItem res)
        {
            var item = res.invenItem;

            if( sesh.Player.InvenItemDict.ContainsKey(item.slot))
            {
                sesh.Player.InvenItemDict[item.slot] = item;
            }
            else
            {
                sesh.Player.InvenItemDict.Add(item.slot, item);
            }
        }
    }

    public override void CheatRequestHandler(Session session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void CheatResponseHandler(Session session, IPacket packet)
    {
        var resultStr = ((CheatResponse)packet).result == 0 ? "Success" : "Failed";
        Form1.LogMsgQ.Enqueue($"Cheat Result : {resultStr }");
    }
}
