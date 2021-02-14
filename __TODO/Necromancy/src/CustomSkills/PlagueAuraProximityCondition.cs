using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using OModAPI;

namespace NecromancerSkills
{
    public class PlagueAuraProximityCondition : ProximityCondition
    {
        public int RequiredActivatedItemID = -1;

        // a Plague Aura is a 2.5f-wide sigil attached to a player's visuals.
        // "Ephemeral" is a component normally found on Sigils, this is a lazy but fairly safe way of finding the sigil for now.
        // A better implementation would be by getting the specific Sigil Item ID, which I have shown how to do but commented out.

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

        public static bool IsInsidePlagueAura(Vector3 _position, int itemID = -1)
        {
            List<Character> charsInRange = new List<Character>();
            CharacterManager.Instance.FindCharactersInRange(_position, 2.5f, ref charsInRange);

            foreach (Character charInRange in charsInRange)
            {
                if (charInRange.GetComponentInChildren<Ephemeral>())
                {
                    // Debug.Log("Found ephemeral");
                    return true;
                }

                // better implementation - check for sigil item ID. not using for now.

                //if (charInRange.GetComponentInChildren<Item>() is Item item && item.ItemID == itemID)
                //{
                //    // Debug.Log("found plague aura in range of position, returning true");
                //    return true;
                //}
            }

            return false;
        }
    }
}
