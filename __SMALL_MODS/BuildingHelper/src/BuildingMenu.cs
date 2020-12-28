using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SideLoader;
using SideLoader.Helpers;
using UnityEngine;

namespace BuildingHelper
{
    public class BuildingMenu
    {
        private static bool m_show;
        public static bool Show
        {
            get => m_show;
            set
            {
                m_show = value;

                if (m_show)
                    ForceUnlockCursor.AddUnlockSource();
                else
                    ForceUnlockCursor.RemoveUnlockSource();
            }
        }

        private static Rect s_windowRect = new Rect(25f, 25f, 420f, 500f);
        private static Vector2 s_scroll = Vector2.zero;

        public enum Pages
        {
            Builder,
            Destroyer
        }

        private static Pages s_currentPage;

        private static Blueprint[] s_allBuildings;

        private static Blueprint s_selectedBuildingToPlace;
        private static Building s_selectedBuildingToDestroy;
        private static Building[] s_currentSceneBuildings = new Building[0];

        public static void Init()
        {
            s_allBuildings = References.RPM_ITEM_PREFABS.Values
                                    .Where(it => it is Blueprint)
                                    .Select(it => it as Blueprint)
                                    .ToArray();
        }

        public static void OnSceneChange()
        {
            s_currentSceneBuildings = new Building[0];
        }

        internal static readonly int WINDOW_ID = BuildingHelperMod.Instance.GetHashCode();

        public static void OnGUI()
        {
            if (!Show)
                return;

            var orig = GUI.skin;
            GUI.skin = UIStyles.WindowSkin;
            s_windowRect = GUI.Window(WINDOW_ID, s_windowRect, WindowFunction, "Building Helper Menu");
            GUI.skin = orig;
        }

        internal static void WindowFunction(int id)
        {
            GUI.DragWindow(new Rect(0, 0, s_windowRect.width - 35, 23));
            if (GUI.Button(new Rect(s_windowRect.width - 30, 2, 30, 20), "X"))
            {
                Show = false;
                return;
            }

            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                GUILayout.Label("You must be the host.");
                return;
            }

            GUILayout.BeginHorizontal();
            PageButton(Pages.Builder);
            PageButton(Pages.Destroyer);
            GUILayout.EndHorizontal();
            GUI.color = Color.white;

            switch (s_currentPage)
            {
                case Pages.Builder:
                    BuilderPage(); break;
                case Pages.Destroyer:
                    DestroyerPage(); break;
            }
        }

        private static void PageButton(Pages page)
        {
            GUI.color = s_currentPage == page
                        ? Color.green
                        : Color.white;

            if (GUILayout.Button(page.ToString()))
                s_currentPage = page;
        }

        private static void BuilderPage()
        {
            BuildingHelperMod.Instance.settings.AutoFinishBuildings 
                = GUILayout.Toggle(BuildingHelperMod.Instance.settings.AutoFinishBuildings, "Instant Build times");
            BuildingHelperMod.Instance.settings.ForceNoRequirements 
                = GUILayout.Toggle(BuildingHelperMod.Instance.settings.ForceNoRequirements, "No requirements to build");

            if (s_selectedBuildingToPlace)
            {
                if (GUILayout.Button("< Back"))
                {
                    s_selectedBuildingToPlace = null;
                    return;
                }

                GUILayout.Label($"Selected Building: " + s_selectedBuildingToPlace.DisplayName);

                if (GUILayout.Button("Start Deploying"))
                {
                    // give player blueprint if they dont have it
                    var player = CharacterManager.Instance.GetFirstLocalCharacter();
                    if (player)
                    {
                        Item item;
                        if (!player.Inventory.OwnsItem(s_selectedBuildingToPlace.ItemID))
                        {
                            item = ItemManager.Instance.GenerateItemNetwork(s_selectedBuildingToPlace.ItemID);
                            item.ChangeParent(player.Inventory.Pouch.transform);
                        }
                        else
                            item = player.Inventory.Pouch.GetItemFromID(s_selectedBuildingToPlace.ItemID);
                        
                        // start deploy (harmony patches handle the rest)
                        if (item)
                        {
                            Show = false;
                            At.Invoke(item, "Start");
                            At.Invoke(item as Blueprint, "Use", player);
                        }

                        s_selectedBuildingToPlace = null;
                    }
                }
            }
            else
            {
                s_scroll = GUILayout.BeginScrollView(s_scroll);

                foreach (var building in s_allBuildings)
                {
                    if (GUILayout.Button(building.DisplayName))
                    {
                        s_selectedBuildingToPlace = building;

                        break;
                    }
                }

                GUILayout.EndScrollView();
            }
        }

        private static void DestroyerPage()
        {
            if (s_selectedBuildingToDestroy)
            {
                if (GUILayout.Button("< Back"))
                {
                    s_selectedBuildingToDestroy = null;
                    return;
                }

                GUILayout.Label($"Are you sure you want to destroy {s_selectedBuildingToDestroy.Name}?");

                if (GUILayout.Button("Yes, destroy it"))
                {
                    BuildingResourcesManager.Instance.UnregisterBuiding(s_selectedBuildingToDestroy);
                    ItemManager.Instance.SendDestroyItem(s_selectedBuildingToDestroy.UID);
                    GameObject.Destroy(s_selectedBuildingToDestroy.gameObject);
                }
            }
            else
            {
                if (GUILayout.Button("Refresh current buildings"))
                    RefreshSceneBuildingCache();

                if (s_currentSceneBuildings == null || s_currentSceneBuildings.Length < 1)
                {
                    GUILayout.Label("<i>No buildings or cache not refreshed...</i>");
                    return;
                }

                GUI.color = Color.red;
                for (int i = 0; i < s_currentSceneBuildings.Length; i++)
                {
                    var building = s_currentSceneBuildings[i];
                    if (!building)
                    {
                        RefreshSceneBuildingCache();
                        break;
                    }

                    if (GUILayout.Button($"<color=white>{building.DisplayName}</color>"))
                        s_selectedBuildingToDestroy = building;
                }
                GUI.color = Color.white;
            }
        }

        private static void RefreshSceneBuildingCache()
        {
            var scene = SceneManagerHelper.ActiveSceneName;
            s_currentSceneBuildings = Resources.FindObjectsOfTypeAll<Building>()
                                                .Where(it => it.gameObject.scene.name == scene)
                                                .ToArray();
        }
    }

    public class UIStyles
    {
        public static GUISkin WindowSkin
        {
            get
            {
                if (_customSkin == null)
                {
                    try
                    {
                        _customSkin = CreateWindowSkin();
                    }
                    catch
                    {
                        _customSkin = GUI.skin;
                    }
                }

                return _customSkin;
            }
        }

        public static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, HorizontalBar);
            GUI.color = c;
        }

        private static GUISkin _customSkin;

        public static Texture2D m_nofocusTex;
        public static Texture2D m_focusTex;

        private static GUIStyle _horizBarStyle;

        private static GUIStyle HorizontalBar
        {
            get
            {
                if (_horizBarStyle == null)
                {
                    _horizBarStyle = new GUIStyle();
                    _horizBarStyle.normal.background = Texture2D.whiteTexture;
                    _horizBarStyle.margin = new RectOffset(0, 0, 4, 4);
                    _horizBarStyle.fixedHeight = 2;
                }

                return _horizBarStyle;
            }
        }

        private static GUISkin CreateWindowSkin()
        {
            var newSkin = UnityEngine.Object.Instantiate(GUI.skin);
            UnityEngine.Object.DontDestroyOnLoad(newSkin);

            m_nofocusTex = MakeTex(550, 700, new Color(0.1f, 0.1f, 0.1f, 0.7f));
            m_focusTex = MakeTex(550, 700, new Color(0.3f, 0.3f, 0.3f, 1f));

            newSkin.window.normal.background = m_nofocusTex;
            newSkin.window.onNormal.background = m_focusTex;

            newSkin.box.normal.textColor = Color.white;
            newSkin.window.normal.textColor = Color.white;
            newSkin.button.normal.textColor = Color.white;
            newSkin.textField.normal.textColor = Color.white;
            newSkin.label.normal.textColor = Color.white;

            return newSkin;
        }

        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
