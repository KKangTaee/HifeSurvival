using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ServerCore;

namespace Server
{
    public struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int ExecTime;
        public Task CancellableTask;       //TODO : 취소 기능 들어가야함. 
        public Action NonCancelTask;

        public JobTimerElem(Action action, int tickAfter)
        {
            ExecTime = Environment.TickCount + tickAfter;
            CancellableTask = null;
            NonCancelTask = action;
        }

        public JobTimerElem(Task task, int tickAfter)
        {
            ExecTime = Environment.TickCount + tickAfter;
            NonCancelTask = null;
            CancellableTask = task;
        }

        public int CompareTo(JobTimerElem obj)
        {
            return ExecTime.CompareTo(obj.ExecTime);
        }
    }

    public class WorkQueue
    {
        private object _lock = new object();
        private List<JobTimerElem> _taskList = new List<JobTimerElem>();
        private Task _mainWork;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public void Start(string name)
        {
            _mainWork = Task.Run(() =>
            {
                Thread.CurrentThread.Name = name;
                var token = _cts.Token;
                while (!token.IsCancellationRequested)
                {
                    Flush();
                    Thread.Sleep(DEFINE.SERVER_TICK);
                }
            });
        }

        public void Stop()
        {
            _cts.Cancel();

            lock (_lock)
            {
                _taskList.Clear();
                _taskList = null;
            }
        }

        public void Push(Action action, int tickAfter = 0)
        {
            lock(_lock)
            {
                _taskList?.Add(new JobTimerElem(action, tickAfter));
            }
        }

        private void Flush()
        {
            int now = Environment.TickCount;
            lock(_lock)
            {
                _taskList?.Sort();

                int rmIndex = 0;
                for (int i = 0; i < _taskList?.Count; i++)
                {
                    if(now < _taskList?[i].ExecTime)
                    {
                        break;
                    }

                    rmIndex = i + 1;
                    if (_taskList?[i].NonCancelTask != null)
                    {
                        _taskList?[i].NonCancelTask.Invoke();
                    }
                    else
                    {
                        _taskList?[i].CancellableTask.Start();
                    }
                }

                if(rmIndex != 0)
                {
                    _taskList?.RemoveRange(0, rmIndex);
                }
            }
        }
    }
}
