using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void CS_SelectHeroHandler(PacketSession session, IPacket packet)
    {
        CS_SelectHero selectHero = packet as CS_SelectHero;  
        Push(session, room => { room?.Mode.OnRecvSelect(selectHero); });
    }

    public static void C_JoinToGameHandler(PacketSession session, IPacket packet)
    {
		C_JoinToGame joinToGame = packet as C_JoinToGame;
		ClientSession client = session as ClientSession;

        if (client.Room == null)
            return;

        GameRoom room = client.Room;

        room.Push(() => room?.Mode.OnRecvJoin(joinToGame, client.SessionId));
    }

    public static void CS_AttackHandler(PacketSession session, IPacket packet)
    {
        CS_Attack attack = packet as CS_Attack;
        Push(session, room => room?.Mode.OnRecvAttack(attack));
    }

    public static void CS_MoveHandler(PacketSession session, IPacket packet)
    {
        CS_Move move = packet as CS_Move;
        Push(session, room => room?.Mode.OnRecvMove(move));
    }

    public static void CS_StopMoveHandler(PacketSession session, IPacket packet)
    {
        CS_StopMove stopMove = packet as CS_StopMove;
        Push(session, room => room?.Mode.OnRecvStopMove(stopMove));
    }

    public static void CS_ReadyToGameHandler(PacketSession session, IPacket packet)
    {
        CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
        Push(session, room => room?.Mode.OnRecvReady(readyToGame));
    }

    public static void CS_UpdateStatHandler(PacketSession session, IPacket packet)
    {
        CS_UpdateStat updateStat = packet as CS_UpdateStat;
        Push(session, room => room?.Mode.OnRecvUpdateStat(updateStat));
    }

    public static void Push(PacketSession session, Action<GameRoom> job)
    {
        ClientSession client = session as ClientSession;

        if (client.Room == null)
            return;

        GameRoom room = client.Room;
        room?.Push(()=> job?.Invoke(room));
    }

    internal static void C_GetItemHandler(PacketSession session, IPacket packet)
    {
        throw new NotImplementedException();
    }
}