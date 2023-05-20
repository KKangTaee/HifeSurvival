using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Linq;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		
		static void Main(string[] args)
		{
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
			endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.9"), 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			
			Console.WriteLine("Listening...");

			while (true)
			{
				JobTimer.Instance.Flush();
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
