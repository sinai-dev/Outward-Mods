using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using SharedModConfig;
using HarmonyLib;
using SideLoader;

namespace CombatAndDodgeOverhaul
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance;

        private List<string> FixedEnemies = new List<string>();
        private string CurrentScene = "";
        private bool SceneChanged = false;

        private ModConfig m_currentSyncInfos;
        private string m_currentHostUID = "";
        public float TimeOfLastSyncSend = -1f; // host set this when sends info
        private float m_timeOfLastSyncRequest = -5f; // non-host set this when receive info
        //private Coroutine _FindHostCoroutine;

        internal void Awake()
        {
            Instance = this;
        }

        // just for resetting FixedEnemies on any scene change
        internal void Update()
        {
            if (CurrentScene != SceneManagerHelper.ActiveSceneName)
            {
                SceneChanged = true;
                CurrentScene = SceneManagerHelper.ActiveSceneName;
            }

            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused)
            {
                return;
            }

            if (SceneChanged)
            {
                SceneChanged = false;
                m_currentHostUID = "";
                FixedEnemies.Clear();
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "ApplyCoopStats")]
        public class CharacterStats_ApplyCoopStats
        {
            // Reverse Patch, used to invoke the original.
            [HarmonyReversePatch]
            public static void ReversePatch(CharacterStats __instance)
            {
                throw new NotImplementedException("It's a stub!");
            }

            // Prefix return false override.
            [HarmonyPrefix]
            public static bool Prefix(CharacterStats __instance)
            {
                var self = __instance;

                var character = self.GetComponent<Character>();
                if (!character.IsAI)
                {
                    var tag = TagSourceManager.Instance.GetTag("81");
                    self.RemoveStatStack(tag, "CombatOverhaul", true);
                    self.AddStatStack(
                        tag,
                        new StatStack(
                            "CombatOverhaul",
                            0.01f * (float)CombatOverhaul.config.GetValue(Settings.Stamina_Cost_Stat)),
                        true);                    

                    return true;
                }

                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    // if we are world host....
                    if (!(bool)CombatOverhaul.config.GetValue(Settings.Enemy_Balancing))
                    {
                        return true;
                    }
                    else
                    {
                        Instance.SetEnemyMods(CombatOverhaul.config, self, character);
                    }
                }
                else // else we are not host
                {
                    //Debug.Log("non-host Apply stats hook");
                    if (Instance.UpdateSyncStats() || Instance.m_currentSyncInfos == null)
                    {
                        //Debug.Log("Settings need update - sent request for sync, and starting delayed orig(self)");
                        Instance.StartCoroutine(Instance.DelayedOrigInvoke(__instance));
                    }
                    else
                    {
                        if (Instance.m_currentSyncInfos != null && (bool)Instance.m_currentSyncInfos.GetValue(Settings.Enemy_Balancing))
                        {
                            //Debug.Log("No sync required, setting up stats from current cache");
                            Instance.SetEnemyMods(Instance.m_currentSyncInfos, self, character);
                        }
                        if (Instance.m_currentSyncInfos == null || !(bool)Instance.m_currentSyncInfos.GetValue(Settings.Enemy_Balancing))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private IEnumerator DelayedOrigInvoke(CharacterStats __instance)
        {
            float start = Time.time;
            while (Time.time - start < 5f && m_currentSyncInfos == null)
            {
                //Debug.Log("Delayed orig self - waiting for sync infos");
                if (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
                {
                    start += 1f;
                }
                yield return new WaitForSeconds(1.0f);
            }
            if (m_currentSyncInfos == null || !(bool)m_currentSyncInfos.GetValue(Settings.Enemy_Balancing))
            {
                try
                {
                    CharacterStats_ApplyCoopStats.ReversePatch(__instance);
                }
                catch { }
            }
        }

        //private IEnumerator DelayedOrigSelf(On.CharacterStats.orig_ApplyCoopStats orig, CharacterStats self)
        //{
        //    float start = Time.time;
        //    while (Time.time - start < 5f && m_currentSyncInfos == null)
        //    {
        //        //Debug.Log("Delayed orig self - waiting for sync infos");
        //        if (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
        //        {
        //            start += 1f;
        //        }
        //        yield return new WaitForSeconds(1.0f);
        //    }
        //    if (m_currentSyncInfos == null || !(bool)m_currentSyncInfos.GetValue(Settings.Enemy_Balancing))
        //    {
        //        try 
        //        { 
        //            orig(self);
        //        }
        //        catch { }
        //    }
        //}

        private bool UpdateSyncStats() // returns true if update is performed, false if no change.
        {
            if (CharacterManager.Instance.GetWorldHostCharacter() is Character host)
            {
                if (host.UID != m_currentHostUID)
                {
                    if (PhotonNetwork.isNonMasterClientInRoom)
                    {
                        if (Time.time - m_timeOfLastSyncRequest > 5f)
                        {
                            m_timeOfLastSyncRequest = Time.time;
                            m_currentSyncInfos = null;
                            RPCManager.Instance.RequestSettings();
                        }
                    }
                    else
                    {
                        m_currentHostUID = host.UID;
                        m_currentSyncInfos = CombatOverhaul.config;
                    }
                    return true;
                }
                else // host UID is already updated.
                {
                    return false;
                }
            }
            else
            {
                Debug.Log("CombatOverhaul: Could not find host!");
                return true;
            }
        }

        // called from RPC manager
        public void SetSyncInfo(bool modsEnabled, bool enemiesAllied, bool customStats, float healthModifier, float damageModifier, float impactRes, float damageRes, float impactDmg)
        {
            m_currentHostUID = CharacterManager.Instance.GetWorldHostCharacter()?.UID;
            //Debug.Log("Received sync from host uid: " + m_currentHostUID);

            this.m_currentSyncInfos = new ModConfig
            {
                ModName = "CombatOverhaul_Sync",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting> 
                {
                    new BoolSetting
                    {
                        Name = Settings.All_Enemies_Allied,
                        m_value = enemiesAllied,
                        DefaultValue = false,
                    },
                    new BoolSetting
                    {
                        Name = Settings.Enemy_Balancing,
                        m_value = customStats
                    },
                    new FloatSetting
                    {
                        Name = Settings.Enemy_Health,
                        m_value = healthModifier
                    },
                    new FloatSetting
                    {
                        Name = Settings.Enemy_Damages,
                        m_value = damageModifier
                    },
                    new FloatSetting
                    {
                        Name = Settings.Enemy_Resistances,
                        m_value = damageRes
                    },
                    new FloatSetting
                    {
                        Name = Settings.Enemy_ImpactRes,
                        m_value = impactRes
                    },
                    new FloatSetting
                    {
                        Name = Settings.Enemy_ImpactDmg,
                        m_value = impactDmg
                    },
                }
            };

            // manually fix the settings dictionary since we are not using ModConfig.Register()
            var dict = new Dictionary<string, BBSetting>();
            foreach (var setting in m_currentSyncInfos.Settings)
            {
                dict.Add(setting.Name, setting);
            }
            At.SetField(m_currentSyncInfos, "m_Settings", dict);

            if (modsEnabled)
            {
                //Debug.Log("Updating all current characters");
                foreach (Character c in CharacterManager.Instance.Characters.Values.Where(x => x.IsAI))
                {
                    SetEnemyMods(m_currentSyncInfos, c.Stats, c);
                }
            }
        }

        // actual function to set an enemy's stats
        private void SetEnemyMods(ModConfig _config, CharacterStats _stats, Character m_character)
        {
            if (m_character == null || !m_character.IsAI || m_character.Faction == Character.Factions.Player)
            {
                //Debug.Log("trying to set stats for a null character, or a non-AI character");
                return;
            }

            if (FixedEnemies.Contains(m_character.UID))
            {
                // Debug.Log("Fixed enemies already contains " + m_character.Name);
                return;
            }

            var m_staminaUseModiifers = new Stat(1f);
            m_staminaUseModiifers.AddMultiplierStack(new StatStack("MyStat", -0.9f));

            if ((bool)_config.GetValue(Settings.Enemy_Balancing))
            {
                string stackSource = "CombatOverhaul";

                if (!PhotonNetwork.isNonMasterClientInRoom)
                {
                    // set health modifier
                    var healthTag = TagSourceManager.Instance.GetTag("77");  // 77 = max health
                    var healthStack = new StatStack(stackSource, (float)_config.GetValue(Settings.Enemy_Health) - 1);
                    _stats.RemoveStatStack(healthTag, stackSource, true);
                    _stats.AddStatStack(healthTag, healthStack, true);
                    At.SetField(_stats, "m_health", _stats.CurrentHealth * (float)_config.GetValue(Settings.Enemy_Health));
                }

                // set impact resistance
                var impactTag = TagSourceManager.Instance.GetTag("84"); // 84 = impact res
                _stats.RemoveStatStack(impactTag, stackSource, false);
                var impactStack = new StatStack(stackSource, (float)_config.GetValue(Settings.Enemy_ImpactRes));
                _stats.AddStatStack(impactTag, impactStack, false); 

                // damage bonus
                var damageTag = TagSourceManager.Instance.GetTag("96"); // 96 = all damage bonus
                _stats.RemoveStatStack(damageTag, stackSource, true);
                var damageStack = new StatStack(stackSource, (float)_config.GetValue(Settings.Enemy_Damages) * 0.01f);
                _stats.AddStatStack(damageTag, damageStack, true);

                // impact modifier
                var impactModifier = At.GetField(_stats, "m_impactModifier") as Stat;
                impactModifier.RemoveStack(stackSource, true);
                impactModifier.AddStack(new StatStack(stackSource, (float)_config.GetValue(Settings.Enemy_ImpactDmg) * 0.01f), true);

                for (int i = 0; i < 6; i++)
                {
                    // damage resistance (Capped at 99, unless already 100)
                    float currentRes = m_character.Stats.GetDamageResistance((DamageType.Types)i);
                    if (currentRes < 100)
                    {
                        var valueToSet = (float)_config.GetValue(Settings.Enemy_Resistances);

                        if (currentRes + valueToSet >= 99)
                        {
                            valueToSet = 99 - currentRes;
                        }

                        int tag = 113 + i;  // 113 to 118 = damage resistance stats
                        var damageResTag = TagSourceManager.Instance.GetTag(tag.ToString());
                        _stats.RemoveStatStack(damageResTag, stackSource, true);
                        var resStack = new StatStack(stackSource, valueToSet);
                        _stats.AddStatStack(damageResTag, resStack, false);
                    }
                }
            }

            if ((bool)_config.GetValue(Settings.All_Enemies_Allied))
            {
                m_character.ChangeFaction(Character.Factions.Bandits);
                m_character.TargetingSystem.AlliedToSameFaction = true;

                Character.Factions[] targets = new Character.Factions[] { Character.Factions.Player };
                At.SetField(m_character.TargetingSystem, "TargetableFactions", targets);

                // fix skills
                foreach (var uid in m_character.Inventory.SkillKnowledge.GetLearnedActiveSkillUIDs())
                {
                    if (ItemManager.Instance.GetItem(uid) is Skill skill)
                    {
                        foreach (Shooter shooter in skill.GetComponentsInChildren<Shooter>())
                        {
                            shooter.Setup(targets, shooter.transform.parent);
                        }
                    }
                }
            }

            FixedEnemies.Add(m_character.UID);
        }
    }
}
