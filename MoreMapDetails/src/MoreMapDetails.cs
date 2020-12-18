using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SharedModConfig;
using BepInEx;
using HarmonyLib;

namespace MoreMapDetails
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.sinai.SharedModConfig", BepInDependency.DependencyFlags.HardDependency)]
    public class MoreMapDetails : BaseUnityPlugin
    {
        const string GUID = "com.sinai.moremapdetails";
        const string NAME = "More Map Details";
        const string VERSION = "1.3";

        public static MoreMapDetails Instance;

        public static ModConfig config;

        private int m_mapID;

        // enemy markers
        public List<EnemyMarker> EnemyMarkers = new List<EnemyMarker>();
        private Transform m_enemyMarkerHolder;
        private List<EnemyMarkerDisplay> m_enemyTexts = new List<EnemyMarkerDisplay>();

        // bag markers
        private List<MapWorldMarker> m_bagMarkers = new List<MapWorldMarker>();

        //// custom icon markers
        //private Transform m_iconHolder;
        //private List<MapMarkerIconDisplay> m_unmarkedDungeons = new List<MapMarkerIconDisplay>();

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            SetupConfig();
            config.Register();

            StartCoroutine(SetupCoroutine());
        }

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

                Instance.m_mapID = (int)At.GetValue(typeof(MapDisplay), self, "m_currentMapSceneID");

                if (!(bool)At.GetValue(typeof(MapDisplay), self, "m_currentAreaHasMap"))
                {
                    return;
                }

                //Debug.LogWarning("Current map ID: " + Instance.m_mapID);

                if (MapConfigs.ContainsKey(Instance.m_mapID))
                {
                    self.CurrentMapScene.MarkerOffset = MapConfigs[Instance.m_mapID].MarkerOffset;
                    self.CurrentMapScene.Rotation = MapConfigs[Instance.m_mapID].Rotation;
                    self.CurrentMapScene.MarkerScale = MapConfigs[Instance.m_mapID].MarkerScale;
                }

                var list = CharacterManager.Instance.Characters.Values
                    .Where(x =>
                        !x.GetComponentInChildren<MapWorldMarker>()
                        && !x.IsDead
                        && x.gameObject.activeSelf);

                foreach (Character c in list)
                {
                    // player markers
                    if ((bool)config.GetValue("ShowPlayerMarkers") && !c.IsAI)
                    {
                        Instance.AddWorldMarker(c.gameObject, c.Name);
                    }
                    // enemy markers
                    if ((bool)config.GetValue("ShowEnemyMarkers") && c.IsAI)
                    {
                        Instance.AddEnemyWorldMarker(c.gameObject, c.Name);
                    }
                }
                // caravanner
                if ((bool)config.GetValue("ShowSoroboreanCaravanner"))
                {
                    var caravanner = GameObject.Find("HumanSNPC_CaravanTrader");

                    if (!caravanner)
                    {
                        caravanner = GameObject.Find("UNPC_CaravanTraderA");
                    }

                    if (caravanner && !caravanner.GetComponentInChildren<MapWorldMarker>())
                    {
                        Instance.AddWorldMarker(caravanner, "Soroborean Caravanner");
                    }
                }
                // player bags
                if ((bool)config.GetValue("ShowPlayerBagMarkers"))
                {
                    foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                    {
                        var c = ps.ControlledCharacter;
                        if (c.Inventory.Equipment.LastOwnedBag != null && c.Inventory.Equipment.LastOwnedBag.OwnerCharacter == null)
                        {
                            var tempObject = new GameObject("TempBagHolder");
                            tempObject.transform.position = c.Inventory.Equipment.LastOwnedBag.transform.position;
                            var marker = Instance.AddWorldMarker(tempObject, c.Name + "'s Bag");
                            Instance.m_bagMarkers.Add(marker);
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
                    float zoomLevelSmooth = (float)At.GetValue(typeof(MapDisplay), MapDisplay.Instance, "m_zoomLevelSmooth");
                    for (int i = 0; i < Instance.EnemyMarkers.Count; i++)
                    {
                        Instance.EnemyMarkers[i].CalculateMapPosition(MapDisplay.Instance.CurrentMapScene, i, zoomLevelSmooth * 1.0351562f);
                        At.SetValue(Instance.EnemyMarkers[i].MapPosition, typeof(EnemyMarker), Instance.EnemyMarkers[i], "m_adjustedMapPosition");
                    }
                }

                // update enemy marker texts
                for (int i = 0; i < Instance.m_enemyTexts.Count; i++)
                {
                    if (i < Instance.EnemyMarkers.Count)
                    {
                        if (!Instance.m_enemyTexts[i].gameObject.activeSelf)
                        {
                            Instance.m_enemyTexts[i].SetActive(true);
                        }
                        Instance.m_enemyTexts[i].UpdateDisplay(Instance.EnemyMarkers[i]);
                    }
                    else
                    {
                        if (Instance.m_enemyTexts[i].gameObject.activeSelf)
                        {
                            Instance.m_enemyTexts[i].SetActive(false);
                        }
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
                if (Instance.m_bagMarkers.Count > 0)
                {
                    for (int i = 0; i < Instance.m_bagMarkers.Count; i++)
                    {
                        if (Instance.m_bagMarkers[i] != null)
                        {
                            Destroy(Instance.m_bagMarkers[i].gameObject);
                            Instance.m_bagMarkers.RemoveAt(i);
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
                    if (Instance.EnemyMarkers.Contains(enemymarker))
                    {
                        Instance.EnemyMarkers.Remove(enemymarker);
                    }
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
            var markerTexts = At.GetValue(typeof(MapDisplay), MapDisplay.Instance, "m_markerTexts") as MapWorldMarkerDisplay[];
            var mapMarkers = At.GetValue(typeof(MapDisplay), MapDisplay.Instance, "m_mapWorldMarkers") as List<MapWorldMarker>;
            if (markerTexts.Length < mapMarkers.Count)
            {
                AddTextHolder(markerTexts);
            }

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
            At.SetValue(list.ToArray(), typeof(MapDisplay), MapDisplay.Instance, "m_markerTexts");
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
            {
                AddEnemyTextHolder();
            }

            return marker;
        }

        /*
         * AddEnemyTextHolder
         * Same as AddTextHolder, but using our custom m_enemyTexts list, attached to our custom m_customMarkerHolder.
         * For the first map, we will do this for every active enemy, since our list starts our with 0 holders.
        */
        private void AddEnemyTextHolder()
        {
            // get any existing one to clone from
            var origTextHolder = MapDisplay.Instance.WorldMapMarkers.GetComponentInChildren<MapWorldMarkerDisplay>();

            // copy the orig as a custom Marker Class
            var tempMarker = Instantiate(origTextHolder.gameObject).GetComponent<MapWorldMarkerDisplay>();
            var newMarker = tempMarker.gameObject.AddComponent<EnemyMarkerDisplay>();
            At.InheritBaseValues(newMarker as MapWorldMarkerDisplay, tempMarker);
            Destroy(tempMarker);
            //newMarker.transform.parent = m_enemyMarkerHolder;
            newMarker.transform.SetParent(m_enemyMarkerHolder, false);
            newMarker.transform.localScale = Vector3.one;
            newMarker.Circle.enabled = false;

            m_enemyTexts.Add(newMarker);
        }

        // =====================  CONFIG  ===================== //

        private void SetupConfig()
        {
            config = new ModConfig
            {
                ModName = "More Map Details",
                SettingsVersion = 1.0,
                Settings = new List<BBSetting>()
                {
                    new BoolSetting
                    {
                        Name = "ShowPlayerMarkers",
                        Description = "Show map markers for Players",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = "ShowPlayerBagMarkers",
                        Description = "Show map markers for Player Bags (when unequipped)",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = "ShowEnemyMarkers",
                        Description = "Show map markers for Enemies (will only show an 'x' until you hover over them)",
                        DefaultValue = true
                    },
                    new BoolSetting
                    {
                        Name = "ShowSoroboreanCaravanner",
                        Description = "Show map markers for the Soroborean Caravanner",
                        DefaultValue = true
                    },
                }
            };
        }
        

        // --- Map Config dictionary ---
        // Key: MapID (as per MapDisplay class)
        // Value: MapDependingScene settings. Only using the offset / rotation / scale values.

        public static Dictionary<int, MapConfig> MapConfigs = new Dictionary<int, MapConfig>
        {
            {
                1, // Chersonese
                new MapConfig()
                {
                    MarkerOffset = new Vector2(-531f, -543f),
                    MarkerScale = new Vector2(0.526f, 0.526f),
                    Rotation = 0f
                }
            },
            {
                3, // Hallowed Marsh
                new MapConfig()
                {
                    MarkerOffset = new Vector2(-573.0f, -515.0f),
                    MarkerScale = new Vector2(0.553f, 0.553f),
                    Rotation = 90f
                }
            },
            {
                5, // Abrassar
                new MapConfig()
                {
                    MarkerOffset = new Vector2(3f, -5f),
                    MarkerScale = new Vector2(0.534f, 0.534f),
                    Rotation = -90f
                }
            },
            {
                7, // Enmerkar Forest
                new MapConfig()
                {
                    MarkerOffset = new Vector2(-500f, -500f),
                    MarkerScale = new Vector2(0.5f, 0.5f),
                    Rotation = 0f
                }
            },
            {
                9, // Antique Plateau
                new MapConfig
                {
                    MarkerOffset = new Vector2(-504f, -505f),
                    MarkerScale = new Vector2(0.50f, 0.50f),
                    Rotation = 0f
                }
            },
            {
                10, // Caldera
                new MapConfig
                {
                    //MarkerOffset = new Vector2(-502.0f, -497.0f),
                    //MarkerScale = new Vector2(0.471f, 0.471f),
                    MarkerOffset = new Vector2(-504.0f, -500.0f), 
                    MarkerScale = new Vector2(0.503f, 0.503f),
                    Rotation = -90f
                }
            },
        };



        /*
         * TEMP DEBUG
         * I used this to align the map offsets for the exterior regions more accurately. 
         * PgDown (-) and PgDown (+) adjust the scale.
         * Arrow keys adjust the X/Y offset.
         * It will print the value (after changes) with Debug.Log()
        */

        internal void Update()
        {
            // adjust scale
            if (Input.GetKey(KeyCode.PageUp))
            {
                AdjustConfig(Vector2.zero, Vector2.one * -0.001f);
            }
            if (Input.GetKey(KeyCode.PageDown))
            {
                AdjustConfig(Vector2.zero, Vector2.one * 0.001f);
            }

            // adjust offsets
            if (Input.GetKey(KeyCode.DownArrow))
            {
                AdjustConfig(new Vector2(0, -1), Vector2.zero);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                AdjustConfig(new Vector2(0, 1), Vector2.zero);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                AdjustConfig(new Vector2(1, 0), Vector2.zero);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                AdjustConfig(new Vector2(-1, 0), Vector2.zero);
            }
        }

        private void AdjustConfig(Vector2 _offset, Vector2 scale)
        {
            MapDisplay.Instance.CurrentMapScene.MarkerOffset += _offset;
            MapDisplay.Instance.CurrentMapScene.MarkerScale += scale;
            MapConfigs[m_mapID].MarkerOffset = MapDisplay.Instance.CurrentMapScene.MarkerOffset;
            MapConfigs[m_mapID].MarkerScale = MapDisplay.Instance.CurrentMapScene.MarkerScale;
            Debug.Log("Offset: " + MapDisplay.Instance.CurrentMapScene.MarkerOffset + ", Scale: " + MapDisplay.Instance.CurrentMapScene.MarkerScale.ToString("0.000"));
        }
    }

    public class MapConfig
    {
        public Vector2 MarkerOffset;
        public Vector2 MarkerScale;
        public float Rotation;
    }
}