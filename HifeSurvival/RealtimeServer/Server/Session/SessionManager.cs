using System.Collections.Generic;

namespace Server
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }
		Dictionary<int, ServerSession> _sessionDict = new Dictionary<int, ServerSession>();

		//NOTE : 20230710 : 현재는 DB가 의미가 없으므로, 세션 ID 을 특정 값부터 증가시켜 사용한다. 
		int _sessionId = DEFINE.PC_BEGIN_ID;
		object _lock = new object();

		public ServerSession Generate()
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				var session = new ServerSession()
				{
					SessionId = sessionId,
				};
				_sessionDict.Add(sessionId, session);

                Logger.Instance.Info($"Connected : {sessionId}");
				return session;
			}
		}

		public ServerSession Find(int id)
		{
			lock (_lock)
			{
				_sessionDict.TryGetValue(id, out var session);
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
