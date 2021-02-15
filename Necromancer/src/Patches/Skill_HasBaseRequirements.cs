using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Necromancer.Patches
{
    // patch for skill activation which require having an active summon

    [HarmonyPatch(typeof(Skill), "HasBaseRequirements")]
    public class Skill_HasBaseRequirements
    {
        [HarmonyPrefix]
        public static bool Prefix(Skill __instance, bool _tryingToActivate, ref bool __result)
        {
            if (_tryingToActivate)
            {
                var self = __instance;

                // custom check for Life Ritual and Death Ritual (requires a summoned skeleton)
                if (self.ItemID == 8890105 || self.ItemID == 8890106)
                {
                    if (!SummonManager.FindWeakestSummon(self.OwnerCharacter.UID))
                    {
                        self.OwnerCharacter.CharacterUI.ShowInfoNotification("You need a Summon to do that!");
                        __result = false;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
