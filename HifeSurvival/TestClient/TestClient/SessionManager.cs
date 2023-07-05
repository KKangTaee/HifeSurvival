using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace TestClient
{
    class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        List<ClientSession> _sessionList = new List<ClientSession>();
        object _lock = new object();


        public ClientSession Generate()
        {
            lock (_lock)
            {
                ClientSession session = new ClientSession();
                _sessionList.Add(session);
                return session;
            }
        }
    }
}
