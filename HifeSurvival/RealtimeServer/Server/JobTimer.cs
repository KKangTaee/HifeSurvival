using System;
using ServerCore;

namespace Server
{
	struct JobTimerElem : IComparable<JobTimerElem>
	{
		public int execTick; // 실행 시간
		public Action action;

		public int CompareTo(JobTimerElem other)
		{
			return other.execTick - execTick;
		}
	}

	class JobTimer
	{
		PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
		object _lock = new object();

		public static JobTimer Instance { get; } = new JobTimer();

		public void Push(Action action, int tickAfter = 1)
		{
			JobTimerElem job;
			job.execTick = System.Environment.TickCount + tickAfter;
			job.action = action;

			lock (_lock)
			{
				_pq.Push(job);
			}
		}

		public void Flush()
		{
			int now = System.Environment.TickCount;

			lock(_lock)
            {
				JobTimerElem job;
				while (_pq.Count != 0)
                {
					job = _pq.Peek();
					if (job.execTick > now)
                    {
						break;
					}

					job.action.Invoke();
					_pq.Pop();
				}
            }
		}
	}
}
