using DummyClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ServerCore;

public class ClientPacketHandler : PacketHandler
{
    public override void C_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_JoinToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_LeaveToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void CS_SelectHeroHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void CS_ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_CountdownHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_StartGameHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void CS_AttackHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_DeadHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_RespawnHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_SpawnMonsterHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_DropRewardHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void C_PickRewardHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_GetItemHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void S_GetGoldHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void MoveRequestHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
    
    public override void UpdateLocationBroadcastHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void MoveResponseHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void UpdateStatBroadcastHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void IncreaseStatRequestHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }

    public override void IncreaseStatResponseHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
}
