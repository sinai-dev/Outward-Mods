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
        public const string DUMMY_NAME = "com.sinai.Combat_Dummy";

        public string Name { get; set; }
        public SL_Character Template { get; set; }

        public bool CharacterExists { get => m_character != null; }
        private Character m_character;

        public bool AddCombatAI;
        public bool CanDodge;
        public bool CanBlock;

        public void DestroyCharacter()
        {
            CustomCharacters.DestroyCharacterRPC(m_character);
        }

        public void SpawnOrReset()
        {
            if (Template == null)
            {
                Debug.LogError("null template!");
                return;
            }

            if (AddCombatAI)
            {
                Template.AI = new SL_CharacterAIMelee();
                Template.AI.CanBlock = this.CanBlock;
                Template.AI.CanDodge = this.CanDodge;
            }
            else
                Template.AI = null;

            var pos = CharacterManager.Instance.GetFirstLocalCharacter().transform.position;
            pos += new Vector3(1f, 0f, 1f);

            if (m_character)
            {
                CustomCharacters.DestroyCharacterRPC(m_character);
                Template.Unregister();
            }

            SL.Log("Spawning clone, AI: " + (Template.AI?.ToString() ?? "null"));

            Template.Prepare();
            m_character = Template.Spawn(pos, UID.Generate());
        }

        [Obsolete]
        public void Reset(Vector3 pos, bool newspawn)
        {
            SpawnOrReset();
        }

        //private IEnumerator ResetCoroutine(Vector3 pos, bool newSpawn)
        //{
        //    yield return new WaitForSeconds(0.5f);

        //    // set and apply stats
        //    Template.ApplyToCharacter(m_character);

        //    if (newSpawn)
        //    {
        //        SetAIEnabled(false);
        //    }

        //    // heal or resurrect
        //    HealCharacter();

        //    // try repair
        //    try { m_character.Inventory.RepairEverything(); } catch { }

        //    // teleport
        //    try { m_character.Teleport(pos, Quaternion.identity); } catch { }
        //}

        //public void SetAIEnabled(bool enabled)
        //{
        //    if (m_character == null)
        //    {
        //        return;
        //    }

        //    var ai = m_character.GetComponent<CharacterAI>();
        //    ai.enabled = enabled;

        //    foreach (var state in ai.AiStates)
        //    {
        //        state.enabled = enabled;

        //        if (state is AISCombat aiscombat)
        //        {
        //            aiscombat.CanDodge = enabled ? Template.CanDodge : false;
        //        }
        //    }
        //}

        //public void HealCharacter()
        //{
        //    At.Invoke(m_character.Stats, "RefreshVitalMaxStat", new object[] { false });

        //    if (m_character.IsDead)
        //    {
        //        m_character.Resurrect();
        //    }

        //    if (m_character.StatusEffectMngr != null)
        //    {
        //        m_character.StatusEffectMngr.Purge();
        //    }

        //    m_character.Stats.RestoreAllVitals();
        //}
    }
}
