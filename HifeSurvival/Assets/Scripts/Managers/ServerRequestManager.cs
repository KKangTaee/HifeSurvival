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
        public Action<bool> doneCallback;
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
                        requestData.doneCallback?.Invoke(false);
                    }
                    else
                    {
                        // TODO@taeho.kang do something.
                        Debug.Log(webRequest.downloadHandler.text);
                        requestData.doneCallback?.Invoke(true);
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
    
    
    private string apiKey       = "AIzaSyABnRmQck9SP3Gv7syjremXAjDBDOky8so";
    private string sheetId      = "104ZnnXWWorMZOAhuY0o1o1xIL2H41opJlrJLsSEk_C4";
    private string sheetName    = "systems";
    private string sheetsApiUrl = "https://sheets.googleapis.com/v4/spreadsheets";

    public void Test()
    {
        AddRequestData(new ServerRequestData()
        {
            URL =  $"{sheetsApiUrl}/{sheetId}/values/{sheetName}?key={apiKey}"
        });
    }
    
}
