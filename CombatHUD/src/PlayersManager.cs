using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SideLoader;

namespace CombatHUD
{
    public class PlayersManager : MonoBehaviour
    {
        public static PlayersManager Instance;

        private readonly List<GameObject> m_labelHolders = new List<GameObject>();

        internal void Awake()
        {
            Instance = this;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                m_labelHolders.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
        }

        private bool wasInMenu = false;

        internal void Update()
        {
            if (NetworkLevelLoader.Instance.IsGameplayLoading || NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                if (!wasInMenu)
                {
                    for (int i = 0; i < m_labelHolders.Count; i++)
                    {
                        if (m_labelHolders[i].activeSelf)
                        {
                            m_labelHolders[i].SetActive(false);
                        }
                    }
                    wasInMenu = true;
                }

                return;
            }

            wasInMenu = false;

            List<StatusEffectInfo> statusInfos = new List<StatusEffectInfo>();

            for (int i = 0; i < SplitScreenManager.Instance.LocalPlayers.Count; i++)
            {
                var player = SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter;

                if (!player || CombatHUD.IsHudHidden(i))
                {
                    continue;
                }

                UpdateVitalText(player);

                if ((bool)CombatHUD.config.GetValue(Settings.PlayerStatusTimers))
                {
                    try
                    {
                        UpdatePlayerStatuses(i, ref statusInfos);
                    }
                    catch //(Exception e)
                    {
                        //Debug.LogError("Error updating statuses: " + e.Message);
                    }
                }
            }

            // update text holders
            for (int i = 0; i < m_labelHolders.Count; i++)
            {
                if (i >= statusInfos.Count || !(bool)CombatHUD.config.GetValue(Settings.PlayerStatusTimers))
                {
                    if (m_labelHolders[i].activeSelf)
                    {
                        m_labelHolders[i].SetActive(false);
                    }
                }
                else
                {
                    var text = m_labelHolders[i].GetComponent<Text>();

                    var iconRect = statusInfos[i].LinkedIcon.RectTransform;
                    var posOffset = new Vector3(0, CombatHUD.Rel(25f, true), 0);
                    text.GetComponent<RectTransform>().position = iconRect.position + posOffset;

                    TimeSpan t = TimeSpan.FromSeconds(statusInfos[i].TimeRemaining);
                    text.text = t.Minutes + ":" + t.Seconds.ToString("00");

                    if (statusInfos[i].TimeRemaining < 15)
                    {
                        text.color = Color.red;
                    }
                    else
                    {
                        text.color = Color.white;
                    }
                    if (!m_labelHolders[i].activeSelf)
                    {
                        m_labelHolders[i].SetActive(true);
                    }
                }
            }
        }

        private void UpdateVitalText(Character player)
        {
            CharacterBarListener manager = player.CharacterUI.transform.Find("Canvas/GameplayPanels/HUD/MainCharacterBars").GetComponent<CharacterBarListener>();

            if (!manager)
                return;

            var healthBar = At.GetField("m_healthBar", manager) as Bar;
            var manaBar = At.GetField("m_manaBar", manager) as Bar;
            var stamBar = At.GetField("m_staminaBar", manager) as Bar;

            if (!healthBar || !manaBar || !stamBar)
                return;

            var healthText = At.GetField("m_lblValue", healthBar) as Text;
            var manaText = At.GetField("m_lblValue", manaBar) as Text;
            var stamText = At.GetField("m_lblValue", stamBar) as Text;

            healthText.fontSize = 14;
            manaText.fontSize = 14;
            stamText.fontSize = 14;

            healthBar.TextValueDisplayed = (bool)CombatHUD.config.GetValue(Settings.PlayerVitals);
            manaBar.TextValueDisplayed = (bool)CombatHUD.config.GetValue(Settings.PlayerVitals);
            stamBar.TextValueDisplayed = (bool)CombatHUD.config.GetValue(Settings.PlayerVitals);
        }

        private void UpdatePlayerStatuses(int splitID, ref List<StatusEffectInfo> statusInfos)
        {
            var player = SplitScreenManager.Instance.LocalPlayers[splitID];

            if (player == null || !player.AssignedCharacter)
            {
                return;
            }

            var effectsManager = player.AssignedCharacter.StatusEffectMngr;
            var panel = player.CharUI.GetComponentInChildren<StatusEffectPanel>();

            if (!panel || !effectsManager)
            {
                Debug.LogError("Could not find status effect managers for " + player.AssignedCharacter.Name);
                return;
            }

            var activeIcons = At.GetField("m_statusIcons", panel) as Dictionary<string, StatusEffectIcon>;

            foreach (var entry in activeIcons)
            {
                if (!entry.Value.gameObject.activeSelf)
                {
                    continue;
                }

                float remainingLifespan = 0f;

                StatusEffect status = effectsManager.Statuses.Find((s => s.IdentifierName == entry.Key));
                if (status)
                {
                    remainingLifespan = status.RemainingLifespan;
                }
                else
                {
                    // some statuses use an identifier tag instead of their own status name for the icon...
                    switch (entry.Key.ToLower())
                    {
                        case "imbuemainweapon":
                            remainingLifespan = panel.LocalCharacter.CurrentWeapon.FirstImbue.RemainingLifespan;
                            break;
                        case "imbueoffweapon":
                            remainingLifespan = panel.LocalCharacter.LeftHandWeapon.FirstImbue.RemainingLifespan;
                            break;
                        case "summonweapon":
                            remainingLifespan = panel.LocalCharacter.CurrentWeapon.SummonedEquipment.RemainingLifespan;
                            break;
                        case "summonghost":
                            remainingLifespan = panel.LocalCharacter.CurrentSummon.RemainingLifespan;
                            break;
                        case "129": // marsh poison uses "129" for its tag, I think that's its effect preset ID?
                            if (effectsManager.Statuses.Find((it => it.IdentifierName.Equals("Hallowed Marsh Poison Lvl1"))) is StatusEffect marshpoison)
                                remainingLifespan = marshpoison.RemainingLifespan;
                            break;
                        default:
                            //Debug.Log("[CombatHUD] Unhandled Status Identifier! Key: " + entry.Key);
                            continue;
                    }
                }

                if (remainingLifespan > 0f && entry.Value)
                {
                    statusInfos.Add(new StatusEffectInfo
                    {
                        TimeRemaining = remainingLifespan,
                        LinkedIcon = entry.Value
                    });
                }
            }
        }

        public class StatusEffectInfo
        {
            public float TimeRemaining;
            public StatusEffectIcon LinkedIcon;
        }
    }
}
