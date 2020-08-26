using Rewired;
using SideLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SharedModConfig
{
    /// <summary>
    /// The internal manager for the UI display.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;

        private readonly string MenuKey = "Shared Mod Config Menu";

        private readonly int[] MenuKeyIds = new int[2];

        public delegate void MenuLoaded();
        public static event MenuLoaded OnMenuLoaded;

        // UI canvas
        private GameObject m_ConfigCanvas;

        private Button m_MenuButton;
        private GameObject m_ConfigPanel;
        private bool m_lastCanvasState;

        private Button m_closeButton;

        private GameObject m_modListHolder;
        private GameObject m_ModListButtonPrefab;

        private GameObject m_SettingsHolder;
        private GameObject m_currentActiveSettings;
        private ModConfig m_currentModConfig;
        private GameObject m_SettingsPrefab;

        public bool InitDone { get; private set; } = false;

        internal void Update()
        {
            if (!Instance.InitDone)
            {
                return;
            }

            // show/hide main menu button
            if (global::MenuManager.Instance.IsInMainMenuScene)
            {
                if (!m_MenuButton.gameObject.activeSelf)
                {
                    m_MenuButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (m_MenuButton.gameObject.activeSelf)
                {
                    m_MenuButton.gameObject.SetActive(false);
                }
            }

            // keybind check
            if (Global.Lobby.PlayersInLobbyCount > 0
                && !NetworkLevelLoader.Instance.IsGameplayLoading
                && !NetworkLevelLoader.Instance.IsGameplayPaused
                && !Global.GamePaused)
            {
                foreach (SplitPlayer player in SplitScreenManager.Instance.LocalPlayers)
                {
                    if (CustomKeybindings.m_playerInputManager[player.RewiredID].GetLastUsedControllerFirstElementMapWithAction(MenuKey).aem == null)
                    {
                        continue;
                    }

                    UpdateMenuKey(player.RewiredID);

                    if (ReInput.players.GetPlayer(player.RewiredID).controllers.GetController(
                        CustomKeybindings.m_playerInputManager[player.RewiredID].GetLastUsedControllerFirstElementMapWithAction(MenuKey).aem.controllerMap.controllerType,
                        CustomKeybindings.m_playerInputManager[player.RewiredID].GetLastUsedControllerFirstElementMapWithAction(MenuKey).aem.controllerMap.controllerId).
                        GetButtonDownById(MenuKeyIds[player.RewiredID]))
                    {
                        ToggleMenu();
                    }
                }

                // fix for displaying mouse when menu opens
                MenuMouseFix();
            }



            // Update current config's displayed values (currently only really used for Float slider value)
            if (m_currentModConfig != null && m_currentModConfig.m_linkedPanel.activeSelf)
            {
                foreach (var setting in m_currentModConfig.Settings)
                {
                    setting.UpdateValue(true); // true = no save
                }
            }
        }

        public void ToggleMenu()
        {
            bool active = !m_ConfigPanel.activeSelf;
            m_ConfigPanel.SetActive(active);

            if (active)
            {
                // Invoke the OnSettingsOpened callback for the current config
                if (m_currentModConfig != null)
                {
                    m_currentModConfig.INTERNAL_OnSettingsOpened();
                }
            }
        }

        internal void Awake()
        {
            Instance = this;

            SL.OnPacksLoaded += Setup;

            CustomKeybindings.AddAction(MenuKey, CustomKeybindings.KeybindingsCategory.Menus, CustomKeybindings.ControlType.Both, 5, CustomKeybindings.InputActionType.Button);
        }

        private void Setup()
        {
            SetupCanvas();

            InitDone = true;

            OnMenuLoaded?.Invoke();
        }

        private void UpdateMenuKey(int id)
        {
            if (MenuKeyIds[id] != CustomKeybindings.m_playerInputManager[id].GetLastUsedControllerFirstElementMapWithAction(MenuKey).aem.elementIdentifierId)
            {
                MenuKeyIds[id] = CustomKeybindings.m_playerInputManager[id].GetLastUsedControllerFirstElementMapWithAction(MenuKey).aem.elementIdentifierId;
            }
        }

        private void SetupCanvas()
        {
            // Debug.Log(ModBase.ModName + " started, version: " + ModBase.ModVersion);

            var pack = SL.Packs["SharedModConfig"];

            if (pack == null)
            {
                Debug.LogError("ERROR: Could not find pack 'SharedModConfig'! Please make sure it exists at Mods/SideLoader/SharedModConfig!");
            }

            var bundle = pack.AssetBundles["sharedmodconfig"];

            if (bundle.LoadAsset("SharedModConfigCanvas") is GameObject canvasAsset)
            {
                m_ConfigCanvas = Instantiate(canvasAsset);
                DontDestroyOnLoad(m_ConfigCanvas);
                m_ConfigCanvas.SetActive(true);

                m_MenuButton = m_ConfigCanvas.transform.Find("MenuButton").GetComponent<Button>();
                m_MenuButton.gameObject.SetActive(true);
                m_MenuButton.onClick.AddListener(MenuButton);

                m_ConfigPanel = m_ConfigCanvas.transform.Find("Panel").gameObject;

                var headerHolder = m_ConfigPanel.transform.Find("Header_Holder");

                // close button (X)
                m_closeButton = headerHolder.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                m_closeButton.onClick.AddListener(CloseButton);

                // mod list and buttons
                m_modListHolder = headerHolder.Find("ModList_Holder").Find("ModList_Viewport").Find("ModList_Content").gameObject;
                m_ModListButtonPrefab = m_modListHolder.transform.Find("Button_Holder").gameObject;
                var modlistButton = m_ModListButtonPrefab.GetComponent<Button>();
                modlistButton.onClick.AddListener(ModListButton);
                m_ModListButtonPrefab.SetActive(false);

                // mod settings template
                m_SettingsHolder = m_ConfigPanel.transform.Find("Settings_Holder").gameObject;

                m_SettingsPrefab = m_SettingsHolder.transform.Find("Settings_Prefab").gameObject;
                m_SettingsPrefab.SetActive(false);

                // disable main panel
                m_ConfigPanel.SetActive(false);
            }
        }

        // config menu button callbacks

        private void CloseButton()
        {
            m_ConfigPanel.SetActive(false);
        }

        private void MenuButton()
        {
            bool active = m_ConfigPanel.activeSelf;
            m_ConfigPanel.SetActive(!active);
        }

        private void ModListButton()
        {
            var buttontext = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;
            //Debug.Log(buttontext);
            if (m_SettingsPrefab.transform.parent.Find(buttontext) is Transform t)
            {
                if (m_currentActiveSettings)
                {
                    m_currentActiveSettings.SetActive(false);
                }

                t.gameObject.SetActive(true);
                m_currentActiveSettings = t.gameObject;

                if (ConfigManager.RegisteredConfigs.ContainsKey(buttontext))
                {
                    var cfg = ConfigManager.RegisteredConfigs[buttontext];
                    m_currentModConfig = cfg;
                    cfg.INTERNAL_OnSettingsOpened();
                }
            }
            else
            {
                Debug.LogError("[SharedModConfig] Error! Could not find transform " + buttontext);
            }
        }

        // Adding a new mod config to the menu

        public void AddConfig(ModConfig config)
        {
            // add button to mod list
            var newButton = Instantiate(m_ModListButtonPrefab);
            newButton.name = config.ModName;
            newButton.transform.SetParent(m_modListHolder.transform, false);

            var text = newButton.GetComponentInChildren<Text>();
            text.text = config.ModName;

            var button = newButton.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ModListButton);

            newButton.SetActive(true);

            // Alphabetize the modlist
            List<Transform> children = new List<Transform>();
            for (int i = m_modListHolder.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = m_modListHolder.transform.GetChild(i);
                children.Add(child);
                child.transform.SetParent(null, false);
            }
            children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
            foreach (Transform child in children)
            {
                child.transform.SetParent(m_modListHolder.transform, false);
            }

            // setup canvas
            var newCanvas = Instantiate(m_SettingsPrefab);
            newCanvas.name = config.ModName;
            newCanvas.transform.SetParent(m_SettingsHolder.transform, false);

            config.m_linkedPanel = newCanvas;

            // set Title
            var title = newCanvas.transform.Find("Title").GetComponent<Text>();
            title.text = config.ModName;

            // setup Save button and add callback listener
            var saveButton = newCanvas.transform.Find("SaveButton").GetComponent<Button>();
            saveButton.onClick.AddListener(new UnityEngine.Events.UnityAction(config.INTERNAL_OnSettingsSaved));

            // setup the settings content holder
            var contentHolder = newCanvas.transform.Find("SettingsScroll_Holder").Find("Settings_Viewport").Find("Settings_Content");

            // get the setting template prefabs
            var boolPrefab = contentHolder.Find("Toggle_Holder").gameObject;
            boolPrefab.transform.SetParent(null, false);
            var stringPrefab = contentHolder.Find("InputField_Holder").gameObject;
            stringPrefab.transform.SetParent(null, false);
            var floatPrefab = contentHolder.Find("HSlider_Holder").gameObject;
            floatPrefab.transform.SetParent(null, false);
            var titlePrefab = contentHolder.Find("Title_Holder").gameObject;
            titlePrefab.transform.SetParent(null, false);

            // setup actual settings
            foreach (var setting in config.Settings)
            {
                switch (setting.GetType().Name)
                {
                    case "BoolSetting":
                    case "SharedModConfig.BoolSetting":
                        SetupBoolSetting(contentHolder, boolPrefab, setting as BoolSetting, titlePrefab);
                        break;
                    case "StringSetting":
                    case "SharedModConfig.StringSetting":
                        SetupStringSetting(contentHolder, stringPrefab, setting as StringSetting, titlePrefab);
                        break;
                    case "FloatSetting":
                    case "SharedModConfig.FloatSetting":
                        SetupFloatSetting(contentHolder, floatPrefab, setting as FloatSetting, titlePrefab);
                        break;
                    default:
                        Debug.LogError("[SharedModConfig] Could not parse BBSetting of type " + setting.GetType().Name);
                        break;
                }
            }

            // cleanup the prefab templates
            Destroy(boolPrefab);
            Destroy(stringPrefab);
            Destroy(floatPrefab);
            Destroy(titlePrefab);
        }

        private GameObject SetupBasicSetting(Transform contentHolder, GameObject prefab, BBSetting setting, GameObject titlePrefab)
        {
            if (!string.IsNullOrEmpty(setting.SectionTitle))
            {
                var newTitle = Instantiate(titlePrefab).gameObject;
                newTitle.transform.SetParent(contentHolder.transform, false);
                var text = newTitle.GetComponent<Text>();
                text.text = setting.SectionTitle;
                newTitle.SetActive(true);
            }

            var newPrefab = Instantiate(prefab);
            newPrefab.transform.SetParent(contentHolder, false);
            setting.LinkedGameObject = newPrefab;
            newPrefab.SetActive(true);

            return newPrefab;
        }

        private void SetupBoolSetting(Transform contentHolder, GameObject prefab, BoolSetting setting, GameObject titlePrefab)
        {
            var newPrefab = SetupBasicSetting(contentHolder, prefab, setting, titlePrefab);

            var Toggle = newPrefab.GetComponentInChildren<Toggle>();
            var Label = newPrefab.GetComponentInChildren<Text>();

            Toggle.isOn = setting.m_value;
            Label.text = string.IsNullOrEmpty(setting.Description) ? setting.Name : setting.Description;
        }

        private void SetupFloatSetting(Transform contentHolder, GameObject prefab, FloatSetting setting, GameObject titlePrefab)
        {
            var newPrefab = SetupBasicSetting(contentHolder, prefab, setting, titlePrefab);

            var label = newPrefab.transform.Find("Label").GetComponent<Text>();
            label.text = string.IsNullOrEmpty(setting.Description) ? setting.Name : setting.Description;

            var slider = newPrefab.GetComponentInChildren<Slider>();

            slider.value = setting.m_value;
            if (setting.Increment > 0)
            {
                slider.value = setting.Increment * Mathf.Floor(slider.value / setting.Increment);
                // formattedValue = Increment * Mathf.Floor(formattedValue / Increment);
            }

            slider.minValue = setting.MinValue;
            slider.maxValue = setting.MaxValue;

            if (setting.RoundTo >= 0)
            {
                slider.value = (float)Math.Round(setting.m_value, setting.RoundTo);
            }

            var textvalue = slider.transform.parent.Find("SliderValue").GetComponent<Text>();
            string s = setting.m_value + (setting.ShowPercent ? "%" : "");
            textvalue.text = s;
        }

        private void SetupStringSetting(Transform contentHolder, GameObject prefab, StringSetting setting, GameObject titlePrefab)
        {
            var newPrefab = SetupBasicSetting(contentHolder, prefab, setting, titlePrefab);

            var Label = newPrefab.transform.Find("Label").GetComponent<Text>();
            Label.text = string.IsNullOrEmpty(setting.Description) ? setting.Name : setting.Description;

            var inputField = newPrefab.GetComponentInChildren<InputField>();
            inputField.text = setting.m_value;
        }

        // mouse control fix

        private void MenuMouseFix()
        {
            if (m_lastCanvasState != m_ConfigPanel.activeSelf)
            {
                m_lastCanvasState = m_ConfigPanel.activeSelf;

                Character c = CharacterManager.Instance.GetFirstLocalCharacter();

                if (c.CharacterUI.PendingDemoCharSelectionScreen is Panel panel)
                {
                    if (m_lastCanvasState)
                        panel.Show();
                    else
                        panel.Hide();
                }
                else if (m_lastCanvasState)
                {
                    GameObject obj = new GameObject();
                    obj.transform.parent = c.transform;
                    obj.SetActive(true);

                    Panel newPanel = obj.AddComponent<Panel>();
                    At.SetValue(newPanel, typeof(CharacterUI), c.CharacterUI, "PendingDemoCharSelectionScreen");
                    newPanel.Show();
                }
            }
        }
    }
}
