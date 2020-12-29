using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SideLoader;
using SideLoader.Helpers;

namespace CombatHUD
{
    public class DamageLabels : MonoBehaviour
    {
        public static DamageLabels Instance;

        public static List<GameObject> LabelHolders = new List<GameObject>();
        public static List<DamageLabel> ActiveLabels = new List<DamageLabel>();

        internal void Awake()
        {
            Instance = this;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                LabelHolders.Add(child.gameObject);
            }

            SceneManager.sceneLoaded += OnSceneChange;
        }

        private void OnSceneChange(Scene scene, LoadSceneMode mode)
        {
            ActiveLabels.Clear();
            for (int j = 0; j < LabelHolders.Count; j++)
            {
                LabelHolders[j].SetActive(false);
            }
        }

        internal void Update()
        {
            //cleanup dead labels first
            var maxLifespan = (float)CombatHUD.config.GetValue(Settings.LabelLifespan);
            for (int z = 0; z < ActiveLabels.Count; z++)
            {
                if (Time.time - ActiveLabels[z].CreationTime > maxLifespan || !ActiveLabels[z].Target)
                {
                    ActiveLabels.RemoveAt(z);
                    z--;
                }
            }

            float ceiling = (float)CombatHUD.config.GetValue(Settings.DamageCeiling);
            int minsize = (int)(float)CombatHUD.config.GetValue(Settings.MinFontSize);
            int maxsize = (int)(float)CombatHUD.config.GetValue(Settings.MaxFontSize);

            if (maxsize < minsize)
                maxsize = minsize;

            for (int i = 0; i < SplitScreenManager.Instance.LocalPlayerCount; i++)
            {
                var player = SplitScreenManager.Instance.LocalPlayers[i];

                if (!player.AssignedCharacter)
                {
                    continue;
                }

                // dont show damage labels if player is in menu
                bool disable = false;
                if (CombatHUD.IsHudHidden(i) || MenuManager.Instance.IsMapDisplayed 
                    || (player.AssignedCharacter.CharacterUI.GetCurrentMenu() is MenuPanel panel && panel.IsDisplayed))
                {
                    disable = true;
                }

                var camera = player.CameraScript;
                int offset = i * 30;

                for (int j = 0 + offset; j < LabelHolders.Count; j++)
                {
                    if (disable || j - offset >= ActiveLabels.Count)
                    {
                        if (LabelHolders[j].activeSelf)
                        {
                            LabelHolders[j].SetActive(false);
                        }
                    }
                    else
                    {
                        var labelInfo = ActiveLabels[j - offset];
                        var labelHolder = LabelHolders[j];

                        var pos = (bool)CombatHUD.config.GetValue(Settings.LabelsStayAtHitPos) ? labelInfo.HitWorldPos : labelInfo.Target.CenterPosition;

                        float damageStrength = (float)((decimal)labelInfo.Damage / (decimal)ceiling); // set damage "strength"
                        float time = Time.time - labelInfo.CreationTime;
                        var timeOffset = Mathf.Lerp(0.3f, 0.07f, damageStrength) * time;

                        var screenPos = camera.WorldToViewportPoint(pos + new Vector3(0, timeOffset));
                        float distance = Vector3.Distance(player.AssignedCharacter.transform.position, pos);

                        if (IsScreenPosVisible(ref screenPos, i) && distance < (float)CombatHUD.config.GetValue(Settings.MaxDistance))
                        {
                            screenPos += new Vector3
                            (
                                CombatHUD.Rel(labelInfo.ranXpos),
                                CombatHUD.Rel(labelInfo.ranYpos, true)
                            );

                            labelHolder.GetComponent<RectTransform>().position = screenPos;

                            var text = labelHolder.GetComponent<Text>();
                            text.text = Math.Round(labelInfo.Damage).ToString();
                            text.fontSize = (int)Mathf.Lerp(minsize, maxsize, damageStrength);
                            text.color = labelInfo.TextColor;

                            if (!LabelHolders[j].activeSelf)
                            {
                                LabelHolders[j].SetActive(true);
                            }
                        }
                        else if (LabelHolders[j].activeSelf)
                        {
                            LabelHolders[j].SetActive(false);
                        }
                    }
                }
            }


        }

        private static bool IsScreenPosVisible(ref Vector3 screenPos, int splitID)
        {
            //var rect = HUDManager.Instance.HUDCanvas.GetComponent<RectTransform>().rect;

            float y1 = 0f;
            float y2 = 1080f;

            // x is always the same (1920)
            screenPos.x *= CombatHUD.Rel(1920f);

            if (SplitScreenManager.Instance.LocalPlayerCount == 1)
            {
                // single player height is 1080
                screenPos.y *= CombatHUD.Rel(1080f, true);
                return screenPos.z > 0f && screenPos.x >= 0f && screenPos.x <= 1920f && screenPos.y >= y1 && screenPos.y <= y2;
            }
            else
            {
                // split height is 540
                screenPos.y *= CombatHUD.Rel(540f, true);

                if (splitID == 0)
                {
                    y1 = CombatHUD.Rel(540f, true);
                    screenPos.y += y1;
                    return screenPos.z > 0f && screenPos.x >= 0f && screenPos.x <= CombatHUD.Rel(1920f) && screenPos.y >= y1 && screenPos.y <= y2;
                }
                else
                {
                    y2 = CombatHUD.Rel(540f, true);
                    return screenPos.z > 0f && screenPos.x >= 0f && screenPos.x <= CombatHUD.Rel(1920f) && screenPos.y >= y1 && screenPos.y <= y2;
                }
            }
        }

        public static void AddDamageLabel(DamageList damageList, Vector3 hitPosition, Character target)
        {
            if (damageList.TotalDamage < (float)CombatHUD.config.GetValue(Settings.MinimumDamage))
                return;

            if (target.IsAI && !(bool)CombatHUD.config.GetValue(Settings.PlayerDamageLabels))
                return;
            
            if (!target.IsAI && !(bool)CombatHUD.config.GetValue(Settings.EnemyDamageLabels))
                return;

            Color damagecolor = Color.white;
            
            if (!(bool)CombatHUD.config.GetValue(Settings.DisableColors))
            {
                float highest = 0f;
                foreach (DamageType type in damageList.List)
                {
                    if (type.Damage > highest)
                    {
                        highest = type.Damage;
                        damagecolor = GetDmgColor(type.Type);
                    }
                }
            }

            var x = CombatHUD.Rel(30f);
            var y = CombatHUD.Rel(15f, true);

            DamageLabel label = new DamageLabel
            {
                CreationTime = Time.time,
                TextColor = damagecolor,
                Damage = damageList.TotalDamage,
                HitWorldPos = hitPosition,
                Target = target,
                ranXpos = UnityEngine.Random.Range(-x, x),
                ranYpos = UnityEngine.Random.Range(-y, y)
            };

            ActiveLabels.Add(label);
        }

        public static Color GetDmgColor(DamageType.Types dtype)
        {
            Color color;
            switch (dtype)
            {
                case DamageType.Types.Physical:
                    color = Global.WHITE_GRAY;
                    break;
                case DamageType.Types.Ethereal:
                    color = Color.magenta;
                    break;
                case DamageType.Types.Decay:
                    color = Global.LIGHT_GREEN;
                    break;
                case DamageType.Types.Electric:
                    color = Color.yellow;
                    break;
                case DamageType.Types.Frost:
                    color = Global.BLUE;
                    break;
                case DamageType.Types.Fire:
                    color = lightRed;
                    break;
                case DamageType.Types.Raw:
                default:
                    color = Color.white;
                    break;
            }
            return color;
        }

        public static Color lightRed = new Color
        {
            r = 1.0f,
            g = 0.51f,
            b = 0.51f,
            a = 1.0f
        };   

        public class DamageLabel
        {
            public float CreationTime;
            public Vector3 HitWorldPos;
            public Character Target;

            public float Damage;
            public Color TextColor;

            public float ranXpos;
            public float ranYpos;
        }

        
    }
}
