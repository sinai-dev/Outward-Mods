using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using SideLoader;

namespace Dismantler
{
    public class InteractionDestroy : InteractionDisassemble
    {
        protected override string DefaultHoldText => "Destroy";

        protected override void OnActivate()
        {
            //base.OnActivate();

            var script = At.GetField(this as InteractionDisassemble, "m_deployableScript") as Deployable;

            if (script)
            {
                Debug.Log("Destroying " + script.Item.Name);

                if (this.LastCharacter is Character character)
                {
                    character.SpellCastAnim(Character.SpellCastType.PackupGround, Character.SpellCastModifier.Immobilized, 1);
                }

                StartCoroutine(DelayedDestroy(script));
            }
        }

        private IEnumerator DelayedDestroy(Deployable _deployable)
        {
            yield return new WaitForSeconds(1.0f);

            ItemManager.Instance.DestroyItem(_deployable.Item.UID);
        }
    }
}
