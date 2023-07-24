using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.ComponentModel.DataAnnotations;

namespace Server
{
    class Program
	{
		private static Listener _listener = new Listener();

		//public static JobTimer MainJobTimer = new JobTimer();

		static void Main(string[] args)
		{
			Logger.Instance.Log("INF", "Server", "Start");
			ThreadPool.GetMaxThreads(out var wtc, out int cptc);
			Logger.Instance.Log("DBG",$"{Environment.OSVersion} core [{Environment.ProcessorCount}] worker [{wtc}] I/O [{cptc}]", "System");
			AppDomain.CurrentDomain.UnhandledException += Dump.UnhandledExceptionHandler;
            
            PacketManager.Instance.BindHandler(new ServerPacketHandler());
            _listener.Init(new IPEndPoint(IPAddress.Any, DEFINE.SERVER_PORT), () => { return SessionManager.Instance.Generate(); });

			GameData.Instance.Init().Wait();

			new Thread(() =>
			{
				Logger.Instance.Log("INF", "Listening", "Start");
				
				while (true)
				{
					Console.Title = $"Room Count : {GameRoomManager.Instance.GetRoomCount()}";
					//MainJobTimer.Flush();
					GameRoomManager.Instance.FlushAllRoom();
					Thread.Sleep(DEFINE.SERVER_TICK);
				}
			}, DEFINE.MAIN_THREAD_STACK_SIZE)
			{ Name = "Server Main" }.Start();

			return;
		}
    }
}