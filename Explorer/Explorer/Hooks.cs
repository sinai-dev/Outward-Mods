using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using HarmonyLib;

namespace Explorer
{
    public class Hooks
    {
        // *************** FIX DEBUG AREA SWITCH NAMES *********** //

        [HarmonyPatch(typeof(DT_CharacterCheats), "InitAreaSwitches")]
        public class DT_CharacterCheats_InitAreaSwitches
        {
            [HarmonyPostfix]
            public static void Postfix(DT_CharacterCheats __instance)
            {
                var self = __instance;
                var families = At.GetValue(typeof(DT_CharacterCheats), self, "m_ddFamilies") as Dropdown[];

                foreach (var dd in families)
                {
                    if (dd == null) { continue; }

                    var trigger = dd.GetComponent<EventTrigger>();
                    if (trigger == null) { continue; }

                    var _event = new EventTrigger.Entry()
                    {
                        eventID = EventTriggerType.PointerUp
                    };
                    _event.callback.AddListener(delegate (BaseEventData data) { OnAreaSelected(dd); });

                    trigger.triggers.Add(_event);
                }
            }
        }

        private static void OnAreaSelected(Dropdown dd)
        {
            foreach (var option in dd.options)
            {
                if (SceneBuildNames.ContainsKey(option.text))
                {
                    option.text = SceneBuildNames[option.text];
                }
            }
        }

        public static Dictionary<string, string> SceneBuildNames = new Dictionary<string, string>
        {
            { "CierzoTutorial", "Shipwreck Beach" },
            { "CierzoNewTerrain", "Cierzo" },
            { "CierzoDestroyed", "Cierzo (Destroyed)" },
            { "ChersoneseNewTerrain", "Chersonese" },
            { "Chersonese_Dungeon1", "Vendavel Fortress" },
            { "Chersonese_Dungeon2", "Blister Burrow" },
            { "Chersonese_Dungeon3", "Ghost Pass" },
            { "Chersonese_Dungeon4_BlueChamber", "Blue Chamber’s Conflux Path" },
            { "Chersonese_Dungeon4_HolyMission", "Holy Mission’s Conflux Path" },
            { "Chersonese_Dungeon4_Levant", "Heroic Kingdom’s Conflux Path" },
            { "Chersonese_Dungeon5", "Voltaic Hatchery" },
            { "Chersonese_Dungeon4_CommonPath", "Conflux Chambers" },
            { "Chersonese_Dungeon6", "Corrupted Tombs" },
            { "Chersonese_Dungeon8", "Cierzo Storage" },
            { "Chersonese_Dungeon9", "Montcalm Clan Fort" },
            { "ChersoneseDungeonsSmall", "Chersonese Misc. Dungeons" },
            { "ChersoneseDungeonsBosses", "Unknown Arena" },
            { "Monsoon", "Monsoon" },
            { "HallowedMarshNewTerrain", "Hallowed Marsh" },
            { "Hallowed_Dungeon1", "Jade Quarry" },
            { "Hallowed_Dungeon2", "Giants’ Village" },
            { "Hallowed_Dungeon3", "Reptilian Lair" },
            { "Hallowed_Dungeon4_Interior", "Dark Ziggurat Interior" },
            { "Hallowed_Dungeon5", "Spire of Light" },
            { "Hallowed_Dungeon6", "Ziggurat Passage" },
            { "Hallowed_Dungeon7", "Dead Roots" },
            { "HallowedDungeonsSmall", "Marsh Misc. Dungeons" },
            { "HallowedDungeonsBosses", "Unknown Arena" },
            { "Levant", "Levant" },
            { "Abrassar", "Abrassar" },
            { "Abrassar_Dungeon1", "Undercity Passage" },
            { "Abrassar_Dungeon2", "Electric Lab" },
            { "Abrassar_Dungeon3", "The Slide" },
            { "Abrassar_Dungeon4", "Stone Titan Caves" },
            { "Abrassar_Dungeon5", "Ancient Hive" },
            { "Abrassar_Dungeon6", "Sand Rose Cave" },
            { "AbrassarDungeonsSmall", "Abrassar Misc. Dungeons" },
            { "AbrassarDungeonsBosses", "Unknown Arena" },
            { "Berg", "Berg" },
            { "Emercar", "Enmerkar Forest" },
            { "Emercar_Dungeon1", "Royal Manticore’s Lair" },
            { "Emercar_Dungeon2", "Forest Hives" },
            { "Emercar_Dungeon3", "Cabal of Wind Temple" },
            { "Emercar_Dungeon4", "Face of the Ancients" },
            { "Emercar_Dungeon5", "Ancestor’s Resting Place" },
            { "Emercar_Dungeon6", "Necropolis" },
            { "EmercarDungeonsSmall", "Enmerkar Misc. Dungeons" },
            { "EmercarDungeonsBosses", "Unknown Arena" },
            { "DreamWorld", "In Between" },
            { "Harmattan",                          "Harmattan" },
            { "AntiqueField",                       "Antique Plateau" },
            { "AntiqueField_Dungeon1",              "Abandoned Living Quarters" },
            { "AntiqueField_Dungeon2",              "Forgotten Research Laboratory" },
            { "AntiqueField_Dungeon3",              "Ancient Foundry" },
            { "AntiqueField_Dungeon4",              "Ruined Warehouse" },
            { "AntiqueField_Dungeon5",              "Lost Golem Manufacturing Facility" },
            { "AntiqueField_Dungeon6",              "Crumbling Loading Docks" },
            { "AntiqueField_Dungeon7",              "Destroyed Test Chambers" },
            { "AntiqueField_Dungeon8",              "Compromised Mana Transfer Station" },
            { "AntiqueField_Dungeon9",              "Old Harmattan" },
            { "AntiqueFieldDungeonsBosses",         "Unknown Arena" },
            { "AntiqueFieldDungeonsSmall",          "Harmattan Misc. Dungeons" },
        };

        // ****************** SKIP LOGOS HOOK ********************** //

        [HarmonyPatch(typeof(StartupVideo), "Start")]
        public class StartupVideo_Start
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                StartupVideo.HasPlayedOnce = true;
            }
        }

        // ********************* QUEST HOOKS ********************* //

        [HarmonyPatch(typeof(SendQuestEventInteraction), "OnActivate")]
        public class SendQuestEventInteraction_OnActivate
        {
            public static void Prefix(SendQuestEventInteraction __instance)
            {
                var self = __instance;

                var _ref = At.GetValue(typeof(SendQuestEventInteraction), self, "m_questReference") as QuestEventReference;
                var _event = _ref.Event;
                var s = _ref.EventUID;

                if (_event != null && s != null)
                {
                    LogQuestEvent(_event, -1);
                }
            }
        }

        [HarmonyPatch(typeof(NodeCanvas.Tasks.Actions.SendQuestEvent), "OnExecute")]
        public class SendQuestEvent_OnExecute
        {
            [HarmonyPrefix]
            public static void Prefix(NodeCanvas.Tasks.Actions.SendQuestEvent __instance) 
            {
                var self = __instance;
                var _event = self.QuestEventRef.Event;
                //var s = self.QuestEventRef.EventUID;

                if (_event != null)
                {
                    LogQuestEvent(_event, self.StackAmount);
                }
            }
        }

        public static void LogQuestEvent(QuestEventSignature _event, int stack = -1)
        {
            if (Explorer.QuestDebugging)
            {
                Debug.LogWarning(
                "------ ADDING QUEST EVENT -------" +
                "\r\nName: " + _event.EventName +
                "\r\nDescription: " + _event.Description +
                (stack == -1 ? "" : "\r\nStack: " + stack) +
                "\r\n---------------------------");
            }
        }
    }
}
