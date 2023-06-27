using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class BattleCalculator
    {
        public static int ComputeAttackValue(in EntityStat atkStat)
        {
            return new Random().Next(atkStat.str - 15, (int)(atkStat.str * 1.2f));
        }

        public static int ComputeDamagedValue(in EntityStat atkStat, in EntityStat defStat)
        {
            return (int)(ComputeAttackValue(atkStat) - defStat.def * 0.1f);
        }

        public static bool CanAttackTarget(in Entity self , in Entity target)
        {
            return self.currentPos.DistanceTo(target.currentPos) < self.stat.attackRange;
        }

        public static bool IsOutOfSpawnArea(in Entity entity)
        {
            return entity.currentPos.DistanceTo(entity.spawnPos) > DEFINE.MONSTER_RESPAWN_AREA_RANGE;
        }
    }
}
