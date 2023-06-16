using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    public class ServerPacketHandler : PacketHandler
    {
        public override void CS_SelectHeroHandler(PacketSession session, IPacket packet) 
        {
            CS_SelectHero selectHero = packet as CS_SelectHero;
            Push(session, room => { room?.Mode.OnRecvSelect(selectHero); });
        }

        public override void C_JoinToGameHandler(PacketSession session, IPacket packet)
        {
            C_JoinToGame joinToGame = packet as C_JoinToGame;
            ClientSession client = session as ClientSession;

            if (client.Room == null)
                return;

            GameRoom room = client.Room;

            room.Push(() => room?.Mode.OnRecvJoin(joinToGame, client.SessionId));
        }

        public override void CS_AttackHandler(PacketSession session, IPacket packet)
        {
            CS_Attack attack = packet as CS_Attack;
            Push(session, room => room?.Mode.OnRecvAttack(attack));
        }

        public override void CS_MoveHandler(PacketSession session, IPacket packet)
        {
            CS_Move move = packet as CS_Move;
            Push(session, room => room?.Mode.OnRecvMove(move));
        }

        public override void CS_StopMoveHandler(PacketSession session, IPacket packet)
        {
            CS_StopMove stopMove = packet as CS_StopMove;
            Push(session, room => room?.Mode.OnRecvStopMove(stopMove));
        }

        public override void CS_ReadyToGameHandler(PacketSession session, IPacket packet)
        {
            CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
            Push(session, room => room?.Mode.OnRecvReady(readyToGame));
        }

        public override void CS_UpdateStatHandler(PacketSession session, IPacket packet)
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
            room?.Push(() => job?.Invoke(room));
        }

        public override void S_JoinToGameHandler(PacketSession session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void S_LeaveToGameHandler(PacketSession session, IPacket packet)
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
    }
}