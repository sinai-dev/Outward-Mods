using BepInEx;
using BepInEx.Logging;
using MonoMod.RuntimeDetour; // for manual hooks
using Rewired;               // for InputManager_Base class
using Rewired.Data;          // for UserData class
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace CharacterEditor
{
    public class CustomKeybindings
    {
        // Recreate this enum from Rewired_Core.dll so that users of CustomKeybindings don't have to import Rewired
        public enum InputActionType
        {
            Axis = 0,
            Button = 1
        }

        // categoryIds--only 1, 2 & 4 are currently shown in-game. The others are unused or hidden
        //
        // 0 - ActionCategory_Default
        // 1 - Menus
        // 2 - Quick Slot
        // 3 - Camera
        // 4 - Actions
        // 5 - Section
        // 6 - ActionCategory_VirtualCursor
        // 7 - Section
        public enum KeybindingsCategory : int
        {
            Actions = 4,
            QuickSlot = 2,
            Menus = 1
        }

        // To choose whether your keybinding show up under keyboard & mouse, controller or both
        public enum ControlType : int
        {
            Keyboard,
            Gamepad,
            Both
        }

        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource("CustomKeyBindings");

        // Mods will need to query the RewiredInputs.GetButton... methods to detect when their keys are pressed
        // The player-specific RewiredInputs instances are in a private variable, and must be acquired by reflection
        // Since it will be accessed on every Update(), it is exposed and cached here for convenience & performance
        public static Dictionary<int, RewiredInputs> m_playerInputManager = (Dictionary<int, RewiredInputs>)typeof(ControlsInput).GetField("m_playerInputManager", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null); // null for static field

        //
        // USE THIS METHOD TO EASILY ADD ACTIONS AND KEYBINDINGS FOR YOUR MOD
        //
        // The name arg is displayed in the keybindings menu
        //
        // The categoryId arg determines where in the keybindings list this action will appear
        //
        // The showForController arg determines if this action can also be bound to a controller
        //
        // The sectionId arg is a weird number. It somehow affects where in the list of keybindings
        // the action appears but I haven't figured out yet what it means. I just try different numbers
        // from 0-5 until I'm happy with where in the list the keybind ends up
        //
        public static void AddAction(string name, KeybindingsCategory categoryId, ControlType controlType, int sectionId = 0, InputActionType type = InputActionType.Button)
        {

            // The user does not have to initialize or instantiate anything themselves to use custom keybinds
            // Just call AddAction() and we'll make sure the plumbing is there
            if (orig_Initialize == null)
            {
                InitHooks();
            }

            // Capture the info the user has specified for the action they would like to create
            InputActionDescription protoAction = new InputActionDescription(name, (int)categoryId, controlType, sectionId, type);
            myActionInfos.Add(protoAction);

            // If this method is called after InputManager_Base.Initialize() has been called, then these actions won't be added
            // So let's report that back to the mod maker to avoid headaches
            bool shouldReportTooLateAdd = false;
            if (shouldReportTooLateAdd)
            {
                bool alreadyInitialized = (bool)typeof(ReInput).GetProperty("initialized", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true).Invoke(null, null);
                if (alreadyInitialized)
                {
                    MyLogger.LogDebug("Tried to add action too late. Add your action earlier in the life cycle.");
                }
            }
        }

        //
        // In this class there are some techniques to simplify the creation of keybindngs.
        //
        // - We expose a single static method AddAction() that the user can call to specifiy actions they would like to create
        // - We create InputActionDescription structs from the info passed by the user--which are like temporary or prototype
        //   InputAction objects. These InputActionDescription structs will be used later to construct actual InputAction objects
        //   in the Initialize() hook
        // - This way the user don't have to worry about hooking
        // - Since the user don't directly create InputAction objects we also simplify the creation of InputActions by fabricating
        //   some boilerplate data for the user
        //

        // InputActionDescription structs are like temporary or prototype InputAction objects.
        // They will be used later to construct actual InputAction objects in the Initialize() hook
        public struct InputActionDescription
        {
            public string name;
            public int categoryId;
            public int sectionId;
            public ControlType controlType;
            public InputActionType type;
            public InputActionDescription(string name, int categoryId, ControlType controlType = ControlType.Keyboard, int sectionId = 0, InputActionType type = InputActionType.Button)
            {
                this.name = name;
                this.categoryId = categoryId;
                this.controlType = controlType;
                this.sectionId = sectionId;
                this.type = type;
            }
        }

        // A place to store the temporary/prototype InputActionDescription structs
        public static List<InputActionDescription> myActionInfos = new List<InputActionDescription>();

        // This dictionary holds the id's of the actual InputAction objects that we create
        // The action id keyes the user-created InputActionDescription structs that were made to create that action
        public static Dictionary<int, InputActionDescription> myCustomActionIds = new Dictionary<int, InputActionDescription>();

        // To add our own keybind actions natively we need to intercept a method in a Rewired dll.
        // Normally, we use the hooks that Partiality uses MonoMod to generate (called HookGen),
        // but Partiality only instructs MonoMod to hook methods in the Assembly-CSharp.dll file,
        // so we need to manually hook the Rewired method. We do this in exactly the same way that
        // Partiality uses MonoMod--with MonoMod.RuntimeDetour--but we do it manually.
        private static void InitHooks()
        {

            // Create the manual hook of the Initialize method in Rewired's InputManager_Base class
            orig_Initialize = (h_Initialize = new Detour(
                typeof(InputManager_Base).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomKeybindings).GetMethod("InputManager_Base_Initialize", BindingFlags.Static | BindingFlags.NonPublic)
                )).GenerateTrampoline<d_Initialize>();
        }

        // These are some things we need to store for our manual hook of the Initialize method on Rewired.InputManager_Base
        private static Detour h_Initialize;
        private delegate void d_Initialize(InputManager_Base self);
        private static d_Initialize orig_Initialize;

        // The famed Rewired hook
        private static void InputManager_Base_Initialize(InputManager_Base self)
        {

            // Add our custom actions, which will be exposed in the keybindings menu by tying an action to an "element"--i.e. a keyboard key or joystick input

            // Loop through the infos/descriptions that the user has specified for adding actions
            foreach (InputActionDescription myActionDescription in myActionInfos)
            {

                // This actually creates the new actions:
                // The AddRewiredAction method is just a wrapper I made for UserData.AddAction(_categoryId), since it's a little fiddly to configure an InputAction
                InputAction myNewlyAddedAction = AddRewiredAction(self.userData, myActionDescription.name, myActionDescription.type, "No descriptiveName", myActionDescription.categoryId, true); // true for userAssignable

                // We keep a list of our actions that we've added, so that it's easy to get them later
                // We also keep the InputActionDescription because we will use it later for sorting & localization
                myCustomActionIds[myNewlyAddedAction.id] = myActionDescription;
            }

            // And that's it. That's all we needed to hook Rewired for
            // Call original implementation to continue Rewired initialization
            orig_Initialize(self);
        }

        // The following is just a wrapper for UserData.AddAction(_categoryId) since it's a little fiddly to configure.
        private static InputAction AddRewiredAction(UserData userData, string name, InputActionType type, string descriptiveName, int categoryId, bool userAssignable)
        {

            // Add an action to the data store
            userData.AddAction(categoryId);

            // Get a reference to the added action
            int[] actionIds = userData.GetActionIds();
            int lastActionId = actionIds[actionIds.Length - 1];
            InputAction inputAction = userData.GetActionById(lastActionId);

            // Configure our action according to args

            if (string.IsNullOrEmpty(name) == false)
            {
                FieldInfo _name = inputAction.GetType().GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);
                _name.SetValue(inputAction, name);
            }
            if (type != InputActionType.Button)
            {
                FieldInfo _type = inputAction.GetType().GetField("_type", BindingFlags.NonPublic | BindingFlags.Instance);
                _type.SetValue(inputAction, type);
            }
            if (string.IsNullOrEmpty(descriptiveName) == false)
            {
                FieldInfo _descriptiveName = inputAction.GetType().GetField("_descriptiveName", BindingFlags.NonPublic | BindingFlags.Instance);
                _descriptiveName.SetValue(inputAction, descriptiveName);
            }
            if (!userAssignable)
            {
                FieldInfo _userAssignable = inputAction.GetType().GetField("_userAssignable", BindingFlags.NonPublic | BindingFlags.Instance);
                _userAssignable.SetValue(inputAction, userAssignable);
            }
            /*
            if (false) { // not used for simple keybinds
                FieldInfo _behaviorId = inputAction.GetType().GetField("_behaviorId", BindingFlags.NonPublic | BindingFlags.Instance);
                _behaviorId.SetValue(inputAction, _behaviorId);
            }
            */

            return inputAction;
        }
    }

    [HarmonyPatch(typeof(ControlMappingPanel), "InitSections")]
    public class ControlMappingPanel_InitSections
    {
        [HarmonyPrefix]
        public static bool InitSections(ControllerMap _controllerMap)
        {
            // Loop through our custom actions we added via Rewired
            foreach (int myActionId in CustomKeybindings.myCustomActionIds.Keys)
            {

                // The info that the user specified for this action
                CustomKeybindings.InputActionDescription myActionDescription = CustomKeybindings.myCustomActionIds[myActionId];

                // There are separate keybinding maps for keyboard, mouse, & controllers
                // We only add our action-to-element mappings to the keybind maps that make sense
                // For example, if you are adding a key that doesn't make sense to have on a controller,
                // then skip when _controllerMap is JoystickMap
                //
                // (Optional)
                // You can check if this method is being called for the Keyboard/Mouse bindings panel or
                // the Controller bindings panel, but I prefer to check the class of the _controllerMap
                //   if (self.ControllerType == ControlMappingPanel.ControlType.Keyboard) {
                //

                // If the controller map's control type does not match our action
                if (!(myActionDescription.controlType == CustomKeybindings.ControlType.Keyboard && (_controllerMap is KeyboardMap || _controllerMap is MouseMap) ||
                      myActionDescription.controlType == CustomKeybindings.ControlType.Gamepad && (_controllerMap is JoystickMap) ||
                      myActionDescription.controlType == CustomKeybindings.ControlType.Both))
                {
                    // Then skip to next action
                    continue;
                }

                // If the categoryId of this controller map does not match our action's
                if (_controllerMap.categoryId != myActionDescription.sectionId)
                {
                    // Skip to next action
                    continue;
                }

                // If we pass the tests, create & add the action-to-element map for this particular action
                _controllerMap.CreateElementMap(myActionId, Pole.Positive, KeyCode.None, ModifierKeyFlags.None);

                // Continue the loop...
            }

            // We're done here. Call original implementation
            return true;
        }
    }

    [HarmonyPatch(typeof(LocalizationManager), "StartLoading")]
    public class LocalizationManager_StartLoading
    {
        [HarmonyPostfix]
        public static void StartLoading(LocalizationManager __instance)
        {
            // Nab the localization dictionary that's used for keybind localization
            Dictionary<string, string> m_generalLocalization = (Dictionary<string, string>)__instance.GetType().GetField("m_generalLocalization", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            // Go through the added actions and use the user-created action descriptions to name them
            foreach (int myActionId in CustomKeybindings.myCustomActionIds.Keys)
            {
                InputAction myAction = ReInput.mapping.GetAction(myActionId);
                CustomKeybindings.InputActionDescription myActionDescription = CustomKeybindings.myCustomActionIds[myActionId];

                // The first string is the technical name of the action, while the second string is what you want to display in the bindings menu
                m_generalLocalization.Add("InputAction_" + myAction.name, myActionDescription.name);
            }
        }
    }
}
