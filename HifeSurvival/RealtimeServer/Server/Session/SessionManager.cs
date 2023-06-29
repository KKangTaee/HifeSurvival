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
		Dictionary<int, ClientSession> _sessionDict = new Dictionary<int, ClientSession>();
		
		int _sessionId = 0;
		object _lock = new object();

		public ClientSession Generate()
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				ClientSession session = new ClientSession();
				session.SessionId = sessionId;
				_sessionDict.Add(sessionId, session);

                Logger.GetInstance().Info($"Connected : {sessionId}");

				return session;
			}
		}

		public ClientSession Find(int id)
		{
			lock (_lock)
			{
				ClientSession session = null;
				_sessionDict.TryGetValue(id, out session);
				return session;
			}
		}

		public void Remove(ClientSession session)
		{
			lock (_lock)
			{
				_sessionDict.Remove(session.SessionId);
			}
		}
	}
}
