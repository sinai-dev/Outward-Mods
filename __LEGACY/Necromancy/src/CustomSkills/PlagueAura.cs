using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace NecromancerSkills
{
    public class PlagueAura : Summon
    {
        //public static float LifeSpan = 40f;

        #region Plague Aura Skill Setup

        // this also sets up the PlagueTendrils effect.

        public static void SetupPlagueAura()
        {
            // 8890107
            var plagueSkill = ResourcesPrefabManager.Instance.GetItemPrefab(8890107) as Skill;

            // destroy wind altar condition
            Destroy(plagueSkill.transform.Find("AdditionalActivationConditions").gameObject);

            // setup skill
            plagueSkill.CastSheathRequired = -1;
            plagueSkill.RequiredItems = new Skill.ItemRequired[0];

            // get the Summon component, change to custom activation fx
            var effects = plagueSkill.transform.Find("Effects").gameObject;
            var origSummon = effects.GetComponent<Summon>();
            var plagueAuraComp = effects.AddComponent<PlagueAura>();
            At.CopyFields(plagueAuraComp, origSummon, null, true);
            Destroy(origSummon);

            // ======== set summoned prefab to our custom activated item (loaded with sideloader) ========
            var plagueStone = ResourcesPrefabManager.Instance.GetItemPrefab(8999050);
            plagueAuraComp.SummonedPrefab = plagueStone.transform;            

            var ephemeral = plagueStone.GetComponent<Ephemeral>();
            ephemeral.LifeSpan = NecromancerBase.settings.PlagueAura_SigilLifespan;

            //// setup custom visuals
            //var origVisuals = plagueStone.GetComponent<Item>().VisualPrefab;
            //origVisuals.gameObject.SetActive(false);
            //var newVisuals = Instantiate(origVisuals.gameObject);
            //DontDestroyOnLoad(newVisuals);
            //origVisuals.gameObject.SetActive(true);
            var newVisuals = CustomItemVisuals.GetOrAddVisualLink(plagueStone).ItemVisuals;

            var magiccircle = newVisuals.transform.Find("mdl_fx_magicCircle");
            // destroy rotating bolt fx
            Destroy(magiccircle.transform.Find("FX_Bolt").gameObject);

            // setup the clouds
            if (newVisuals.transform.Find("mdl_itm_firestone") is Transform t)
            {
                t.parent = magiccircle;
                Destroy(t.Find("FX_Bolt").gameObject);

                var ps = t.Find("smoke_desu").GetComponent<ParticleSystem>();
                var m = ps.main;
                m.startColor = Color.green;

                t.Find("smoke_desu").position += Vector3.down * 3.2f;
            }

            // setup the Plague Tendrils effect (from inside that class)
            PlagueAuraTendrils.SetupPlagueTendrils(effects);
        }

        #endregion

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
