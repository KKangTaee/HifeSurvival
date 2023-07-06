using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    using ServerCore;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ServerRequestManager
    {
        private static ServerRequestManager _instance;
        private HttpClient _client;

        public static ServerRequestManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServerRequestManager();
                }

                return _instance;
            }
        }

        public struct ServerRequestData
        {
            public string URL;
            public Action<string> doneCallback;
        }

        private Queue<ServerRequestData> _requestQueue = new Queue<ServerRequestData>();
        private bool _isRunning = false;
        private CancellationTokenSource _cts;

        private ServerRequestManager()
        {
            _client = new HttpClient();
        }

        public void AddRequestData(ServerRequestData data)
        {
            _requestQueue.Enqueue(data);

            if (!_isRunning)
            {
                _cts = new CancellationTokenSource();
                _ = RequestToServer();
            }
        }

        private async Task RequestToServer()
        {
            _isRunning = true;

            while (_requestQueue.Count > 0)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    break;
                }

                var requestData = _requestQueue.Dequeue();
                try
                {
                    var response = await _client.GetAsync(requestData.URL, _cts.Token);
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Instance.Error($"status code : {response.StatusCode}");
                        requestData.doneCallback?.Invoke(null);
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        requestData.doneCallback?.Invoke(content);
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Error($"Exception {e.Message}");
                }
            }

            _isRunning = false;
        }

        public void Clear()
        {
            _requestQueue.Clear();
            _isRunning = false;
            _cts.Cancel();
        }
    }
}