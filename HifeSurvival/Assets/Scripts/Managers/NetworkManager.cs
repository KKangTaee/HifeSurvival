using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using ServerCore;
using System;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour 
{
    private static NetworkManager _instance;
    public static NetworkManager Instance
    {
        get
        {
            if(_instance == null)
            {
                var obj = new GameObject();
                obj.name = nameof(NetworkManager);
                DontDestroyOnLoad(obj);

               _instance = new NetworkManager();
            }

            return _instance;
        }
    }

    private ServerSession _session = new ServerSession();
    
    private void Update()
    {
        var packet = PacketQueue.Instance.Pop();

        if(packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    public void ConnectToRealtimeServer()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

        // IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();
        connector.Connect(endPoint,()=> _session);
    
        DummyTest();
    }


    public async void DummyTest()
    {
        await Task.Delay(1000);

        C_Chat chatPacket = new C_Chat();
        chatPacket.chat = $"유니티에서 보낸다 !";
        ArraySegment<byte> segment = chatPacket.Write();

        _session.Send(segment);
        Debug.Log("보냄!!");
    }
}
