using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SideLoader;

namespace CombatHUD
{
    public class TargetManager : MonoBehaviour
    {
        public int Split_ID;

        private Character m_LinkedCharacter;
        private string m_lastTargetUID;

        // Target HUD stuff
        private GameObject m_TargetHUDHolder; // Each SplitPlayer gets one of these.
        private Text m_targetHealthText; // holder for showing enemy's health text
        private GameObject m_StatusHolder; // holds all the enemy status icon and text holders

        // Infobox Stuff
        private GameObject m_infoboxHolder; // each SplitPlayer gets one of these.
        private Text m_infoboxName; // targeted enemy's name
        private Text m_infoboxHealth; // active health / max health
        private Text m_infoboxImpact; // impact res
        private readonly List<Text> m_infoboxDamageTexts = new List<Text>(); // [0] -> [5], enemy resistances, uses (DamageType.Types)int
        private Text m_infoboxNoImmuneText; // the "none" text
        private Image m_infoboxBurningSprite; // Burning sprite
        private Image m_infoboxBleedingSprite; // Bleeding sprite
        private Image m_infoboxPoisonSprite; // Poison sprite

        private Vector2 m_startPos;
        private Vector2 m_currentOffset = new Vector2(0, 0);

        // setup global vars from the prefab asset
        internal void Awake()
        {
            // Setup TargetHUD
            m_TargetHUDHolder = this.transform.Find("TargetHUD_Holder").gameObject;
            m_targetHealthText = m_TargetHUDHolder.transform.Find("text_Health").GetComponent<Text>();
            m_StatusHolder = m_TargetHUDHolder.transform.Find("StatusEffects_Holder").gameObject;
            for (int i = 0; i < m_StatusHolder.transform.childCount; i++)
            {
                var child = m_StatusHolder.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }

            // setup new status effects
            var baseStatus = m_StatusHolder.transform.GetChild(0);
            var names = new string[] { "Weaken", "Sapped" };
            for (int i = 0; i < 2; i++)
            {
                var newHolder = Instantiate(baseStatus.gameObject);
                DontDestroyOnLoad(newHolder);
                newHolder.transform.SetParent(baseStatus.parent, false);
                newHolder.name = names[i];

                var status = ResourcesPrefabManager.Instance.GetStatusEffectPrefab(names[i]);

                var icon = newHolder.GetComponentInChildren<Image>();
                icon.sprite = status.StatusIcon;
            }

            // Setup Infobox
            m_infoboxHolder = this.transform.Find("InfoboxHolder").gameObject;

            var texts = m_infoboxHolder.transform.Find("Texts");

            m_infoboxName   = texts.Find("text_Name").GetComponent<Text>();
            m_infoboxHealth = texts.Find("text_Health").GetComponent<Text>();
            m_infoboxHealth.fontSize = 14;
            m_infoboxImpact = texts.Find("text_Impact").GetComponent<Text>();

            var damageTexts = texts.Find("DamageTexts");
            m_infoboxDamageTexts.Add(damageTexts.Find("text_Phys").GetComponent<Text>());
            m_infoboxDamageTexts.Add(damageTexts.Find("text_Ethereal").GetComponent<Text>());
            m_infoboxDamageTexts.Add(damageTexts.Find("text_Decay").GetComponent<Text>());
            m_infoboxDamageTexts.Add(damageTexts.Find("text_Lightning").GetComponent<Text>());
            m_infoboxDamageTexts.Add(damageTexts.Find("text_Frost").GetComponent<Text>());
            m_infoboxDamageTexts.Add(damageTexts.Find("text_Fire").GetComponent<Text>());

            var statusIcons  = texts.Find("StatusIcons");
            m_infoboxBleedingSprite = statusIcons.Find("Bleeding").GetComponent<Image>();
            m_infoboxBurningSprite  = statusIcons.Find("Burning").GetComponent<Image>();
            m_infoboxPoisonSprite   = statusIcons.Find("Poison").GetComponent<Image>();
            m_infoboxNoImmuneText   = statusIcons.Find("None").GetComponent<Text>();
        }

        internal void Update()
        {
            if (!m_LinkedCharacter)
            {
                if (SplitScreenManager.Instance.LocalPlayerCount > Split_ID && SplitScreenManager.Instance.LocalPlayers[Split_ID].AssignedCharacter)
                {
                    m_LinkedCharacter = SplitScreenManager.Instance.LocalPlayers[Split_ID].AssignedCharacter;
                }
                else
                {
                    DisableHolders();   
                }
            }
            else
            {
                if (m_LinkedCharacter.TargetingSystem.Locked && m_LinkedCharacter.TargetingSystem.LockedCharacter)
                {
                    UpdateTarget();
                }
                else
                {
                    DisableHolders();
                }
            }
        }

        private void UpdateTarget()
        {
            var target = m_LinkedCharacter.TargetingSystem.LockedCharacter;

            if (target.UID != m_lastTargetUID)
            {
                m_lastTargetUID = target.UID;
                UpdateOnTargetChange();
            }

            if (CombatHUD.IsHudHidden(this.Split_ID))
            {
                DisableHolders();
            }
            else
            {
                UpdateTargetHUD(target);

                if ((bool)CombatHUD.config.GetValue(Settings.EnemyInfobox))
                {
                    UpdateInfobox(target);
                }

                EnableHolders();
            }
        }

        private void UpdateInfobox(Character target)
        {
            float x;
            float y;
            if (Split_ID == 0)
            {
                x = (float)CombatHUD.config.GetValue(Settings.Infobox_P1_X);
                y = (float)CombatHUD.config.GetValue(Settings.Infobox_P1_Y);
            }
            else
            {
                x = (float)CombatHUD.config.GetValue(Settings.Infobox_P2_X);
                y = (float)CombatHUD.config.GetValue(Settings.Infobox_P2_Y);
            }

            var rect = m_infoboxHolder.GetComponent<RectTransform>();
            if (m_startPos == Vector2.zero)
            {
                m_startPos = rect.position;
            }
            if (m_currentOffset.x != x || m_currentOffset.y != y)
            {
                m_currentOffset = new Vector2(x, y);
                rect.position = m_startPos + m_currentOffset;
            }

            m_infoboxHealth.text = Math.Round(target.Stats.CurrentHealth) + " / " + Math.Round(target.Stats.ActiveMaxHealth);
            m_infoboxImpact.text = Math.Round(target.Stats.GetImpactResistance()).ToString();

            for (int i = 0; i < 6; i++)
            {
                float value = target.Stats.GetDamageResistance((DamageType.Types)i) * 100f;
                m_infoboxDamageTexts[i].text = Math.Round(value).ToString();

                if (value > 0)
                {
                    m_infoboxDamageTexts[i].color = new Color(0.3f, 1.0f, 0.3f);
                }
                else if (value < 0)
                {
                    m_infoboxDamageTexts[i].color = new Color(1.0f, 0.4f, 0.4f);
                }
                else
                {
                    m_infoboxDamageTexts[i].color = new Color(0.3f, 0.3f, 0.3f);
                }
            }
        }

        private void UpdateTargetHUD(Character target)
        {
            // update health text
            if ((bool)CombatHUD.config.GetValue(Settings.EnemyHealth))
            {
                if (!m_targetHealthText.gameObject.activeSelf)
                {
                    m_targetHealthText.gameObject.SetActive(true);
                }
                m_targetHealthText.text = Math.Round(target.Stats.CurrentHealth) + " / " + Math.Round(target.Stats.ActiveMaxHealth);
                m_targetHealthText.rectTransform.position = RectTransformUtility.WorldToScreenPoint(m_LinkedCharacter.CharacterCamera.CameraScript, target.UIBarPosition);
                m_targetHealthText.rectTransform.position += Vector3.up * CombatHUD.Rel(10f, true);
            }
            else if (m_targetHealthText.gameObject.activeSelf)
            {
                m_targetHealthText.gameObject.SetActive(false);
            }

            if ((bool)CombatHUD.config.GetValue(Settings.EnemyStatus))
            {
                if (!m_StatusHolder.activeSelf)
                {
                    m_StatusHolder.SetActive(true);
                }
                UpdateStatuses(target);
            }
            else if (m_StatusHolder.activeSelf)
            {
                m_StatusHolder.SetActive(false);
            }
        }

        private void UpdateStatuses(Character target)
        {
            // update status icons
            float offset = 0f;
            float offsetInterval = CombatHUD.Rel(30f, true);

            var barPos = RectTransformUtility.WorldToScreenPoint(m_LinkedCharacter.CharacterCamera.CameraScript, target.UIBarPosition);
            var pos = barPos + new Vector2(CombatHUD.Rel(225f), 0);

            for (int i = 0; i < m_StatusHolder.transform.childCount; i++)
            {
                var obj = m_StatusHolder.transform.GetChild(i).gameObject;
                string identifier = obj.name;
                var status = target.StatusEffectMngr.Statuses.Find((x => x.IdentifierName == identifier));

                if (!status)
                {
                    obj.SetActive(false);
                }
                else
                {
                    var parentRect = obj.GetComponent<RectTransform>();
                    parentRect.position = new Vector3(pos.x, pos.y + offset);

                    var text = parentRect.transform.Find("Text").GetComponent<Text>();
                    
                    if ((bool)CombatHUD.config.GetValue(Settings.EnemyStatusTimers))
                    {
                        TimeSpan t = TimeSpan.FromSeconds(status.RemainingLifespan);
                        var s = $"{t.Minutes}:{t.Seconds:00}";
                        text.text = s;
                        text.color = Color.white;

                        if (!text.gameObject.activeSelf)
                        {
                            text.gameObject.SetActive(true);
                        }
                        if (!obj.activeSelf)
                        {
                            obj.SetActive(true);
                        }

                        offset -= offsetInterval;
                    }
                    else if (text.gameObject.activeSelf)
                    {
                        text.gameObject.SetActive(false);
                    }
                }
            }

            // buildups
            if ((bool)CombatHUD.config.GetValue(Settings.EnemyBuildup))
            {
                var dict = (IDictionary)At.GetField(target.StatusEffectMngr, "m_statusBuildUp");

                foreach (string name in dict.Keys)
                {
                    if (dict[name] == null)
                        continue;

                    if (!(m_StatusHolder.transform.Find(name) is Transform t))
                        continue;

                    var holder = t.gameObject;
                    if (holder.activeSelf)
                    {
                        // status is already active (ie. its 100%)
                        continue;
                    }

                    if (s_buildupField == null)
                        s_buildupField = dict[name].GetType().GetField("BuildUp");

                    float value = (float)s_buildupField.GetValue(dict[name]);

                    if (value > 0 && value < 100)
                    {
                        var parentRect = holder.GetComponent<RectTransform>();
                        parentRect.position = new Vector3(pos.x, pos.y + offset);
                        offset -= offsetInterval;

                        var text = holder.GetComponentInChildren<Text>();
                        text.text = Math.Round(value) + "%";
                        text.color = new Color(1.0f, 0.5f, 0.5f, value * 0.01f + 0.25f);

                        if (!holder.activeSelf)
                        {
                            holder.SetActive(true);
                        }
                    }
                }
            }
        }

        internal static FieldInfo s_buildupField;

        private void UpdateOnTargetChange()
        {
            var target = m_LinkedCharacter.TargetingSystem.LockedCharacter;

            m_infoboxName.text = target.Name;

            // only update status immunities when we change targets.
            List<string> immunityTags = new List<string>();

            var statusNaturalImmunities = (TagSourceSelector[])At.GetField(target.Stats, "m_statusEffectsNaturalImmunity");
            foreach (var tagSelector in statusNaturalImmunities)
                immunityTags.Add(tagSelector.Tag.TagName);

            var statusImmunities = (Dictionary<Tag, List<string>>)At.GetField(target.Stats, "m_statusEffectsImmunity");
            foreach (var entry in statusImmunities)
            {
                if (entry.Value.Count > 0)
                    immunityTags.Add(entry.Key.TagName);
            }

            if (immunityTags.Count > 0)
            {
                m_infoboxNoImmuneText.gameObject.SetActive(false);
                float offset = 0f;
                var pos = m_infoboxNoImmuneText.rectTransform.position;

                if (immunityTags.Contains("Bleeding"))
                {
                    m_infoboxBleedingSprite.gameObject.SetActive(true);
                    m_infoboxBleedingSprite.rectTransform.position = new Vector3(pos.x, pos.y - 2f, 0);
                    offset += CombatHUD.Rel(22f);
                }
                else
                {
                    m_infoboxBleedingSprite.gameObject.SetActive(false);
                }
                if (immunityTags.Contains("Burning"))
                {
                    m_infoboxBurningSprite.gameObject.SetActive(true);
                    m_infoboxBurningSprite.rectTransform.position = new Vector3(pos.x + offset, pos.y - 2f, 0);
                    offset += CombatHUD.Rel(22f);
                }
                else
                {
                    m_infoboxBurningSprite.gameObject.SetActive(false);
                }
                if (immunityTags.Contains("Poison"))
                {
                    m_infoboxPoisonSprite.gameObject.SetActive(true);
                    m_infoboxPoisonSprite.rectTransform.position = new Vector3(pos.x + offset, pos.y - 2f, 0);
                }
                else
                {
                    m_infoboxPoisonSprite.gameObject.SetActive(false);
                }
            }
            else
            {
                m_infoboxNoImmuneText.gameObject.SetActive(true);

                m_infoboxBurningSprite.gameObject.SetActive(false);
                m_infoboxBleedingSprite.gameObject.SetActive(false);
                m_infoboxPoisonSprite.gameObject.SetActive(false);
            }
        }

        private void EnableHolders()
        {
            if (MenuManager.Instance.IsMapDisplayed || m_LinkedCharacter.CharacterUI.GetCurrentMenu() is MenuPanel panel && panel.IsDisplayed)
            {
                if (m_TargetHUDHolder.activeSelf)
                {
                    m_TargetHUDHolder.SetActive(false);
                }
            }
            else
            {
                if (!m_TargetHUDHolder.activeSelf)
                {
                    m_TargetHUDHolder.SetActive(true);
                }
            }

            if (!(bool)CombatHUD.config.GetValue(Settings.EnemyInfobox))
            {
                if (m_infoboxHolder.activeSelf)
                {
                    m_infoboxHolder.SetActive(false);
                }
            }
            else
            {
                if (!m_infoboxHolder.activeSelf)
                {
                    m_infoboxHolder.SetActive(true);
                }
            }
        }

        private void DisableHolders()
        {
            if (m_TargetHUDHolder.activeSelf)
            {
                m_TargetHUDHolder.SetActive(false);
            }

            if (m_infoboxHolder.activeSelf)
            {
                m_infoboxHolder.SetActive(false);
            }
        }
    }
}

