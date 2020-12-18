using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SideLoader;

namespace Combat_Dummy
{
    public class DummyCharacter
    {
        public const string DUMMY_NAME = "Combat Dummy";

        public string Name { get; set; }
        public DummyConfig Config { get; set; }

        public bool CharacterExists { get => m_character != null; }
        private Character m_character;

        public void DestroyCharacter()
        {
            CustomCharacters.DestroyCharacterRPC(m_character);
        }

        public void SpawnOrReset()
        {
            if (Config == null)
            {
                Debug.LogError("null config!");
                return;
            }

            var pos = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;
            pos += new Vector3(1f, 0f, 1f);

            bool newspawn = false;
            if (m_character == null)
            {
                // create character with SideLoader
                var newCharacter = CustomCharacters.SpawnCharacter(pos, UID.Generate(), Name ?? DUMMY_NAME, true, null);
                m_character = newCharacter.GetComponent<Character>();

                newspawn = true;
            }

            m_character.gameObject.SetActive(true);

            Reset(pos, newspawn);
        }

        public void Reset(Vector3 pos, bool newspawn)
        {
            CombatDummyMod.Instance.StartCoroutine(ResetCoroutine(pos, newspawn));
        }

        private IEnumerator ResetCoroutine(Vector3 pos, bool newSpawn)
        {
            yield return new WaitForSeconds(0.5f);

            // set and apply stats
            Config.ApplyToCharacter(m_character);

            if (newSpawn)
            {
                SetAIEnabled(false);
            }

            // heal or resurrect
            HealCharacter();

            // try repair
            try { m_character.Inventory.RepairEverything(); } catch { }

            // teleport
            try { m_character.Teleport(pos, Quaternion.identity); } catch { }
        }

        public void SetAIEnabled(bool enabled)
        {
            if (m_character == null)
            {
                return;
            }

            var ai = m_character.GetComponent<CharacterAI>();
            ai.enabled = enabled;

            foreach (var state in ai.AiStates)
            {
                state.enabled = enabled;

                if (state is AISCombat aiscombat)
                {
                    aiscombat.CanDodge = enabled ? Config.CanDodge : false;
                }
            }
        }

        public void HealCharacter()
        {
            At.Invoke(m_character.Stats, "RefreshVitalMaxStat", new object[] { false });

            if (m_character.IsDead)
            {
                m_character.Resurrect();
            }

            if (m_character.StatusEffectMngr != null)
            {
                m_character.StatusEffectMngr.Purge();
            }

            m_character.Stats.RestoreAllVitals();
        }
    }

    public class DummyConfig
    {
        public const string StatSourceID = "DummyStat";

        // faction
        public Character.Factions Faction = Character.Factions.Bandits;

        // gear
        public int Weapon = 2000010;
        public int Shield = 2300000;

        // ai
        public bool CanDodge = false;
        public bool CanBlock = false;

        // stats
        public float Health = 500;
        public float ImpactResist = 0;
        public float Protection = 0;
        public float[] Damage_Resists = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
        public float[] Damage_Bonus = new float[6] { 0f, 0f, 0f, 0f, 0f, 0f };
    
        public void ApplyToCharacter(Character character)
        {
            // set faction
            character.ChangeFaction(Faction);

            // gear
            TryEquip(character, Weapon);
            TryEquip(character, Shield);

            // AI
            var ai = character.GetComponent<CharacterAI>();
            foreach (var state in ai.AiStates)
            {
                if (state is AISCombat aisCombat)
                {
                    aisCombat.CanDodge = CanDodge;
                    aisCombat.CanBlock = CanBlock;
                }
            }

            // stats
            var stats = character.GetComponent<CharacterStats>();

            var m_maxHealthStat = (Stat)At.GetField(stats, "m_maxHealthStat");
            m_maxHealthStat.AddStack(new StatStack(StatSourceID, Health - 100), false);

            var m_impactResistance = (Stat)At.GetField(stats, "m_impactResistance");
            m_impactResistance.AddStack(new StatStack(StatSourceID, ImpactResist), false);

            var m_damageProtection = (Stat[])At.GetField(stats, "m_damageProtection");
            m_damageProtection[0].AddStack(new StatStack(StatSourceID, Protection), false);

            var m_damageResistance = (Stat[])At.GetField(stats, "m_damageResistance");
            for (int i = 0; i < 6; i++)
            {            
                m_damageResistance[i].AddStack(new StatStack(StatSourceID, Damage_Resists[i]), false);
            }

            var m_damageTypesModifier = (Stat[])At.GetField(stats, "m_damageTypesModifier");
            for (int i = 0; i < 6; i++)
            {
                m_damageTypesModifier[i].AddStack(new StatStack(StatSourceID, Damage_Bonus[i]), false);
            }
        }

        private void TryEquip(Character character, int id)
        {
            if (ResourcesPrefabManager.Instance.GetItemPrefab(id) is Equipment item)
            {
                bool makeNew = true;

                if (character.Inventory.Equipment.GetEquippedItem(item.EquipSlot) is Equipment existing)
                {
                    if (existing.ItemID == id)
                    {
                        makeNew = false;
                    }
                    else
                    {
                        GameObject.DestroyImmediate(existing.gameObject);
                    }
                }

                if (makeNew)
                {
                    var newItem = ItemManager.Instance.GenerateItemNetwork(id);
                    newItem.ChangeParent(character.Inventory.Equipment.GetMatchingEquipmentSlotTransform(item.EquipSlot));
                }
            }
        }
    }
}
