using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Necromancer.Skills.EffectConditions
{
    public class DeathRitualCondition : EffectCondition
    {
        public int RequiredSummonEquipment = -1;

        protected override bool CheckIsValid(Character _affectedCharacter)
        {
            var targetSummon = SummonManager.FindWeakestSummon(_affectedCharacter.UID);

            if (targetSummon && targetSummon.GetComponentInChildren<Character>() is Character c && c.Inventory.HasEquipped(RequiredSummonEquipment))
            {
                return true;
            }

            return false;
        }
    }
}
