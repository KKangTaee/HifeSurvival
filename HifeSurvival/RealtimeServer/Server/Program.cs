﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class Program
	{
		static Listener _listener = new Listener();
		
		static async Task Main(string[] args)
		{
			Logger.Instance.Log("INF", "Server", "Start");

			// DNS (Domain Name System)
			// string host = Dns.GetHostName();
			// IPHostEntry ipHost = Dns.GetHostEntry(host);
			// IPAddress ipAddr = ipHost.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
			await GameData.Instance.Init();

			//TODO : 포트 번호 Config로 관리하기. 
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);
            // endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.3"), 7777);

            PacketManager.Instance.BindHandler(new ServerPacketHandler());
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Logger.Instance.Log("INF", "Listening", "Start");

			while (true)
			{
				JobTimer.Instance.Flush();
				Thread.Sleep(DEFINE.SERVER_TICK);
			}
		}
    }
}

// 어드레스 리스트
// 0 : {::1} 		// IPv6 주소
// 1 : {fe80::1%1}  // IPv6 주소
// 2 : {127.0.0.1}  // Loopback 주소 (로컬 머신내에서만 접속 가능)
// 3 : {192.168.0.9} // 로컬네트워크 주소
// 4 : {fe80::1060:a4d5:6455:27a8%6} // IPv6 주소