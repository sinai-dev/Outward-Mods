using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using BepInEx;
using HarmonyLib;
using SideLoader;

namespace Dismantler
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Dismantler : BaseUnityPlugin
    {
        const string GUID = "com.sinai.dismantler";
        const string NAME = "Dismantler";
        const string VERSION = "1.1";

        public static Dismantler Instance;

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        [HarmonyPatch(typeof(Deployable), "InitFromItem")]
        public class Deployable_InitFromItem
        {
            [HarmonyPostfix]
            public static void Postfix(Deployable __instance)
            {
                var self = __instance;

                if (self.IsDeployed && self.PackedStateItemPrefab == null)
                {
                    Instance.AddDestroyInteraction(self);
                }
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            StartCoroutine(WaitForSceneReady());
        }

        private IEnumerator WaitForSceneReady()
        {
            while (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return null;
            }

            try
            {
                foreach (var deployable in ItemManager.Instance.WorldItems.Values.Where(x => x.GetExtension("Deployable") != null))
                {
                    var comp = deployable.GetComponent<Deployable>();

                    if (comp.IsDeployed && comp.PackedStateItemPrefab == null)
                    {
                        AddDestroyInteraction(comp);
                    }
                }
            }
            catch (Exception ex)
            {
                SL.LogWarning("Exception applying Dismantler interactions");
                SL.LogInnerException(ex);
            }
        }

        private void AddDestroyInteraction(Deployable self)
        {
            if (!self.PackedStateItemPrefab)
            {
                var m_item = self.Item;

                // Debug.Log("DeployableManager: Adding InteractionDestroy to " + m_item.Name + " (" + m_item.UID + ")");

                var holder = m_item.InteractionHolder;

                // add custom "Destroy" component
                var comp = m_item.InteractionHolder.AddComponent<InteractionDestroy>();

                // set up the activator and base for our hold action, and do what the game normally does for the disassemble interaction
                var activator = holder.GetComponent<InteractionActivator>();
                activator.AddHoldInteractionOverride(comp);

                var triggerbase = holder.GetComponent<InteractionTriggerBase>();                
                At.SetField(self, "m_disassembleInteractionTrigger", triggerbase);

                self.RefreshDisassembleCollider();
            }
        }
    }
}
