using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectionBubble
{
    public class ProtectionBubble : Effect
    {
        public const string IDENTIFIER = "Protection Bubble";

        //private const float REGEN_DELAY = 5f;
        private const float UPDATE_RATE = 0.1f;
        private float m_timeOfLastUpdate = 0f;

        public float m_maxProtection;        
        public float m_activeProtection;

        public StatusEffect LinkedStatus
        {
            get
            {
                if (!m_linkedStatus)
                {
                    m_linkedStatus = GetComponentInParent<StatusEffect>();
                }
                return m_linkedStatus;
            }
        }
        private StatusEffect m_linkedStatus;

        public VFXSystem LinkedVFX
        {
            get
            {
                if (m_linkedVFX == null)
                {
                    m_linkedVFX = this.LinkedStatus?.FxTransform?.GetComponent<VFXSystem>();
                }
                return m_linkedVFX;
            }
        }
        private VFXSystem m_linkedVFX;

        public Image LinkedIcon
        {
            get
            {
                if (!m_linkedIcon)
                {
                    if (LinkedStatus && this.OwnerCharacter && this.OwnerCharacter.CharacterUI)
                    {
                        var panel = this.OwnerCharacter.CharacterUI.GetComponentInChildren<StatusEffectPanel>(true);
                        if (panel)
                        {
                            var dict = (Dictionary<string, StatusEffectIcon>)At.GetValue(typeof(StatusEffectPanel), panel, "m_statusIcons");
                            if (dict != null && dict.ContainsKey(IDENTIFIER))
                            {
                                m_linkedIcon = (Image)At.GetValue(typeof(StatusEffectIcon), dict[IDENTIFIER], "m_icon");
                            }
                        }
                    }
                }

                return m_linkedIcon;
            }
        }
        private Image m_linkedIcon;

        // ======================= setup ===========================

        public static void Setup()
        {
            var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(IDENTIFIER);

            // status effect data

            status.IsHidden = true;
            status.DisplayInHud = false;

            var sig = status.transform.GetChild(0);
            var effects = new GameObject("Activation");
            effects.transform.parent = sig;
            effects.AddComponent<ProtectionBubble>();

            SL_StatusEffect.CompileEffectsToData(status);

            // visual fx

            GameObject fx = Instantiate((ResourcesPrefabManager.Instance.GetItemPrefab(8100140) as Skill)
                                        .StartVFX
                                        .gameObject);
            DontDestroyOnLoad(fx);
            fx.SetActive(false);
            fx.transform.ResetLocal();
            fx.GetComponent<VFXSystem>().Stop();

            DestroyImmediate(fx.transform.Find("SphereSmoke").gameObject);

            fx.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.loop = true;
                main.simulationSpeed = 0.3f;

                var color = main.startColor;
                color.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);

                main.duration = 5f;
            }

            status.FXPrefab = fx.transform;
            status.FxInstantiation = StatusEffect.FXInstantiationTypes.Normal;
        }

        // ======================= methods ===========================

        public void MitigateDamage(ref DamageList damages, bool applyToActiveCount = true)
        {
            float current = m_activeProtection;

            foreach (var dmg in damages.List)
            {
                MitigateDamage(ref dmg.Damage);
            }

            if (!applyToActiveCount)
            {
                m_activeProtection = current;
            }
        }

        public void MitigateDamage(ref float damage)
        {
            //Debug.Log("Receiving " + damage + " damage, Active protection: " + m_activeProtection);

            if (m_activeProtection > 0f)
            {
                if (damage >= m_activeProtection)
                {
                    // damage is going to drain all protection.
                    damage -= m_activeProtection;
                    
                    m_activeProtection = 0f;
                }
                else
                {
                    // damage will not drain all protection.
                    m_activeProtection -= damage;

                    damage = 0f;
                }
            }

            //Debug.Log("Remaining prot: " + m_activeProtection + ", received damage: " + damage);
        }

        // ======================= overrides ===========================

        internal void Update()
        {
            if (Time.time - m_timeOfLastUpdate > UPDATE_RATE)
            {
                m_timeOfLastUpdate = Time.time;

                OnRefreshPotency();
            }
        }

        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            OnRefreshPotency();

            m_activeProtection = m_maxProtection;
        }

        protected override void OnRefreshPotency()
        {
            base.OnRefreshPotency();

            if (!this.OwnerCharacter)
            {
                return;
            }

            // update max protection
            var prot = this.OwnerCharacter.Stats.GetDamageProtection(DamageType.Types.Physical);
            var ratio = (float)ProtectionBubbleMod.config.GetValue(Settings.ShieldRatio) / 100f;
            m_maxProtection = prot * ratio;

            // active shield value regen
            if (m_activeProtection < m_maxProtection && Time.time - this.OwnerCharacter.LastHurtTime > (float)ProtectionBubbleMod.config.GetValue(Settings.RegenDelay))
            {
                var regen = (float)ProtectionBubbleMod.config.GetValue(Settings.RegenRate);

                m_activeProtection += regen;
            }

            // linked status icon / description / etc
            if (this.LinkedStatus)
            {
                var remaining = Mathf.Clamp(Mathf.Round(m_activeProtection), 0f, m_maxProtection);

                if (remaining > 0f)
                {
                    OnShow();

                    // update desc
                    At.SetValue($"Your Protection is shielding you from the next {remaining:#.0} damage.",
                        typeof(StatusEffect),
                        LinkedStatus,
                        "m_description"
                    );

                    // set icon alpha
                    if (this.LinkedIcon)
                    {
                        var color = this.LinkedIcon.color;
                        var alpha = (float)((decimal)m_activeProtection / (decimal)m_maxProtection);

                        this.LinkedIcon.color = new Color(color.r, color.g, color.b, alpha);
                    }
                }
                else 
                {
                    OnHide();
                }
            }
        }

        private void OnShow()
        {
            if (LinkedStatus.IsHidden)
            {
                LinkedStatus.IsHidden = false;
                LinkedStatus.DisplayInHud = true;

                if ((bool)ProtectionBubbleMod.config.GetValue(Settings.ShowFX) && LinkedVFX)
                {
                    LinkedVFX.gameObject.SetActive(true);
                    LinkedVFX.transform.localPosition = Vector3.down;

                    LinkedVFX.Play(this.OwnerCharacter, null);
                }
            }
        }

        private void OnHide()
        {
            // OnHide
            if (!LinkedStatus.IsHidden)
            {
                LinkedStatus.IsHidden = true;
                LinkedStatus.DisplayInHud = false;
                LinkedStatus.FxTransform?.gameObject.SetActive(false);

                if (LinkedVFX)
                {
                    LinkedVFX.Stop();
                }
            }
        }
    }
}
