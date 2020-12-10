using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SideLoader;
using UnityEngine;
using UnityEngine.AI;
//using SinAPI;

namespace NecromancerSkills
{
    public class SummonManager : MonoBehaviour
    {
        //public ModBase global;
        public static SummonManager Instance;

        public Dictionary<string, List<string>> SummonedCharacters = new Dictionary<string, List<string>>(); // Key: Caster UID, Value: List of Summon UIDs

        private float lastUpdateTime = -1f;

        public static readonly SL_Character Skeleton = new SL_Character
        {
            UID = "com.sinai.Necromancer.skeleton",
            SaveType = CharSaveType.Follower,
            Name = "Skeleton",
            AddCombatAI = true,
            CanBlock = true,
            CanDodge = false,
            Faction = Character.Factions.Player,
            Health = NecromancerBase.settings.Summon_MaxHealth,
            HealthRegen = NecromancerBase.settings.Summon_HealthLoss,
            Status_Immunity = new List<string>()
            {
                "Bleeding",
                "Poison"
            },
            Weapon_ID = 2598500,
            Chest_ID = 3200030,
            Helmet_ID = 3200031,
            Boots_ID = 3200032,
        };

        public static readonly SL_Character Ghost = new SL_Character
        {
            UID = "com.sinai.Necromancer.ghost",
            SaveType = CharSaveType.Follower,
            Name = "Ghost",
            AddCombatAI = true,
            CanBlock = true,
            CanDodge = true,
            Faction = Character.Factions.Player,
            Health = NecromancerBase.settings.StrongSummon_MaxHealth,
            HealthRegen = NecromancerBase.settings.StrongSummon_HealthLoss,
            Status_Immunity = new List<string>()
            {
                "Bleeding",
                "Poison"
            },
            Weapon_ID = 2598500,
            Chest_ID = 3200040,
            Helmet_ID = 3200041,
            Boots_ID = 3200042,
            Backpack_ID = 5400010,
        };

        // Only host calls this directly. This is the Main function for creating a summon. In our case, a skeleton minion.
        // See the "SummonSkeleton" class for an example of how this works.
        public GameObject SummonSpawn(Character caster, string summonUID, bool insidePlagueAura)
        {
            Vector3 spawnPos = caster.transform.position + (Vector3.forward * 0.5f);

            var template = insidePlagueAura ? Ghost : Skeleton;

            var character = CustomCharacters.SpawnCharacter(template, spawnPos, summonUID, caster.UID.ToString())
                                            .GetComponent<Character>();

            // unsheathe
            character.SheatheInput();

            return character.gameObject;
        }

        private void OnSpawn(Character character, string rpcData)
        {
            //SL.Log("Necromancy SummonManager.OnSpawn, character: " + character?.name + ", rpcData: " + rpcData);

            try
            {
                var ownerUID = rpcData;
                var summonUID = character.UID;

                // add to dictionary
                if (SummonedCharacters.ContainsKey(ownerUID))
                    SummonedCharacters[ownerUID].Add(summonUID);
                else
                    SummonedCharacters.Add(ownerUID, new List<string> { summonUID });

                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    var owner = CharacterManager.Instance.GetCharacter(ownerUID);

                    // get the Wander state, and set the FollowTransfer to our caster character
                    var wander = character.GetComponentInChildren<AISWander>(true);
                    wander.FollowTransform = owner.transform;

                    // add auto-teleport component
                    var tele = character.gameObject.AddComponent<SummonTeleport>();
                    tele.m_character = character;
                    tele.TargetCharacter = owner.transform;
                }
            }
            catch (Exception e)
            {
                SL.LogInnerException(e);
            }
        }

        // find the weakest current summon for a character. can be used arbitrarily by anything.
        public Character FindWeakestSummon(string ownerUID)
        {
            UpdateSummonedCharacters(); // force update of characters to remove dead ones etc

            Character character = null;

            if (SummonedCharacters.ContainsKey(ownerUID) && SummonedCharacters[ownerUID].Count() > 0)
            {
                float lowest = float.MaxValue;
                foreach (string uid in SummonedCharacters[ownerUID])
                {
                    if (CharacterManager.Instance.GetCharacter(uid) is Character c && c.Stats.CurrentHealth < lowest)
                    {
                        lowest = c.Stats.CurrentHealth;
                        character = c;
                    }
                }
            }

            return character;
        }

        // ========= internal ==========

        internal void Awake()
        {
            Instance = this;

            Ghost.Prepare();
            Skeleton.Prepare();

            Ghost.OnSpawn += OnSpawn;
            Skeleton.OnSpawn += OnSpawn;
        }

        // the tick update is limited to 0.5 secs, since its just for cleaning up dead summons and low priority stuff.
        internal void Update()
        {
            if (SummonedCharacters.Count > 0 && Time.time - lastUpdateTime > 0.5f)
            {
                lastUpdateTime = Time.time;
                UpdateSummonedCharacters();
            }
        }

        private void UpdateSummonedCharacters()
        {
            foreach (KeyValuePair<string, List<string>> entry in SummonedCharacters)
            {
                List<string> toRemove = new List<string>();

                foreach (string uid in entry.Value)
                {
                    if (CharacterManager.Instance.GetCharacter(uid) is Character c)
                    {
                        // clear dead resurrects
                        if (c.IsDead)
                        {
                            //OLogger.Warning(c.Name + " is dead! Removing from list and destroying object.");
                            DestroySummon(c);
                            toRemove.Add(uid);
                        }
                    }
                    else
                    {
                        //OLogger.Warning("CharacterManager GetCharacter " + uid + " is null! Removing from list.");
                        toRemove.Add(uid);
                    }
                }

                if (toRemove.Count > 0)
                {
                    foreach (string uid in toRemove)
                    {
                        entry.Value.Remove(uid);
                    }
                }
            }
        }

        public static void DestroySummon(Character summon)
        {
            CustomCharacters.DestroyCharacterRPC(summon);
        }
    }
}
