using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    public class ServerPacketHandler : PacketHandler
    {
        private void PushJob(Session session, Action<GameRoom> job)
        {
            var sesh = session as ServerSession;
            if (sesh == null || sesh.Room == null)
            {
                return;
            }

            sesh.Room.Push(() => job?.Invoke(sesh.Room));
        }

        public override void CS_SelectHeroHandler(Session session, IPacket packet)
        {
            PushJob(session, room => { room?.Mode.OnRecvSelect(packet as CS_SelectHero); });
        }

        public override void C_JoinToGameHandler(Session session, IPacket packet)
        {
            var sesh = session as ServerSession;
            if (sesh == null || sesh.Room == null)
            {
                return;
            }

            var room = sesh.Room;
            room.Push(() => room?.Mode.OnRecvJoin(packet as C_JoinToGame, sesh.SessionId));
        }

        public override void CS_AttackHandler(Session session, IPacket packet)
        {
            var attack = packet as CS_Attack;
            PushJob(session, room => room?.Mode.OnRecvAttack(attack));
        }

        public override void CS_ReadyToGameHandler(Session session, IPacket packet)
        {
            var readyToGame = packet as CS_ReadyToGame;
            PushJob(session, room => room?.Mode.OnRecvReady(readyToGame));
        }

        public override void MoveRequestHandler(Session session, IPacket packet)
        {
            var move = packet as MoveRequest;
            PushJob(session, room => room?.Mode.OnRecvMoveRequest(move));
        }

        public override void IncreaseStatRequestHandler(Session session, IPacket packet)
        {
            var req = packet as IncreaseStatRequest;
            PushJob(session, room => room?.Mode.OnRecvIncreaseStatRequest(req));
        }

        public override void PickRewardRequestHandler(Session session, IPacket packet)
        {
            var req = packet as PickRewardRequest;
            PushJob(session, room => room?.Mode.OnRecvPickRewardRequest(req));
        }

        public override void PlayStartRequestHandler(Session session, IPacket packet)
        {
            var req = packet as PlayStartRequest;
            PushJob(session, room => room?.Mode.OnPlayStartRequest(req));
        }

        public override void CheatRequestHandler(Session session, IPacket packet)
        {
            var sesh = session as ServerSession;
            if (sesh == null)
            {
                return;
            }

            var req = packet as CheatRequest;
            PushJob(session, room => room?.Mode.OnCheatRequest(sesh.SessionId, req));
        }
    }
}