using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SideLoader;
using UnityEngine;
using UnityEngine.AI;

namespace Necromancer
{
    public static class SummonManager
    {
        public static Dictionary<string, List<string>> SummonedCharacters = new Dictionary<string, List<string>>(); // Key: Caster UID, Value: List of Summon UIDs

        public static SL_Character Skeleton;
        public static SL_Character Ghost;

        internal static void Init()
        {
            var pack = SL.GetSLPack("sinai-dev Necromancer");

            Skeleton = pack.CharacterTemplates["com.sinai.necromancer.skeleton"];
            Skeleton.OnSpawn += OnSummonSpawn;

            Ghost = pack.CharacterTemplates["com.sinai.necromancer.ghost"];
            Ghost.OnSpawn += OnSummonSpawn;
        }

        private static void OnSummonSpawn(Character character, string rpcData)
        {
            SL.Log("Necromancy SummonManager.OnSpawn, character: " + character?.name + ", rpcData: " + rpcData);

            try
            {
                var ownerUID = rpcData;
                var summonUID = character.UID;

                // add to dictionary
                if (SummonedCharacters.ContainsKey(ownerUID))
                    SummonedCharacters[ownerUID].Add(summonUID);
                else
                    SummonedCharacters.Add(ownerUID, new List<string> { summonUID });

                character.OnDeath += () => { OnSummonDeath(character); };

                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    var owner = CharacterManager.Instance.GetCharacter(ownerUID);

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

        public static void OnSummonDeath(Character summon)
        {
            //if (PhotonNetwork.isNonMasterClientInRoom)
            //    return;

            foreach (var spawnList in SummonedCharacters)
            {
                if (spawnList.Value.Contains(summon.UID))
                {
                    spawnList.Value.Remove(summon.UID);
                    break;
                }
            }
        }

        // find the weakest current summon for a character. can be used arbitrarily by anything.
        public static Character FindWeakestSummon(string ownerUID)
        {
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
