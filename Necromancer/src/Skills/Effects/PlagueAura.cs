using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace Necromancer.Skills.Effects
{
    public class PlagueAura : Summon
    {
        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            if (PhotonNetwork.isNonMasterClientInRoom) { return; }

            m_nextObjectID = (int)_infos[1];

            if (m_lastSummonedObject[m_nextObjectID] != null)
            {
                Destroy(m_lastSummonedObject[m_nextObjectID].gameObject);
            }
            m_lastSummonedObject[m_nextObjectID] = Instantiate(SummonedPrefab).transform;

            if (m_lastSummonedObject[m_nextObjectID] is Transform summonedVisuals)
            {
                summonedVisuals.parent = _affectedCharacter.transform;
                summonedVisuals.position = _affectedCharacter.transform.position;

                summonedVisuals.gameObject.SetActive(true);

                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    StartCoroutine(FixVisualsCoroutine(summonedVisuals, _affectedCharacter.Visuals.transform));
                }
            }

            this.m_nextObjectID++;
            if (this.m_nextObjectID >= this.m_lastSummonedObject.Length)
            {
                this.m_nextObjectID = 0;
            }
        }

        private IEnumerator FixVisualsCoroutine(Transform summonedVisuals, Transform visualsTransform)
        {
            yield return new WaitForSeconds(1.5f);

            if (summonedVisuals != null)
            {
                summonedVisuals.parent = visualsTransform;
                summonedVisuals.position = visualsTransform.position;
            }

            yield return null;
        }
    }
}
