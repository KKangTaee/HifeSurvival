using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;
using System.Linq;

namespace Server
{
    public static class PacketExtensionHelper
    {
        public static bool IsSame(this PVec3 selfPos, in PVec3 otherPos)
        {
            var dist = selfPos.DistanceTo(otherPos);
            return dist < 0.25f;     //해당 값보다 작으면, 같다고 상정한다.
        }

        public static bool IsDifferent(this PVec3 selfPos, in PVec3 otherPos)
        {
            return !IsSame(selfPos, otherPos);
        }

        public static PVec3 AddPVec3(this PVec3 selfPos, in PVec3 otherPos)
        {
            return new PVec3()
            {
                x = selfPos.x + otherPos.x,
                y = selfPos.y + otherPos.y,
                z = selfPos.z + otherPos.z
            };
        }

        public static PVec3 MulitflyPVec3(this PVec3 selfPos, float factor)
        {
            return new PVec3()
            {
                x = selfPos.x * factor,
                y = selfPos.y * factor,
                z = selfPos.z * factor
            };
        }

        public static PVec3 SubtractPVec3(this PVec3 selfPos, in PVec3 otherPos)
        {
            return new PVec3()
            {
                x = selfPos.x - otherPos.x,
                y = selfPos.y - otherPos.y,
                z = selfPos.z - otherPos.z
            };
        }

        public static PVec3 NormalizePVec3(this PVec3 selfPos)
        {
            float length = (float)Math.Sqrt(selfPos.x * selfPos.x + selfPos.y * selfPos.y + selfPos.z * selfPos.z);

            if (length > 0)
            {
                return new PVec3
                {
                    x = selfPos.x / length,
                    y = selfPos.y / length,
                    z = selfPos.z / length
                };
            }
            else
            {
                throw new InvalidOperationException("Cannot normalize a zero vector.");
            }
        }

        public static PVec3 NormalizeToTargetPVec3(this PVec3 selfPos, in PVec3 targetPos)
        {
            return targetPos.SubtractPVec3(selfPos).NormalizePVec3();
        }

        public static float DistanceTo(this PVec3 selfPos, in PVec3 targetPos)
        {
            float dx = targetPos.x - selfPos.x;
            float dy = targetPos.y - selfPos.y;
            float dz = targetPos.z - selfPos.z;

            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static string Print(this PVec3 selfPos)
        {
            return $"[{selfPos.x}, {selfPos.y}, {selfPos.z}]";
        }

        private static Random _rand = new Random();

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static PStat ConvertToPStat(this EntityStat selfEntity)
        {
            return new PStat()
            {
                str = selfEntity.Str,
                def = selfEntity.Def,
                hp = selfEntity.MaxHp,
                attackSpeed = selfEntity.AttackSpeed,
                moveSpeed = selfEntity.MoveSpeed,
                detectRange = selfEntity.DetectRange,
                attackRange = selfEntity.AttackRange,
                bodyRange = selfEntity.BodyRange,
            };
        }

        public static string FilterRewardIdsByRandomProbability(this string rewardIdStr)
        {
            var split = rewardIdStr.Split(':');


            // NOTE@taeho.kang 만약 1:0:50 으로 값이 들어올 경우, 100퍼 확률
            if (split?.Length == 3)
            {
                return rewardIdStr;
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
                    Logger.Instance.Debug("Drop Failed By Probability");
                    return null;
                }
            }

            Logger.Instance.Error("rewardIds is wrong! check static sheet");
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
