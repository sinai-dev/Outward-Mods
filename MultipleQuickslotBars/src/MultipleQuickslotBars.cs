using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SideLoader;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace MultipleQuickslotBars
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class MultipleQuickslotBars : BaseUnityPlugin
    {
        const string GUID = "com.sinai.multiplequickslotbars";
        const string NAME = "Multiple Quickslot Bars";
        const string VERSION = "1.1.0";

        const string NextKeyToggle = "Next Quickslot Bar";
        const string PrevKeyToggle = "Previous Quickslot Bar";

        internal const string CTG_NAME = "Settings";
        public static ConfigEntry<int> cfg_NumberOfExtraBars;

        internal static readonly Dictionary<string, QuickslotSaveExtension> s_quickslotSaves = new Dictionary<string, QuickslotSaveExtension>();

        public static QuickslotSaveExtension GetOrCreateSave(string characterUID)
        {
            QuickslotSaveExtension ret;

            if (!s_quickslotSaves.ContainsKey(characterUID))
                s_quickslotSaves.Add(characterUID, ret = new QuickslotSaveExtension());
            else if (!s_quickslotSaves.TryGetValue(characterUID, out ret))
                ret = s_quickslotSaves[characterUID] = new QuickslotSaveExtension();

            return ret;
        }

        internal void Awake()
        {
            cfg_NumberOfExtraBars = Config.Bind(CTG_NAME, "Extra Quickslot Bars", 1, "How many extra quickslot bars to add.");

            CustomKeybindings.AddAction(NextKeyToggle, KeybindingsCategory.CustomKeybindings, ControlType.Both);
            CustomKeybindings.AddAction(PrevKeyToggle, KeybindingsCategory.CustomKeybindings, ControlType.Both);
        }

        internal void Update()
        {
            try
            {
                if (MenuManager.Instance.IsInMainMenuScene || NetworkLevelLoader.Instance.IsGameplayPaused)
                    return;
            }
            catch
            {
                return;
            }

            if (CustomKeybindings.GetKeyDown(NextKeyToggle, out int playerID))
            {
                var player = SplitScreenManager.Instance.LocalPlayers[playerID];
                CycleQuickslotBar(player.AssignedCharacter, true);
            }

            if (CustomKeybindings.GetKeyDown(PrevKeyToggle, out playerID))
            {
                var player = SplitScreenManager.Instance.LocalPlayers[playerID];
                CycleQuickslotBar(player.AssignedCharacter, false);
            }
        }

        public static void CycleQuickslotBar(Character character, bool cycleForward)
        {
            int max = cfg_NumberOfExtraBars.Value;

            //Debug.Log("Cycling QS (forward: " + cycleForward + ", max: " + max + ")");

            if (max < 1)
                return;

            var ext = GetOrCreateSave(character.UID);

            ext.SetDataFromCharacter(character);

            int desired = ext.ActiveBarIndex + (cycleForward ? 1 : -1);

            if (desired < 0)
                desired = max;
            else if (desired > max)
                desired = 0;

            //Debug.Log("setting desired ActiveBarIndex: " + desired);

            ext.ActiveBarIndex = desired;

            QuickslotSaveExtension.ApplySaveToCharacter(character);
        }
    }
}
