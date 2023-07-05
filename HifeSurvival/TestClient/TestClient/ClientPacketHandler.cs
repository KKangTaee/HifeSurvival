using System;
using ServerCore;
using TestClient;

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
    }

    public override void S_LeaveToGameHandler(Session session, IPacket packet)
    {
        
    }

    public override void CS_SelectHeroHandler(Session session, IPacket packet)
    {
        
    }

    public override void CS_ReadyToGameHandler(Session session, IPacket packet)
    {
        
    }

    public override void S_CountdownHandler(Session session, IPacket packet)
    {
        
    }

    public override void S_StartGameHandler(Session session, IPacket packet)
    {
        
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
        
    }

    public override void PlayStartRequestHandler(Session session, IPacket packet)
    {
        
    }

    public override void PlayStartResponseHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdateGameModeStatusBroadcastHandler(Session session, IPacket packet)
    {
        
    }

    public override void UpdateInvenItemHandler(Session session, IPacket packet)
    {
        
    }
}
