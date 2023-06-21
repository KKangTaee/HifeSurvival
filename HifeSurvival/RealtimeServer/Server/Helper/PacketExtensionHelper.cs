using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;
using System.Linq;

namespace Server.Helper
{
    public static class PacketExtensionHelper
    {
        public static bool IsSame(this PVec3 inSelf, in PVec3 inOther)
        {
            var sbv = inSelf.SubtractPVec3(inOther);
            var gap = Math.Abs(sbv.x) + Math.Abs(sbv.y) + Math.Abs(sbv.z);
            return gap < 0.000001f;     //해당 값보다 작으면, 같다고 상정한다.
        }

        public static PVec3 AddPVec3(this PVec3 inSelf, in PVec3 inOther)
        {
            return new PVec3()
            {
                x = inSelf.x + inOther.x,
                y = inSelf.y + inOther.y,
                z = inSelf.z + inOther.z
            };
        }

        public static PVec3 MulitflyPVec3(this PVec3 inSelf, float inOther)
        {
            return new PVec3()
            {
                x = inSelf.x * inOther,
                y = inSelf.y * inOther,
                z = inSelf.z * inOther
            };
        }

        public static PVec3 SubtractPVec3(this PVec3 inSelf, in PVec3 inOther)
        {
            return new PVec3()
            {
                x = inSelf.x - inOther.x,
                y = inSelf.y - inOther.y,
                z = inSelf.z - inOther.z
            };
        }

        public static PVec3 NormalizePVec3(this PVec3 inSelf)
        {
            float length = (float)Math.Sqrt(inSelf.x * inSelf.x + inSelf.y * inSelf.y + inSelf.z * inSelf.z);

            if (length > 0)
            {
                return new PVec3
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

        public static float DistanceTo(this PVec3 inSelf, in PVec3 inOther)
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
                hp = inSelf.hp,
                attackSpeed = inSelf.attackSpeed,
                moveSpeed = inSelf.moveSpeed,
            };
        }

        public static string FilterRewardIdsByRandomProbability(this string inSelf)
        {
            var split = inSelf.Split(':');


            // NOTE@taeho.kang 만약 1:0:50 으로 값이 들어올 경우, 100퍼 확률
            if (split?.Length == 3)
            {
                return inSelf;
            }
            else if (split?.Length == 4)
            {
                var probability = Convert.ToInt32(split[3]);
                var random = new Random();
                var randomNumber = random.Next(100); // Generate random number between 0 to 99.

                if (randomNumber < probability)
                {
                    return string.Join(":", split.Take(3));
                }
                else
                {
                    return null;
                }
            }

            Logger.GetInstance().Error("rewardIds is wrong! check static sheet");
            return null;
        }

        public static PVec3 Lerp(in PVec3 v1, PVec3 v2, float t)
        {
            return new PVec3
            {
                x = v1.x + (v2.x - v1.x) * t,
                y = v1.y + (v2.y - v1.y) * t,
                z = v1.z + (v2.z - v1.z) * t
            };
        }
    }
}
