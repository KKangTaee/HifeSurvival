using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ServerCore;

namespace Server
{
	public struct JobTimerElem
	{
		public int ExecTime;
		public Task TaskUnit;		//TODO : 취소 기능 들어가야함. 

		public JobTimerElem(Action action, int tickAfter)
        {
			ExecTime = Environment.TickCount + tickAfter;
			TaskUnit = new Task(action);
		}
	}

	public class WorkManager
	{
		private ConcurrentQueue<JobTimerElem> _taskQueue = new ConcurrentQueue<JobTimerElem>();

		public void ClearTimer()
        {
			_taskQueue.Clear();
		}

		public void Push(Action action, int tickAfter = 0)
		{
			_taskQueue.Enqueue(new JobTimerElem(action, tickAfter));
		}

		public void Flush()
		{
			_taskQueue.OrderBy(t => t.ExecTime);

			int now = Environment.TickCount;
            while (_taskQueue.Count != 0)
            {
                _taskQueue.TryPeek(out var job);
                if (job.ExecTime > now)
                {
                    break;
                }

				_taskQueue.TryDequeue(out var doneJob);
				doneJob.TaskUnit.Start();
			}
		}
	}
}
