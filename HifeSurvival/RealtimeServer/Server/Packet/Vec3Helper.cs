using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class Vec3Helper
    {
        public static Vec3 Lerp(in Vec3 v1, Vec3 v2, float t)
        {
            return new Vec3
            {
                x = v1.x + (v2.x - v1.x) * t,
                y = v1.y + (v2.y - v1.y) * t,
                z = v1.z + (v2.z - v1.z) * t
            };
        }
    }
}
