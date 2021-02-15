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

        public bool CharacterExists { get => m_character; }
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
            else
                Template.Unregister();

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

            Template.ApplyTemplate();
            m_character = Template.Spawn(pos, UID.Generate());
        }

        [Obsolete]
        public void Reset(Vector3 pos, bool newspawn)
        {
            SpawnOrReset();
        }
    }
}
