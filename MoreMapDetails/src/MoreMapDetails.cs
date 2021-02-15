using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;
using SideLoader;
using BepInEx.Configuration;
using MoreMapDetails.MapConfigs;

namespace MoreMapDetails
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class MoreMapDetails : BaseUnityPlugin
    {
        const string GUID = "com.sinai.moremapdetails";
        const string NAME = "More Map Details";
        const string VERSION = "1.5";

        public static MoreMapDetails Instance;

        private int m_mapID;

        // enemy markers
        public static List<EnemyMarker> EnemyMarkers = new List<EnemyMarker>();
        private static Transform m_enemyMarkerHolder;
        private static readonly List<EnemyMarkerDisplay> m_enemyTexts = new List<EnemyMarkerDisplay>();

        // bag markers
        private static readonly List<MapWorldMarker> m_bagMarkers = new List<MapWorldMarker>();

        //// custom icon markers
        //private Transform m_iconHolder;
        //private List<MapMarkerIconDisplay> m_unmarkedDungeons = new List<MapMarkerIconDisplay>();

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            SetupConfig();

            StartCoroutine(SetupCoroutine());
        }

        // =====================  CONFIG  ===================== //

        public static ConfigEntry<bool> s_ShowPlayerMarkers;
        public static ConfigEntry<bool> s_ShowPlayerBagMarkers;
        public static ConfigEntry<bool> s_ShowEnemyMarkers;
        public static ConfigEntry<bool> s_ShowSoroborean;

        private void SetupConfig()
        {
            s_ShowPlayerMarkers = Config.Bind("Map Settings", "Show map markers for Players", true);
            s_ShowPlayerBagMarkers = Config.Bind("Map Settings", "Show map markers for Player Bags", true);
            s_ShowEnemyMarkers = Config.Bind("Map Settings", "Show map markers for Enemies", true);
            s_ShowSoroborean = Config.Bind("Map Settings", "Show map markers for Soroborean Caravanner", true);
        }

        // ===================== Core ===================== //

        // wait for MapDisplay Instance to start up
        private IEnumerator SetupCoroutine()
        {
            while (MapDisplay.Instance == null || MapDisplay.Instance.WorldMapMarkers == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            m_enemyMarkerHolder = new GameObject("CustomMarkerHolder").transform;
            DontDestroyOnLoad(m_enemyMarkerHolder.gameObject);

            m_enemyMarkerHolder.transform.parent = MapDisplay.Instance.WorldMapMarkers.parent;
            m_enemyMarkerHolder.transform.position = MapDisplay.Instance.WorldMapMarkers.position;
            m_enemyMarkerHolder.transform.localScale = Vector3.one;

            //m_iconHolder = new GameObject("CustomIconHolder").transform;
            //DontDestroyOnLoad(m_iconHolder);

            //m_iconHolder.transform.parent = MapDisplay.Instance.WorldMapMarkers.parent;
            //m_iconHolder.transform.position = MapDisplay.Instance.WorldMapMarkers.position;
            //m_iconHolder.transform.localScale = Vector3.one;
        }

        // ==================== HOOKS ==================== //

        /* 
         * HOOK MapDisplay.Show
         * This is where we setup our custom markers. 
         * If a marker already exists on the object, it is skipped.
         * A MapWorldMarker will automatically update its position on the map, based on the gameobject it is attached to.
        */

        [HarmonyPatch(typeof(MapDisplay), "Show", new Type[] { typeof(CharacterUI) })]
        public class MapDisplay_Show
        {
            [HarmonyPostfix]
            public static void Postfix(MapDisplay __instance, CharacterUI _owner)
            {
                var self = __instance;

                Instance.m_mapID = (int)At.GetField(self, "m_currentMapSceneID");

                if (!(bool)At.GetField(self, "m_currentAreaHasMap"))
                {
                    return;
                }

                //Debug.LogWarning("Current map ID: " + Instance.m_mapID);

                if (MapConfig.ConfigDict.ContainsKey(Instance.m_mapID))
                {
                    self.CurrentMapScene.MarkerOffset = MapConfig.ConfigDict[Instance.m_mapID].MarkerOffset;
                    self.CurrentMapScene.Rotation = MapConfig.ConfigDict[Instance.m_mapID].Rotation;
                    self.CurrentMapScene.MarkerScale = MapConfig.ConfigDict[Instance.m_mapID].MarkerScale;
                }

                // caldera node finder

                //var nodes = Resources.FindObjectsOfTypeAll<Gatherable>()
                //                     .Where(it => it.gameObject.scene.name == SceneManagerHelper.ActiveSceneName
                //                               && it.Name.Contains("Unidentified")
                //                               && !it.gameObject.GetComponent<MapWorldMarker>());

                //foreach (var node in nodes)
                //{
                //    var parent = node.transform.parent.parent;
                //    foreach (Transform child in parent)
                //    {
                //        if (child.name.Contains("Pos"))
                //        {
                //            string s;
                //            if (node.Name.Contains("Molepig"))
                //                s = "M";
                //            else if (node.Name.Contains("Ore"))
                //                s = "O";
                //            else
                //                s = "P";
                //            var id = parent.name.Substring(parent.name.Length - 1, 1);
                //            Instance.AddEnemyWorldMarker(child.gameObject, $"{s}{id}");
                //        }
                //    }
                //}

                var list = CharacterManager.Instance.Characters.Values
                    .Where(x =>
                        !x.GetComponentInChildren<MapWorldMarker>()
                        && !x.IsDead
                        && x.gameObject.activeSelf);

                foreach (Character c in list)
                {
                    if (!c.IsAI)
                    {
                        // player markers
                        if (s_ShowPlayerMarkers.Value)
                            Instance.AddWorldMarker(c.gameObject, c.Name);
                    }
                    else
                    {
                        // enemy markers
                        if (s_ShowEnemyMarkers.Value)
                            Instance.AddEnemyWorldMarker(c.gameObject, c.Name);
                    }
                }

                // caravanner
                if (s_ShowSoroborean.Value)
                {
                    var caravanner = GameObject.Find("HumanSNPC_CaravanTrader");

                    if (!caravanner)
                        caravanner = GameObject.Find("UNPC_CaravanTraderA");

                    if (caravanner && !caravanner.GetComponentInChildren<MapWorldMarker>())
                        Instance.AddWorldMarker(caravanner, "Soroborean Caravanner");
                }

                // player bags
                if (s_ShowPlayerBagMarkers.Value)
                {
                    foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                    {
                        var c = ps.ControlledCharacter;
                        if (c.Inventory.Equipment.LastOwnedBag != null && c.Inventory.Equipment.LastOwnedBag.OwnerCharacter == null)
                        {
                            var tempObject = new GameObject("TempBagHolder");
                            tempObject.transform.position = c.Inventory.Equipment.LastOwnedBag.transform.position;
                            var marker = Instance.AddWorldMarker(tempObject, c.Name + "'s Bag");
                            m_bagMarkers.Add(marker);
                        }
                    }
                }

                ////unmarked dungeons
                //if (ModBase.settings.Show_Unmarked_Dungeons && MapConfigs.ContainsKey(m_mapID))
                //{
                //    var disabledObjects = FindDisabledGameObjectsByName(MapConfigs[m_mapID].UnmarkedDungeonObjects.Keys.ToList());

                //    for (int i = 0; i < MapConfigs[m_mapID].UnmarkedDungeonObjects.Count; i++)
                //    {
                //        var entry = MapConfigs[m_mapID].UnmarkedDungeonObjects.ElementAt(i);
                //        var go = disabledObjects[i];

                //        AddIconMarker(go, entry.Value);
                //    }
                //}

                return;
            }
        }

        /* 
         * HOOK MapDisplay.UpdateWorldMarkers
         * Just adding on our custom enemy marker update here.
        */

        [HarmonyPatch(typeof(MapDisplay), "UpdateWorldMarkers")]
        public class MapDisplay_UpdateWorldMarkers
        {
            [HarmonyFinalizer]
            public static Exception Finalizer(MapDisplay __instance, Exception __exception)
            {
                var self = __instance;

                bool flag = !(self.CurrentMapScene.MarkerOffset == Vector2.zero) || !(self.CurrentMapScene.MarkerScale == Vector2.zero);

                if (flag)
                {
                    // update EnemyMarker positions
                    float zoomLevelSmooth = (float)At.GetField(MapDisplay.Instance, "m_zoomLevelSmooth");
                    for (int i = 0; i < EnemyMarkers.Count; i++)
                    {
                        EnemyMarkers[i].CalculateMapPosition(MapDisplay.Instance.CurrentMapScene, i, zoomLevelSmooth * 1.0351562f);
                        At.SetField(EnemyMarkers[i], "m_adjustedMapPosition", EnemyMarkers[i].MapPosition);
                    }
                }

                // update enemy marker texts
                for (int i = 0; i < m_enemyTexts.Count; i++)
                {
                    if (i < EnemyMarkers.Count)
                    {
                        if (!m_enemyTexts[i].gameObject.activeSelf)
                            m_enemyTexts[i].SetActive(true);

                        m_enemyTexts[i].UpdateDisplay(EnemyMarkers[i]);
                    }
                    else
                    {
                        if (m_enemyTexts[i].gameObject.activeSelf)
                            m_enemyTexts[i].SetActive(false);
                    }
                }

                //if (__exception != null)
                //{
                //    Debug.Log("MapDisplay.UpdateWorldMarkers had an exception!");
                //    Debug.Log(__exception.ToString());
                //}

                return null;
            }
        }

        /*
         * HOOK MapDisplay.OnHide
         * Cleanup bags and unmarked dungeon markers
        */

        [HarmonyPatch(typeof(MapDisplay), "OnHide")]
        public class MapDisplay_OnHide
        {
            [HarmonyPostfix]
            public static void Postfix(MapDisplay __instance)
            {
                // bags
                if (m_bagMarkers.Count > 0)
                {
                    for (int i = 0; i < m_bagMarkers.Count; i++)
                    {
                        if (m_bagMarkers[i] != null)
                        {
                            Destroy(m_bagMarkers[i].gameObject);
                            m_bagMarkers.RemoveAt(i);
                            i--;
                        }
                    }
                }

                //// unmarked dungeons
                //for (int i = 0; i < m_unmarkedDungeons.Count; i++)
                //{
                //    if (m_unmarkedDungeons[i] != null)
                //    {
                //        Destroy(m_unmarkedDungeons[i].gameObject);
                //        m_unmarkedDungeons.RemoveAt(i);
                //        i--;
                //    }
                //}
            }
        }

        /*
         * HOOK Character.Die
         * Remove Enemy MapMarker on character death
        */
        [HarmonyPatch(typeof(Character), "Die")]
        public class Character_Die
        {
            [HarmonyPostfix]
            public static void Postfix(Character __instance, Vector3 _hitVec, bool _loadedDead = false)
            {
                var self = __instance;

                if (self.GetComponentInChildren<EnemyMarker>() is EnemyMarker enemymarker)
                {
                    if (EnemyMarkers.Contains(enemymarker))
                        EnemyMarkers.Remove(enemymarker);

                    Destroy(enemymarker.gameObject);
                }
            }
        }

        // ==================== CUSTOM FUNCTIONS ==================== //

        /*
         * AddWorldMarker
         * Adds a simple MapWorldMarker on a new gameobject as a child for the specified GameObject.
         * Returns the MapWorldMarker component.
        */
        public MapWorldMarker AddWorldMarker(GameObject go, string name)
        {
            var markerHolder = new GameObject("MarkerHolder");
            markerHolder.transform.parent = go.transform;
            markerHolder.transform.position = go.transform.position;

            // setup the marker
            MapWorldMarker marker = markerHolder.AddComponent<MapWorldMarker>();
            marker.ShowCircle = true;
            marker.AlignLeft = false;
            marker.Text = name;

            // check if we need to add another text holder
            var markerTexts = At.GetField(MapDisplay.Instance, "m_markerTexts") as MapWorldMarkerDisplay[];
            var mapMarkers = At.GetField(MapDisplay.Instance, "m_mapWorldMarkers") as List<MapWorldMarker>;
            if (markerTexts.Length < mapMarkers.Count)
                AddTextHolder(markerTexts);

            return marker;
        }

        /*
         * AddTextHolder
         * Add another MapWorldMarkerDisplay holder to the MapDisplay.m_markerTexts list.
         * The game will not add more if we use them all, so we have to do it ourselves
         * 
         * Note: Since I moved enemies to their own m_enemyTexts holder, we will probably never actually use this.
         * But incase I end up wanting to use more than the default text holders in the future, I'll leave this.
         * The only case I can think it would be used is maybe in Monsoon with MP Limit Remover and like 10+ people in the city.
        */
        private void AddTextHolder(MapWorldMarkerDisplay[] markerTexts)
        {
            // get any existing one to clone from
            var origTextHolder = MapDisplay.Instance.WorldMapMarkers.GetComponentInChildren<MapWorldMarkerDisplay>();
            var origCircle = origTextHolder.Circle;
            // copy the orig
            var newMarker = Instantiate(origTextHolder.gameObject).GetComponent<MapWorldMarkerDisplay>();
            newMarker.transform.SetParent(MapDisplay.Instance.WorldMapMarkers, false);
            newMarker.RectTransform.localScale = Vector3.one;
            // copy the circle
            newMarker.Circle = Instantiate(origCircle.gameObject).GetComponent<Image>();
            newMarker.Circle.transform.SetParent(origCircle.transform.parent, false);
            newMarker.Circle.transform.localScale = Vector3.one;
            newMarker.Circle.gameObject.SetActive(true);
            // add to list
            var list = markerTexts.ToList();
            list.Add(newMarker);
            // set value
            At.SetField(MapDisplay.Instance, "m_markerTexts", list.ToArray());
        }

        /*
         * AddEnemyWorldMarker
         * Basically the same as AddWorldMarker, but adds our custom EnemyMarker class.
        */

        public EnemyMarker AddEnemyWorldMarker(GameObject go, string name)
        {
            var markerHolder = new GameObject("MarkerHolder");
            markerHolder.transform.parent = go.transform;
            markerHolder.transform.position = go.transform.position;

            var marker = markerHolder.AddComponent<EnemyMarker>();
            marker.Text = name;
            marker.Anchored = true;
            marker.ShowCircle = false;
            marker.MarkerWidth = marker.Text.Length * 15f;

            // check if we need to add another text holder
            if (m_enemyTexts.Count < EnemyMarkers.Count)
                AddEnemyTextHolder();

            return marker;
        }

        /*
         * AddEnemyTextHolder
         * Same as AddTextHolder, but using our custom m_enemyTexts list, attached to our custom m_customMarkerHolder.
         * For the first map, we will do this for every active enemy, since our list starts our with 0 holders.
        */
        private void AddEnemyTextHolder()
        {
            try
            {
                // get any existing one to clone from
                var origTextHolder = MapDisplay.Instance.WorldMapMarkers.GetComponentInChildren<MapWorldMarkerDisplay>();

                // copy the orig as a custom Marker Class
                var tempMarker = Instantiate(origTextHolder.gameObject).GetComponent<MapWorldMarkerDisplay>();
                var newMarker = tempMarker.gameObject.AddComponent<EnemyMarkerDisplay>();
                At.CopyFields(newMarker, tempMarker);
                Destroy(tempMarker);

                //newMarker.transform.parent = m_enemyMarkerHolder;

                newMarker.transform.SetParent(m_enemyMarkerHolder, false);
                newMarker.transform.localScale = Vector3.one;

                if (newMarker.Circle)
                    newMarker.Circle.enabled = false;

                m_enemyTexts.Add(newMarker);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }
}