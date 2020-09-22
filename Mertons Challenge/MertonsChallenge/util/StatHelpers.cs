using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MertonsChallenge
{
    public class StatHelpers : MonoBehaviour
    {
        public ChallengeGlobal global;

        // -------------------------- ITEM STAT HELPER FUNCTIONS ------------------------

        // This helper function is to set the appropriate scaled damage depending on the attack id (which part of combo).
        // Outward has no function to handle this in their api, according to Keos these are managed in a separate tool.
        // So I just copied the modifiers that they use for each class.

        public void SetScaledDamages(Weapon.WeaponType type, int attackID, ref List<float> stepDamage, ref float stepImpact)
        {
            float dmgMulti = 1.0f;
            float impactMulti = 1.0f;

            switch (type)
            {
                // cases 0 and 1 are ignored because light attacks always use 1.0f modifiers. 2 is heavy attack, 3 and 4 are the combo attacks.
                case Weapon.WeaponType.Sword_1H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.1f;
                            impactMulti = 1.1f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Axe_1H:
                    switch (attackID)
                    {
                        case 2:
                        case 3:
                        case 4:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Mace_1H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.3f;
                            impactMulti = 2.5f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Sword_2H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.5f;
                            impactMulti = 1.5f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.1f;
                            impactMulti = 1.1f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Axe_2H:
                    switch (attackID)
                    {
                        case 2:
                        case 3:
                        case 4:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Mace_2H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 0.75f;
                            impactMulti = 2.0f;
                            break;
                        case 3:
                        case 4:
                            dmgMulti = 1.4f;
                            impactMulti = 1.4f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Spear_2H:
                    switch (attackID)
                    {
                        case 2:
                            dmgMulti = 1.4f;
                            impactMulti = 1.2f;
                            break;
                        case 3:
                            dmgMulti = 1.3f;
                            impactMulti = 1.2f;
                            break;
                        case 4:
                            dmgMulti = 1.2f;
                            impactMulti = 1.1f;
                            break;
                    }
                    break;
                case Weapon.WeaponType.Halberd_2H:
                    switch (attackID)
                    {
                        case 2:
                        case 3:
                            dmgMulti = 1.3f;
                            impactMulti = 1.3f;
                            break;
                        case 4:
                            dmgMulti = 1.7f;
                            impactMulti = 1.7f;
                            break;
                    }
                    break;
                default:
                    break;
            }

            for (int i = 0; i < stepDamage.Count(); i++)
            {
                stepDamage[i] *= dmgMulti;
            }

            stepImpact *= impactMulti;

            return;
        }

        // damage names pretty print / helper
        public string[] DamageNames = new string[10]
        {
            "Physical",
            "Ethereal",
            "Decay",
            "Lightning",
            "Frost",
            "Fire",
            "DarkOLD",
            "LightOLD",
            "Raw",
            "None"
        };

        // weapon type pretty print
        public Dictionary<string, Weapon.WeaponType> weaponTypes = new Dictionary<string, Weapon.WeaponType>() {
            { "Sword 1H", Weapon.WeaponType.Sword_1H },
            { "Axe 1H", Weapon.WeaponType.Axe_1H },
            { "Mace 1H", Weapon.WeaponType.Mace_1H },
            { "Sword 2H", Weapon.WeaponType.Sword_2H },
            { "Axe 2H", Weapon.WeaponType.Axe_2H },
            { "Mace 2H", Weapon.WeaponType.Mace_2H },
            { "Polearm", Weapon.WeaponType.Halberd_2H },
            { "Spear", Weapon.WeaponType.Spear_2H },
            { "Shield", Weapon.WeaponType.Shield },
            { "Bow", Weapon.WeaponType.Bow },
            { "Pistol", Weapon.WeaponType.Pistol_OH },
            { "Chakram", Weapon.WeaponType.Chakram_OH },
            { "Dagger", Weapon.WeaponType.Dagger_OH }
        };

        // relevant status effects dictionary
        public List<string> StatusNames = new List<string>()
        {
            "Bleeding",
            "Bleeding +",
            "Burning",
            "Poisoned",
            "Poisoned +",
            "Burn",
            "Chill",
            "Curse",
            "Elemental Vulnerability",
            "Haunted",
            "Doom",
            "Pain",
            "Confusion",
            "Dizzy",
            "Cripped",
            "Slow Down",
        };
    }
}
