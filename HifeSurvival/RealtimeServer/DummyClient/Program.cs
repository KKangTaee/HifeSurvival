﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using ServerCore;

namespace DummyClient
{
	class Program
	{
		static void Main(string[] args)
		{
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.3"), 7777);
            // System.Console.WriteLine(ipAddr.Address);

            PacketManager.Instance.BindHandler(new ClientPacketHandler());

            Connector connector = new Connector();

			connector.Connect(endPoint, 
				() => { return SessionManager.Instance.Generate(); },
				1);

			while (true)
			{
				try
				{
					// SessionManager.Instance.SendForEach();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				Thread.Sleep(250);
			}
		}
	}
}
