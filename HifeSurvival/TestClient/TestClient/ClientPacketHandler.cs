using System;
using ServerCore;
using TestClient;
using System.Collections.Generic;


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
            var player = response.joinPlayerList[0];
            sesh.Player = new PlayerEntity()
            {
                Id = player.id,
                ClientStatus = 0,
                GameModeStatus = 0,
            };
        }

        sesh.AutoReady();
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
            sesh.Player.HeroKey = res.playerList[0].herosKey;
            sesh.AutoPlayStart();
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

    public override void S_SpawnMonsterHandler(Session session, IPacket packet)
    {
        
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
            sesh.Player.OriginStat = res.originStat;
            sesh.Player.AdditionalStat = res.addStat;
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
        
    }
}
