using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    //public class Timer
    //{
    //    public int timerMatchId;
    //}

    //class SampleTimer
    //{
    //    private int timerSeq = 0;

    //    Dictionary<int/*id*/, Dictionary<int /*timer type*/, Timer>> timerTable = new Dictionary<int, Dictionary<int, Timer>>();
    //    Dictionary<int/*timerSeq*/, Timer> timerList;

    //    public SampleTimer()
    //    {
    //        timerList = Enumerable.Range(0, DEFINE.TIMER_SEQ_MAX_PER_GAME).ToDictionary<int, Timer>(k => k , v => new Timer());
    //    }

    //    private void NextSeq()
    //    {
    //        var tempSeq = Interlocked.Increment(ref timerSeq);
    //        if (tempSeq == DEFINE.TIMER_SEQ_MAX_PER_GAME - 1)
    //        {
    //            Interlocked.Exchange(ref timerSeq, 0);
    //        }
    //    }

    //    public void AddTimer(int entityId, int timerType, Timer timer)
    //    {

    //    }


    }
}
