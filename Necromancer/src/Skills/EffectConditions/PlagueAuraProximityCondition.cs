using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Necromancer.Skills.EffectConditions
{
    public class PlagueAuraProximityCondition : ProximityCondition
    {
        public int RequiredActivatedItemID = 8999050; // Activated Plague Aura

        // This can be used as a component, or by simply calling IsInsidePlagueAura(Vector3 _position) from anywhere

        protected override bool CheckIsValid(Character _affectedCharacter)
        {
            if (IsInsidePlagueAura(_affectedCharacter.transform.position, RequiredActivatedItemID)) 
            { 
                return true; 
            }

            return false;
            //return base.CheckIsValid(_affectedCharacter);
        }

        public static bool IsInsidePlagueAura(Vector3 _position, int itemID = 8999050)
        {
            List<Character> charsInRange = new List<Character>();
            CharacterManager.Instance.FindCharactersInRange(_position, 2.5f, ref charsInRange);

            foreach (Character charInRange in charsInRange)
            {
                var ephemerals = charInRange.GetComponentsInChildren<Ephemeral>();
                foreach (var ephemeral in ephemerals)
                {
                    if (ephemeral.Item.ItemID == itemID)
                        return true;
                }
            }

            return false;
        }
    }
}
