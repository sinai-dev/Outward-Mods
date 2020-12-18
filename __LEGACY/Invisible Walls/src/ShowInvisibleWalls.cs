using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using SharedModConfig;

namespace ShowInvisibleWalls
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SharedModConfig.SharedModConfig.GUID)]
    public class ShowInvisibleWalls : BaseUnityPlugin
    {
        const string GUID = "com.sinai.invisiblewalls";
        const string NAME = "InvisibleWallMod";
        const string VERSION = "1.1.0";

        private static readonly ModConfig config = new ModConfig
        {
            ModName = NAME,
            Settings = new List<BBSetting>
            {
                new BoolSetting
                {
                    Name = Settings.Disable,
                    DefaultValue = true,
                    Description = "Disable invisible boundaries"
                },
                new BoolSetting
                {
                    Name = Settings.Reveal,
                    DefaultValue = false,
                    Description = "Reveal invisible boundaries (overrides 'Disable Boundaries')",
                }
            },
        };

        private class Settings
        {
            public const string Disable = "Disable";
            public const string Reveal = "Reveal";
        }

        internal void Awake()
        {
            config.Register();

            config.OnSettingsSaved += OnSceneChange;
            SideLoader.SL.OnSceneLoaded += OnSceneChange;
        }

        private void OnSceneChange()
        {
            SetWalls();
        }

        private void SetWalls()
        {
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.scene.name != SceneManagerHelper.ActiveSceneName)
                {
                    continue;
                }

                string s = obj.name.ToLower();
                if (s.Contains("cube") || s.Contains("collision") || s.Contains("collider") || s.Contains("bounds"))
                {
                    // put this more costly check here
                    if (obj.GetComponentInParent<Item>())
                    {
                        continue;
                    }

                    Debug.Log(s);

                    //obj.SetActive(!(bool)config.GetValue(Settings.Disable));
                    if (obj.GetComponent<Collider>() is Collider col)
                    {
                        col.enabled = !(bool)config.GetValue(Settings.Disable);
                    }

                    var renderer = obj.GetOrAddComponent<MeshRenderer>();
                    if ((bool)config.GetValue(Settings.Reveal))
                    {
                        renderer.material = null;
                    }
                    else
                    {
                        DestroyImmediate(renderer);
                    }
                }
            }
        }


        //private void DisableVisuals(Character c)
        //{
        //    c.Visuals.Head.gameObject.SetActive(false);
        //    c.Visuals.ActiveVisualsBody.gameObject.SetActive(false);
        //    c.Visuals.ActiveVisualsFoot.gameObject.SetActive(false);
        //    c.Visuals.ActiveVisualsHelmOrHead.gameObject.SetActive(false);
        //    if (c.Inventory.EquippedBag && c.Inventory.EquippedBag.LoadedVisual)
        //    {
        //        c.Inventory.EquippedBag.LoadedVisual.gameObject.SetActive(false);
        //    }
        //}

    }
}
