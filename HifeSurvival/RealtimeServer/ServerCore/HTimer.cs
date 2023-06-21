using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class HTimer
    {
        public static long GetCurrentTimestamp()
        {
            var dto = new DateTimeOffset();
            return dto.ToUnixTimeMilliseconds();
        }
    }
}
