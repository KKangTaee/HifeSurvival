using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class ServerTime
    {
        public static long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
