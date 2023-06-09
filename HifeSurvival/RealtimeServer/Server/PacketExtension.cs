using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class PacketExtension
    {
        public static Vec3 AddVec3(this Vec3 inSelf, in Vec3 inOther)
        {
            return new Vec3()
            {
                x = inSelf.x + inOther.x,
                y = inSelf.y + inOther.y,
                z = inSelf.z + inOther.z
            };
        }

        public static Vec3 MulitflyVec3(this Vec3 inSelf, float inOther)
        {
            return new Vec3()
            {
                x = inSelf.x * inOther,
                y = inSelf.y * inOther,
                z = inSelf.z * inOther
            };
        }

        public static Vec3 SubtractVec3(this Vec3 inSelf, in Vec3 inOther)
        {
            return new Vec3()
            {
                x = inSelf.x - inOther.x,
                y = inSelf.y - inOther.y,
                z = inSelf.z - inOther.z
            };
        }

        public static Vec3 NormalizeVec3(this Vec3 inSelf)
        {
            float length = (float)Math.Sqrt(inSelf.x * inSelf.x + inSelf.y * inSelf.y + inSelf.z * inSelf.z);

            if (length > 0)
            {
                return new Vec3
                {
                    x = inSelf.x / length,
                    y = inSelf.y / length,
                    z = inSelf.z / length
                };
            }
            else
            {
                throw new InvalidOperationException("Cannot normalize a zero vector.");
            }
        }

        public static float DistanceTo(this Vec3 inSelf, in Vec3 inOther)
        {
            float dx = inOther.x - inSelf.x;
            float dy = inOther.y - inSelf.y;
            float dz = inOther.z - inSelf.z;

            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static Random rand = new Random();

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static Stat ConvertStat(this EntityStat inSelf)
        {
            return new Stat()
            {
                str = inSelf.str,
                def = inSelf.def,
                hp  = inSelf.maxHp,
                attackSpeed = inSelf.attackSpeed,
                moveSpeed   = inSelf.moveSpeed,
            };
        }
    }
}
