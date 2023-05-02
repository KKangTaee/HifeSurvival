using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx.Async;
using System;
using System.Threading;


public class ServerRequestManager
{
    private static ServerRequestManager _instance;

    public static ServerRequestManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ServerRequestManager();

            return _instance;
        }
    }


    //----------------
    // structs
    //----------------

    public struct ServerRequestData
    {
        public string URL;
        public Action<string> doneCallback;
    }



    //----------------
    // variables
    //----------------

    private Queue<ServerRequestData> _requestQueue = new Queue<ServerRequestData>();
    private bool _isRunning = false;
    private CancellationTokenSource _cts;



    //-----------------
    // functions
    //-----------------

    public void AddRequestData(ServerRequestData inData)
    {
        _requestQueue.Enqueue(inData);

        if (_isRunning == false)
        {
            _cts = new CancellationTokenSource();
            RequestToServer().Forget();
        }
    }


    private async UniTaskVoid RequestToServer()
    {
        while (_requestQueue?.Count > 0)
        {
            try
            {
                _cts.Token.ThrowIfCancellationRequested();
                var requestData = _requestQueue.Dequeue();

                using (UnityWebRequest webRequest = UnityWebRequest.Get(requestData.URL))
                {
                    var operation = webRequest.SendWebRequest();

                    while (operation.isDone == false)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
                    }

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[{nameof(RequestToServer)}] webRequest Error : {webRequest.error}");
                        requestData.doneCallback?.Invoke(null);
                    }
                    else
                    {
                        // TODO@taeho.kang do something.
                        requestData.doneCallback?.Invoke(webRequest.downloadHandler.text);
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.Log(e.Message);
                break;
            }

            _isRunning = true;
        }

        _isRunning = false;
    }


    public void Clear()
    {
        _requestQueue?.Clear();
        _isRunning = false;
        _cts?.Cancel();
    }
}
   // ServerRequestManager.Instance.AddRequestData(new ServerRequestManager.ServerRequestData()
        // {
        //     URL =  $"{sheetsApiUrl}/{sheetId}/values/{sheetName}?key={apiKey}",
        //     doneCallback = (jsonStr) =>
        //     {

        //     }
        // });