using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ShowInvisibleWalls
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ShowInvisibleWalls : BaseUnityPlugin
    {
        const string GUID = "com.sinai.invisiblewalls";
        const string NAME = "InvisibleWallMod";
        const string VERSION = "1.1.0";

        internal const string CTG_NAME = "Settings";
        public ConfigEntry<bool> RevealBoundaries;
        //public ConfigEntry<bool> DisableBoundaryColliders;

        private class Settings
        {
            public const string Disable = "Disable";
            public const string Reveal = "Reveal";
        }

        internal void Awake()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            RevealBoundaries = Config.Bind(CTG_NAME, "Reveal invisible boundaries?", true, "Puts a semi-transparent pink shader on every invisible boundary.");
            //DisableBoundaryColliders = Config.Bind(CTG_NAME, "Disable boundary colliders?", false, "Disables the colliders on all invisible boundaries.");

            RevealBoundaries.SettingChanged += RevealBoundaries_SettingChanged;
            //DisableBoundaryColliders.SettingChanged += RevealBoundaries_SettingChanged;
        }

        private void RevealBoundaries_SettingChanged(object sender, EventArgs e)
        {
            SetWalls();
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            StartCoroutine(WaitForSceneLoaded());
        }

        private IEnumerator WaitForSceneLoaded()
        {
            while (NetworkLevelLoader.Instance.IsGameplayPaused || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue)
                yield return new WaitForSeconds(1f);

            SetWalls();
        }

        private void SetWalls()
        {
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>()
                                                .Where(it => it.scene.name == SceneManagerHelper.ActiveSceneName))
            {
                string s = obj.name.ToLower();
                if (s.Contains("cube") || s.Contains("collision") || s.Contains("collider") || s.Contains("bounds"))
                {
                    if (obj.GetComponentInParent<Item>())
                        continue;

                    //Debug.Log(s);

                    //if (DisableBoundaryColliders.Value)
                    //{
                    //    if (obj.GetComponent<Collider>() is Collider col)
                    //        col.enabled = false;
                    //}
                    
                    if (RevealBoundaries.Value)
                    {
                        var renderer = obj.GetOrAddComponent<MeshRenderer>();
                        renderer.material = null;
                    }
                }
            }
        }
    }
}
