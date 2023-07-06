using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }
		Dictionary<int, ServerSession> _sessionDict = new Dictionary<int, ServerSession>();
		
		int _sessionId = 0;
		object _lock = new object();

		public ServerSession Generate()
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				ServerSession session = new ServerSession();
				session.SessionId = sessionId;
				_sessionDict.Add(sessionId, session);

                Logger.Instance.Info($"Connected : {sessionId}");

				return session;
			}
		}

		public ServerSession Find(int id)
		{
			lock (_lock)
			{
				ServerSession session = null;
				_sessionDict.TryGetValue(id, out session);
				return session;
			}
		}

		public void Remove(ServerSession session)
		{
			lock (_lock)
			{
				_sessionDict.Remove(session.SessionId);
			}
		}
	}
}
