using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using SideLoader;
using HarmonyLib;

namespace NecromancerSkills
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance;

        public static SkillSchool NecromancerTree;

        internal void Awake()
        {
            Instance = this;

            SL.OnPacksLoaded += Setup;
        }

        private void Setup()
        {
            Debug.Log("Setting up Necromancer skills");

            try
            {
                SetupSkills(); // setup the actual skills

                SetupSkillTree(); // setup the trainer menu, and link these skills to the SkillTreeHolder (the game's skill manager)
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception setting up Necromancer skills!");
                Debug.LogWarning(e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        // patch for skill activation (pretty lazy method, but works)
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
                        if (!SummonManager.Instance.FindWeakestSummon(self.OwnerCharacter.UID))
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

        private void SetupSkills()
        {
            // I use the SideLoader to do the preliminary setup (clone existing skill, change ID / name / etc..)

            SetupTranscendence();

            SummonSkeleton.SetupSummon();

            NoxiousTendrils.SetupTendrils();

            LifeRitual.SetupFrenzy();

            DeathRitual.SetupDetonate();

            PlagueAura.SetupPlagueAura();
        }

        private void SetupTranscendence()
        {
            // =============== transendence ===============

            var transcendence = ResourcesPrefabManager.Instance.GetItemPrefab(8890104) as PassiveSkill;
            var passiveTransform = transcendence.transform.Find("Passive");
            DestroyImmediate(passiveTransform.GetComponent<AffectStat>());

            // add elemental bonuses using custom ManaPointAffectStat class
            // elemental bonus stats are id 98 to 102

            for (int i = 98; i < 103; i++)
            {
                var comp = passiveTransform.gameObject.AddComponent<ManaPointAffectStat>();
                comp.SelectedUID = i.ToString();
            }
        }


        // Skill Tree setup, and add to SkillTreeHolder (game manager)

        private void SetupSkillTree()
        {
            var tree = new SL_SkillTree()
            {
                Name = "Necromancer",
                SkillRows = new List<SL_SkillRow>()
                {
                    new SL_SkillRow()
                    {
                        RowIndex = 1,
                        Slots = new List<SL_BaseSkillSlot>()
                        {
                            new SL_SkillSlot() // Summon
                            {
                                ColumnIndex = 2,
                                SilverCost = 50,
                                SkillID = 8890103,
                                Breakthrough = false,
                                RequiredSkillSlot = Vector2.zero,
                            },
                            new SL_SkillSlot() // Vital Attunement
                            {
                                ColumnIndex = 3,
                                SilverCost = 50,
                                SkillID = 8890101,
                                Breakthrough = false,
                                RequiredSkillSlot = Vector2.zero,
                            },
                        }
                    },
                    new SL_SkillRow()
                    {
                        RowIndex = 2,
                        Slots = new List<SL_BaseSkillSlot>()
                        {
                            new SL_SkillSlot() // Life Ritual
                            {
                                ColumnIndex = 2,
                                SkillID = 8890105,
                                SilverCost = 100,
                                Breakthrough = false,
                                RequiredSkillSlot = new Vector2(1, 2), // requires Summon (row 1, slot 2)
                            },
                            new SL_SkillSlot() // Tendrils
                            {
                                ColumnIndex = 3,
                                SkillID = 8890100,
                                SilverCost = 100,
                                Breakthrough = false,
                                RequiredSkillSlot = new Vector2(1, 3), // requires Vital Attunement (row 1, slot 3)
                            }
                        }
                    },
                    new SL_SkillRow()
                    {
                        RowIndex = 3,
                        Slots = new List<SL_BaseSkillSlot>() // Transcendence (Breakthrough)
                        {
                            new SL_SkillSlot()
                            {
                                Breakthrough = true,
                                SkillID = 8890104,
                                RequiredSkillSlot = new Vector2(2, 2), // requires Life Ritual (row 2, slot 2),
                                SilverCost = 500,
                                ColumnIndex = 2
                            }
                        }
                    },
                    new SL_SkillRow()
                    {
                        RowIndex = 4,
                        Slots = new List<SL_BaseSkillSlot>() // Death Ritual
                        {
                            new SL_SkillSlot()
                            {
                                ColumnIndex = 2,
                                RequiredSkillSlot = new Vector2(3, 2), // requires breakthrough
                                SkillID = 8890106,
                                SilverCost = 600,
                                Breakthrough = false
                            }
                        }
                    },
                    new SL_SkillRow()
                    {
                        RowIndex = 5,
                        Slots = new List<SL_BaseSkillSlot>() // fork choice
                        {
                            new SL_SkillSlotFork()
                            {
                                ColumnIndex = 2,
                                RequiredSkillSlot = new Vector2(4, 2), // requires Death Ritual
                                Choice1 = new SL_SkillSlot() // Plague Aura
                                {
                                    ColumnIndex = 2,
                                    Breakthrough = false,
                                    SilverCost = 600,
                                    SkillID = 8890107,
                                    RequiredSkillSlot = new Vector2(4, 2), // requires Death Ritual
                                },
                                Choice2 = new SL_SkillSlot() // Army of Death
                                {
                                    ColumnIndex = 2,
                                    Breakthrough = false,
                                    SilverCost = 600,
                                    SkillID = 8890108,
                                    RequiredSkillSlot = new Vector2(4, 2), // requires Death Ritual
                                }
                            }
                        }
                    }
                }
            };

            NecromancerTree = tree.CreateBaseSchool(true);

            //SL.Log("Created Necromancer tree, UID: " + NecromancerTree.UID);

            //Debug.Log("Set up Necromancer tree. Components: " + NecromancerTree.gameObject.GetComponentsInChildren<BaseSkillSlot>().Length);
        }
    }
}
