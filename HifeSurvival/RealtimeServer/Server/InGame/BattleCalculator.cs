using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class BattleCalculator
    {
        public static int ComputeAttackValue(in EntityStat atkStat)
        {
            return new Random().Next((int)(atkStat.Str * 0.8)  , (int)(atkStat.Str * 1.3));
        }

        public static int ComputeDamagedValue(in EntityStat atkStat, in EntityStat defStat)
        {
            return (ComputeAttackValue(atkStat) - (new Random().Next((int)(defStat.Def * 0.2f), (int)(defStat.Def * 0.4f))));
        }

        public static bool CanAttackDistance(in Entity self , in Entity target)
        {
            return self.currentPos.DistanceTo(target.currentPos) <= self.stat.AttackRange + target.stat.BodyRange;
        }

        public static bool IsOutOfSpawnArea(in Entity entity)
        {
            return entity.currentPos.DistanceTo(entity.spawnPos) > DEFINE.MONSTER_RESPAWN_AREA_RANGE;
        }
    }
}
