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

    private ClientSession _session;

    private SimpleTaskCompletionSource<bool> _connectCompleted = new SimpleTaskCompletionSource<bool>();
    private Action _disconnectCB;
    // private bool _isConnected = false;

    public ClientSession SessionSelf => _session;

    private JobQueue _jobQueue = new JobQueue();
    private string _ipAddr;


    //----------------
    // unity events
    //----------------

    private void Update()
    {
        var packet = PacketQueue.Instance.Pop();

        if (packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }


    public async Task<bool> ConnectAsync(string inIpAddr)
    {

         IPEndPoint endPoint = null;

        if(inIpAddr == null)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            endPoint = new IPEndPoint(ipAddr, 7777);
        }
        else
        {
            endPoint = new IPEndPoint(IPAddress.Parse(inIpAddr), 7777);
        }

        PacketManager.Instance.BindHandler(new ClientPacketHandler());

        // Debug.LogWarning($"접속IP : {endPoint}");

        // endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.3"), 7777);
        // endPoint = new IPEndPoint(IPAddress.Parse("61.83.232.18"), 7777);

        _session = new ClientSession();
        Connector connector = new Connector();
        connector.Connect(endPoint, () => _session);

        _connectCompleted.Reset();

        var waitResult = await _connectCompleted.Wait(5000);

        if (waitResult.isSuccess == false)
            return false;

        // _isConnected = waitResult.isSuccess;
        _ipAddr = inIpAddr;
        
        return waitResult.result;
    }


    private async void RetryConnect(Action<bool> doneCallback = null)
    {
        SimpleLoading.Show("다시 접속중입니다.");

        bool isSuccess = await ConnectAsync(_ipAddr);

        SimpleLoading.Hide();

        doneCallback?.Invoke(isSuccess);
    }

    public void Disconnect(Action inDisconnectCB = null)
    {
        AddEvent(inDisconnectCB);
        _session.Disconnect();
    }


    public void Send(IPacket inPacket)
    {
        _session.Send(inPacket.Write());
    }

    public void OnConnectResult(bool inResult)
    {
        _connectCompleted.Signal(inResult);
    }

    public void OnDisconnectResult()
    {
        Push(() =>
        {
            // NOTE@ytaeho.kang Retry 내부의 UnityEngine 을 사용하기 위해 Push로 래핑처리함.
            if(GameMode.Instance.Status != EGameModeStatus.None)
            {
                RetryConnect(isSuccess =>
                {
                    if (isSuccess == false)
                    {
                        // _isConnected = false;
                        _disconnectCB?.Invoke();
                    }
                });
            }
            else
            {
                // _isConnected = false;
                _disconnectCB?.Invoke();
            }
        });
    }

    public void AddEvent(Action inDisconnect)
    {
        _disconnectCB = inDisconnect;
    }

    public void Push(Action job)
    {
        _jobQueue.Push(job);
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

// Ready 캐릭터 선택창
// Countdown 
// LoadGame : 카운트다운 끝나고
// -> 클라에서 PlayStar를 리퀘스트한다.
// -> 리스폰을 받으면 대기한다.
// 플레이 스타트 