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
        public Task TaskUnit;       //TODO : 취소 기능 들어가야함. 

        public JobTimerElem(Action action, int tickAfter)
        {
            ExecTime = Environment.TickCount + tickAfter;
            TaskUnit = new Task(action);
        }

        public int CompareTo(JobTimerElem obj)
        {
            return ExecTime.CompareTo(obj.ExecTime);
        }
    }

    public class WorkManager
    {
        private object _lock = new object();
        private List<JobTimerElem> _taskList = new List<JobTimerElem>();
        private Task _mainWork;
        private string _name;
        private bool _isRun;

        public WorkManager(string name)
        {
            _name = name;
        }

        public void Start()
        {
            _mainWork = Task.Run(() =>
            {
                Thread.CurrentThread.Name = _name;
                while (_isRun)
                {
                    Flush();
                    Thread.Sleep(DEFINE.SERVER_TICK);
                }
            });

            _isRun = true;
        }

        public void Stop()
        {
            _taskList.Clear();
            _isRun = false;
        }

        public void Push(Action action, int tickAfter = 0)
        {
            lock(_lock)
            {
                _taskList.Add(new JobTimerElem(action, tickAfter));
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
                    if(_taskList[i].ExecTime > now)
                    {
                        break;
                    }

                    rmIndex = i + 1;
                    _taskList[i].TaskUnit.Start();
                }

                if(rmIndex != 0)
                {
                    _taskList.RemoveRange(0, rmIndex);
                }
            }
        }
    }
}
