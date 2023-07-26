using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ServerCore;

namespace Server
{
    public struct WorkItem : IComparable<WorkItem>
    {
        public bool CanCancel;
        public int ExecTime;
        public Task CancellableTask;       //TODO : 취소 기능 들어가야함. 
        public Action Job;

        public WorkItem(Action action, int tickAfter)
        {
            CanCancel = false;
            ExecTime = Environment.TickCount + tickAfter;
            CancellableTask = null;
            Job = action;
        }

        public WorkItem(Task task, int tickAfter)
        {
            CanCancel = true;
            ExecTime = Environment.TickCount + tickAfter;
            CancellableTask = task;
            Job = null;
        }

        public int CompareTo(WorkItem obj)
        {
            return ExecTime.CompareTo(obj.ExecTime);
        }
    }

    public class WorkQueue
    {
        private object _lock = new object();
        private List<WorkItem> _taskList = new List<WorkItem>();
        private Task _mainWork;
        private CancellationTokenSource _mainCts = new CancellationTokenSource();

        public void Start(string name)
        {
            _mainWork = Task.Run(() =>
            {
                Thread.CurrentThread.Name = name;
                var token = _mainCts.Token;
                while (!token.IsCancellationRequested)
                {
                    Flush();
                    Thread.Sleep(DEFINE.SERVER_TICK);
                }

                _taskList.Clear();
                _taskList = null;
            });
        }

        public void Stop()
        {
            _mainCts.Cancel();
            _mainCts = null;
        }

        public void Push(Action action, int tickAfter = 0)
        {
            lock(_lock)
            {
                _taskList?.Add(new WorkItem(action, tickAfter));
            }
        }

        private void Flush()
        {
            int now = Environment.TickCount;
            lock(_lock)
            {
                _taskList.Sort();

                int rmIndex = 0;
                for (int i = 0; i < _taskList.Count; i++)
                {
                    if(now < _taskList[i].ExecTime)
                    {
                        break;
                    }

                    rmIndex = i + 1;
                    if (_taskList[i].CanCancel)
                    {
                        _taskList[i].CancellableTask.Start();
                    }
                    else
                    {
                        _taskList[i].Job.Invoke();
                    }
                }

                if(rmIndex != 0)
                {
                    _taskList.RemoveRange(0, rmIndex);
                }
            
            }
        }
    }
}
