using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SideLoader;
using UnityEngine;
using UnityEngine.AI;

namespace NecromancerSkills
{
    public class SummonManager : MonoBehaviour
    {
        //public ModBase global;
        public static SummonManager Instance;

        public Dictionary<string, List<string>> SummonedCharacters = new Dictionary<string, List<string>>(); // Key: Caster UID, Value: List of Summon UIDs

        //private float lastUpdateTime = -1f;

        public static readonly SL_Character Skeleton = new SL_Character
        {
            UID = "com.sinai.Necromancer.skeleton",
            SaveType = CharSaveType.Follower,
            Name = "Skeleton",
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
            AI = new SL_CharacterAIMelee(),
        };

        public static readonly SL_Character Ghost = new SL_Character
        {
            UID = "com.sinai.Necromancer.ghost",
            SaveType = CharSaveType.Follower,
            Name = "Ghost",
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
            AI = new SL_CharacterAIMelee(),
        };

        internal void Awake()
        {
            Instance = this;

            Ghost.Prepare();
            Skeleton.Prepare();

            Ghost.OnSpawn += OnSpawn;
            Skeleton.OnSpawn += OnSpawn;
        }

        public static void DestroySummon(Character summon)
        {
            CustomCharacters.DestroyCharacterRPC(summon);
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

                character.OnDeath += () => { StartCoroutine(OnSummonDeath(summonUID)); };

                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    var owner = CharacterManager.Instance.GetCharacter(ownerUID);

                    //foreach (var wander in character.GetComponentsInChildren<AISWander>())
                    //{
                    //    wander.FollowTransform = owner.transform;
                    //    wander.FollowOffset = new Vector3(1.5f, 0f, 1.5f);
                    //}

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

        private IEnumerator OnSummonDeath(string UID)
        {
            yield return new WaitForSeconds(1.0f);

            foreach (var spawnList in SummonedCharacters)
            {
                if (spawnList.Value.Contains(UID))
                {
                    spawnList.Value.Remove(UID);
                    break;
                }
            }

            if (!PhotonNetwork.isNonMasterClientInRoom)
                CustomCharacters.DestroyCharacterRPC(UID);
        }

        // find the weakest current summon for a character. can be used arbitrarily by anything.
        public Character FindWeakestSummon(string ownerUID)
        {
            //UpdateSummonedCharacters(); // force update of characters to remove dead ones etc

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
    }
}
