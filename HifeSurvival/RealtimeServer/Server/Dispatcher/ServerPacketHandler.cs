using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    public class ServerPacketHandler : PacketHandler
    {
        private void push(Session session, Action<GameRoom> job)
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
            push(session, room => { room?.Mode.OnRecvSelect(packet as CS_SelectHero); });
        }

        public override void C_JoinToGameHandler(Session session, IPacket packet)
        {
            var sesh = session as ServerSession;
            if (sesh == null || sesh.Room == null)
            {
                return;
            }

            GameRoom room = sesh.Room;
            room.Push(() => room?.Mode.OnRecvJoin(packet as C_JoinToGame, sesh.SessionId));
        }

        public override void CS_AttackHandler(Session session, IPacket packet)
        {
            CS_Attack attack = packet as CS_Attack;
            push(session, room => room?.Mode.OnRecvAttack(attack));
        }

        public override void CS_ReadyToGameHandler(Session session, IPacket packet)
        {
            CS_ReadyToGame readyToGame = packet as CS_ReadyToGame;
            push(session, room => room?.Mode.OnRecvReady(readyToGame));
        }

        public override void MoveRequestHandler(Session session, IPacket packet)
        {
            MoveRequest move = packet as MoveRequest;
            push(session, room => room?.Mode.OnRecvMoveRequest(move));
        }

        public override void IncreaseStatRequestHandler(Session session, IPacket packet)
        {
            var req = packet as IncreaseStatRequest;
            push(session, room => room?.Mode.OnRecvIncreaseStatRequest(req));
        }

        public override void PickRewardRequestHandler(Session session, IPacket packet)
        {
            var req = packet as PickRewardRequest;
            push(session, room => room?.Mode.OnRecvPickRewardRequest(req));
        }

        public override void PlayStartRequestHandler(Session session, IPacket packet)
        {
            var req = packet as PlayStartRequest;
            push(session, room => room?.Mode.OnPlayStartRequest(req));
        }

        public override void CheatRequestHandler(Session session, IPacket packet)
        {
            var sesh = session as ServerSession;
            if (sesh == null)
            {
                return;
            }

            var req = packet as CheatRequest;
            push(session, room => room?.Mode.OnCheatRequest(sesh.SessionId, req));
        }


        #region NOT_HANDLED
        public override void S_JoinToGameHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void S_LeaveToGameHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void S_CountdownHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void S_StartGameHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void S_DeadHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void S_RespawnHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void S_SpawnMonsterHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void UpdateLocationBroadcastHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void MoveResponseHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void IncreaseStatResponseHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void UpdateStatBroadcastHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void PickRewardResponseHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void UpdateRewardBroadcastHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void UpdatePlayerCurrencyHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void PlayStartResponseHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void UpdateGameModeStatusBroadcastHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void UpdateInvenItemHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public override void CheatResponseHandler(Session session, IPacket packet)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}