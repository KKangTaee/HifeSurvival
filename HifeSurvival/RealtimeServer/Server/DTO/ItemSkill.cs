using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class ItemSkill
    {
        public int SkillKey { get; private set; }
        public int CoolTime { get; private set; }
        public int FormulaKey { get; private set; }

        public ItemSkill(ItemSkillData skillData)
        {
            SkillKey = skillData.key;
            CoolTime = skillData.coolTime;
            FormulaKey = skillData.formulaKey;
        }
    }
}
