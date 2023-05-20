using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using ServerCore;
using System;
using System.Threading.Tasks;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;
    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject();
                obj.name = nameof(NetworkManager);
                
                DontDestroyOnLoad(obj);

                _instance = obj.AddComponent<NetworkManager>();
            }

            return _instance;
        }
    }

    private ServerSession _session;

    private SimpleTaskCompletionSource<bool> _connectCompleted = new SimpleTaskCompletionSource<bool>();
    private SimpleTaskCompletionSource<bool> _disconnectCompleted = new SimpleTaskCompletionSource<bool>();

    public ServerSession SessionSelf => _session;

    private void Update()
    {
        var packet = PacketQueue.Instance.Pop();

        if (packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    public async Task<bool> ConnectAsync()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Debug.Log($"접속IP : {endPoint}");
        endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.9"), 7777);
        //endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

        _session = new ServerSession();
        Connector connector = new Connector();
        connector.Connect(endPoint, () => _session);

        _connectCompleted.Reset();

        var waitResult = await _connectCompleted.Wait(5000);

        if (waitResult.isSuccess == false)
            return false;

        return waitResult.result;
    }

    public async Task<bool> DisconnectAsync()
    {
        _disconnectCompleted.Reset();
        
        _session.Disconnect();

        var waitResult = await _disconnectCompleted.Wait(2000);

        if (waitResult.isSuccess == false)
            return false;

        return waitResult.result;
    }


    public void Send(IPacket inPacket)
    {
        _session.Send(inPacket.Write());
    }

    public void OnConnectResult(bool inResult)
    {
        _connectCompleted.Signal(inResult);
    }

    public void OnDisconnectResult(bool inResult)
    {
        _disconnectCompleted.Signal(inResult);
    }
}


public class SimpleTaskCompletionSource<T>
{
    TaskCompletionSource<T> _source;

    public SimpleTaskCompletionSource()
    {
        Reset();
    }

    public void Reset()
    {
        _source = new TaskCompletionSource<T>();
    }

    public async Task<(bool isSuccess, T result)> Wait(int millisecondsDelay)
    {
        if (_source.Task.IsCompleted)
        {
            Debug.Log("이미 완료");
            return (true, _source.Task.Result);
        }

        var completedTask = await Task.WhenAny(_source.Task, Task.Delay(millisecondsDelay));

        if (completedTask != _source.Task)
        {
            Debug.LogError($"Timeout!  millisecondsDelay : {millisecondsDelay}");
            return (false, default(T));
        }

        return (true, _source.Task.Result);
    }

    public void Signal(T inSignal)
    {
        _source.SetResult(inSignal);
    }
}
