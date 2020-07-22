using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace BetterTargeting
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BetterTargeting : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.bettertargeting";
        public const string NAME = "Better Targeting";
        public const string VERSION = "1.6.0";

        public static BetterTargeting Instance;

        private const string ToggleKey = "Toggle Target";

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony("com.sinai.bettertargeting");
            harmony.PatchAll();

            CustomKeybindings.AddAction(ToggleKey, CustomKeybindings.KeybindingsCategory.Actions, CustomKeybindings.ControlType.Both, 5, CustomKeybindings.InputActionType.Button);
        }

        internal void Update()
        {
            for (int i = 0; i < SplitScreenManager.Instance.LocalPlayers.Count; i++)
            {
                var character = SplitScreenManager.Instance.LocalPlayers[i].AssignedCharacter;

                if (character && CustomKeybindings.m_playerInputManager[i].GetButtonDown(ToggleKey))
                {
                    CustomToggleTarget(character);
                }
            }
        }

        private void CustomToggleTarget(Character character)
        {
            var localControl = character.CharacterControl as LocalCharacterControl;

            if (!character.CharacterCamera.InZoomMode && !character.TargetingSystem.LockedCharacter)
            {
                // If not locked, we will just get a target (same as vanilla method)

                At.Call(localControl, "AcquireTarget", new object[0]);

                if (character.TargetingSystem.Locked && localControl.ControlMode == LocalCharacterControl.CameraControlMode.Classic)
                {
                    localControl.FaceLikeCamera = true;
                }
            }
            else if (character.TargetingSystem.LockedCharacter)
            {
                // Otherwise we need to find a new target. This is similar to vanilla, but a bit different.

                if (character.TargetingSystem.CameraRef != null)
                {
                    Collider[] array = Physics.OverlapSphere(character.CenterPosition, character.TargetingSystem.TrueRange, Global.LockingPointsMask);

                    Matrix4x4 worldToCameraMatrix = character.TargetingSystem.CameraRef.worldToCameraMatrix;
                    var cam = character.TargetingSystem.CameraRef.transform.position;

                    LockingPoint lockingPoint = null;
                    float num = float.MaxValue;

                    // foreach collider that is not our current target
                    var currentCollider = character.TargetingSystem.LockingPoint.GetComponent<Collider>();
                    foreach (Collider collider in array.Where(x => x != currentCollider))
                    {
                        // this is my custom bit. Find the target with the smallest angle relative to our camera direction.
                        var angle = Vector2.Angle(cam, collider.transform.position);
                        if (angle < num || lockingPoint == null)
                        {
                            LockingPoint component = collider.GetComponent<LockingPoint>();
                            if (component.OwnerChar == null
                                || (character.TargetingSystem.IsTargetable(component.OwnerChar)
                                    && !Physics.Linecast(character.CenterPosition, collider.transform.position, Global.SightHideMask)))
                            {
                                lockingPoint = component;
                                num = angle;
                            }
                        }
                    }

                    // if we got a new target, set it to them now.
                    if (lockingPoint != null)
                    {
                        if (character.TargetingSystem.Locked && localControl.ControlMode == LocalCharacterControl.CameraControlMode.Classic)
                        {
                            localControl.FaceLikeCamera = true;
                        }

                        character.TargetingSystem.SetLockingPoint(lockingPoint);
                        character.TargetingSystem.LockingPointOffset = Vector2.zero;
                        character.CharacterCamera.LookAtTransform = character.TargetingSystem.LockingPointTrans;
                    }
                    else // otherwise we did not find a new target. Release the current target.
                    {
                        At.Call(character.CharacterControl as LocalCharacterControl, "ReleaseTarget", new object[0]);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LocalCharacterControl), "UpdateTargeting")]
        public class LocalCharacterControl_UpdateTargeting
        {
            [HarmonyPrefix]
            public static bool Prefix(LocalCharacterControl __instance)
            {
                var self = __instance;

                var m_character = At.GetValue(typeof(CharacterControl), self as CharacterControl, "m_character") as Character;
                var m_targetingSystem = m_character.TargetingSystem;

                bool m_lockHoldUp = false;

                if (!m_character.CharacterCamera.InZoomMode
                    && (ControlsInput.LockToggle(m_character.OwnerPlayerSys.PlayerID) || ControlsInput.LockHoldDown(m_character.OwnerPlayerSys.PlayerID)))
                {
                    At.SetValue(false, typeof(LocalCharacterControl), self, "m_lockHoldUp");

                    if (m_targetingSystem.Locked)
                    {
                        At.Call(self, "ReleaseTarget", new object[0]);

                        if (self.ControlMode == LocalCharacterControl.CameraControlMode.Classic)
                        {
                            self.FaceLikeCamera = false;
                        }
                    }
                    else
                    {
                        At.Call(self, "AcquireTarget", new object[0]);

                        if (m_targetingSystem.Locked && self.ControlMode == LocalCharacterControl.CameraControlMode.Classic)
                        {
                            self.FaceLikeCamera = true;
                        }
                    }
                }

                if (ControlsInput.LockHoldUp(m_character.OwnerPlayerSys.PlayerID))
                {
                    m_lockHoldUp = true;
                    At.SetValue(true, typeof(LocalCharacterControl), self, "m_lockHoldUp");
                }

                if (!m_character.CharacterCamera.InZoomMode && m_lockHoldUp)
                {
                    At.Call(self, "ReleaseTarget", new object[0]);
                }

                if (Input.GetMouseButtonDown(3) && self.TargetMode == LocalCharacterControl.TargetingMode.Aim)
                {
                    Ray ray = m_character.CharacterCamera.CameraScript.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit raycastHit, m_targetingSystem.TrueRange * 1.5f, Global.AimTargetMask))
                    {
                        LockingPoint lockingPoint = raycastHit.collider.GetComponent<LockingPoint>();
                        if (lockingPoint == null)
                        {
                            Character characterOwner = raycastHit.collider.GetCharacterOwner();
                            if (characterOwner)
                            {
                                lockingPoint = characterOwner.LockingPoint;
                            }
                        }
                        if (lockingPoint)
                        {
                            At.Call(self, "SwitchTarget", new object[] { lockingPoint });
                        }
                    }
                }

                if (m_targetingSystem.Locked && !m_character.CharacterCamera.InZoomMode)
                {
                    if (!self.FaceLikeCamera)
                    {
                        self.FaceLikeCamera = true;
                    }

                    if (self.TargetMode == LocalCharacterControl.TargetingMode.Classic)
                    {
                        Vector2 vector = new Vector2(
                            ControlsInput.SwitchTargetHorizontal(m_character.OwnerPlayerSys.PlayerID),
                            ControlsInput.SwitchTargetVertical(m_character.OwnerPlayerSys.PlayerID));

                        float magnitude = vector.magnitude;

                        float m_lastTargetSwitchTime = (float)At.GetValue(typeof(LocalCharacterControl), self, "m_lastTargetSwitchTime");

                        if (Time.time - m_lastTargetSwitchTime > 0.3f)
                        {
                            //Vector2 m_previousInput = (Vector2)At.GetValue(typeof(LocalCharacterControl), self, "m_previousInput");
                            //float magnitude2 = (vector - m_previousInput).magnitude;

                            //if (magnitude2 >= 0.45f && magnitude > 0.6f)
                            //{
                            //    At.Call(self, "SwitchTarget", new object[] { vector });
                            //}

                            // this is for bows
                            if (m_character.CurrentWeapon is ProjectileWeapon)
                            {
                                var m_timeOfLastAimOffset = (float)At.GetValue(typeof(LocalCharacterControl), self, "m_timeOfLastAimOffset");
                                var m_timeToNextAimOffset = (float)At.GetValue(typeof(LocalCharacterControl), self, "m_timeToNextAimOffset");
                                var m_aimOffsetRandom = (Vector2)At.GetValue(typeof(LocalCharacterControl), self, "m_aimOffsetRandom");

                                if (ControlsInput.IsLastActionGamepad(m_character.OwnerPlayerSys.PlayerID))
                                {
                                    Vector2 a = vector;
                                    a.x *= -1f;
                                    if (Time.time - m_timeOfLastAimOffset > m_timeToNextAimOffset)
                                    {
                                        m_aimOffsetRandom = UnityEngine.Random.insideUnitCircle;
                                        At.SetValue(m_aimOffsetRandom, typeof(LocalCharacterControl), self, "m_aimOffsetRandom");
                                        At.SetValue(Time.time, typeof(LocalCharacterControl), self, "m_timeOfLastAimOffset");
                                        At.SetValue(UnityEngine.Random.Range(0.1f, 0.3f), typeof(LocalCharacterControl), self, "m_timeToNextAimOffset");

                                    }

                                    a += m_aimOffsetRandom * ((Vector3)At.GetValue(typeof(LocalCharacterControl), self, "m_modifMoveInput")).magnitude * Time.deltaTime * 0.5f;

                                    m_character.TargetingSystem.LockingPointOffset = Vector2.Scale(a, new Vector2(-1f, 1f));
                                }
                                else
                                {
                                    Vector2 vector2 = vector * self.LockAimMouseSense;
                                    vector2.x *= -1f;
                                    if (Time.time - m_timeOfLastAimOffset > m_timeToNextAimOffset)
                                    {
                                        m_aimOffsetRandom = UnityEngine.Random.insideUnitCircle;
                                        At.SetValue(m_aimOffsetRandom, typeof(LocalCharacterControl), self, "m_aimOffsetRandom");
                                        At.SetValue(Time.time, typeof(LocalCharacterControl), self, "m_timeOfLastAimOffset");
                                        At.SetValue(UnityEngine.Random.Range(0.1f, 0.3f), typeof(LocalCharacterControl), self, "m_timeToNextAimOffset");
                                    }
                                    vector2 += m_aimOffsetRandom * ((Vector3)At.GetValue(typeof(LocalCharacterControl), self, "m_modifMoveInput")).magnitude * Time.deltaTime * 0.5f;
                                    m_character.TargetingSystem.LockingPointOffset -= new Vector3(vector2.x, vector2.y, 0);
                                    m_character.TargetingSystem.LockingPointOffset = Vector3.ClampMagnitude(m_character.TargetingSystem.LockingPointOffset, 1f);
                                }
                            }
                            At.SetValue(vector, typeof(LocalCharacterControl), self, "m_previousInput");
                        }
                        else if (ControlsInput.IsLastActionGamepad(m_character.OwnerPlayerSys.PlayerID) && magnitude == 0f)
                        {
                            At.SetValue(0f, typeof(LocalCharacterControl), self, "m_lastTargetSwitchTime");
                        }
                    }
                    else if (self.TargetMode == LocalCharacterControl.TargetingMode.Aim)
                    {
                        Global.LockCursor(false);
                    }

                    Vector3 lockedPointPos = m_targetingSystem.LockedPointPos;
                    float m_lastInSightTime = (float)At.GetValue(typeof(LocalCharacterControl), self, "m_lastInSightTime");
                    if (!Physics.Linecast(m_character.CenterPosition, lockedPointPos, Global.SightHideMask))
                    {
                        m_lastInSightTime = Time.time;
                        At.SetValue(m_lastInSightTime, typeof(LocalCharacterControl), self, "m_lastInSightTime");
                    }

                    bool isLocked = m_targetingSystem.LockedCharacter != null && !m_targetingSystem.LockedCharacter.Alive;
                    if (Vector3.Distance(lockedPointPos, m_character.CenterPosition) > m_targetingSystem.TrueRange + 2f || Time.time - m_lastInSightTime > 1f || isLocked)
                    {
                        At.Call(self, "ReleaseTarget", new object[0]);
                        self.Invoke("AcquireTarget", 0.5f);
                    }
                }
                else
                {
                    m_targetingSystem.LockingPointOffset = Vector3.zero;
                    if (m_character.CharacterCamera.InZoomMode)
                    {
                        float m_lastFreeAimUpdateTime = (float)At.GetValue(typeof(LocalCharacterControl), self, "m_lastFreeAimUpdateTime");
                        if (Time.time - m_lastFreeAimUpdateTime > 0.05f)
                        {
                            m_lastFreeAimUpdateTime = Time.time;
                            At.SetValue(m_lastFreeAimUpdateTime, typeof(LocalCharacterControl), self, "m_lastFreeAimUpdateTime");

                            bool m_debugFreeAim = (bool)At.GetValue(typeof(LocalCharacterControl), self, "m_debugFreeAim");

                            At.SetValue(
                                m_character.CharacterCamera.GetObstaclePos(new Vector3(0.5f, 0.5f, 0f), m_debugFreeAim),
                                typeof(LocalCharacterControl),
                                self,
                                "m_freeAimTargetPos");
                        }


                        var m_freeAimLockingPoint = At.GetValue(typeof(LocalCharacterControl), self, "m_freeAimLockingPoint") as LockingPoint;
                        var m_freeAimTargetPos = (Vector3)At.GetValue(typeof(LocalCharacterControl), self, "m_freeAimTargetPos");
                        if ((bool)At.GetValue(typeof(LocalCharacterControl), self, "m_wasFreeAiming"))
                        {
                            float num = (m_freeAimLockingPoint.transform.position - m_freeAimTargetPos).sqrMagnitude;
                            num = Mathf.Max(num, 10f);
                            m_freeAimLockingPoint.transform.position = Vector3.Lerp(m_freeAimLockingPoint.transform.position, m_freeAimTargetPos, num * Time.deltaTime);
                        }
                        else
                        {
                            m_freeAimLockingPoint.transform.position = m_freeAimTargetPos;
                        }
                    }
                }

                At.SetValue(m_character.CharacterCamera.InZoomMode, typeof(LocalCharacterControl), self, "m_wasFreeAiming");

                return false;
            }
        }
    }



    public static class At // Access Tools
    {
        public static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Default;

        //reflection call
        public static object Call(object obj, string method, params object[] args)
        {
            var methodInfo = obj.GetType().GetMethod(method, flags);
            if (methodInfo != null)
            {
                return methodInfo.Invoke(obj, args);
            }
            return null;
        }

        // set value
        public static void SetValue<T>(T value, Type type, object obj, string field)
        {
            FieldInfo fieldInfo = type.GetField(field, flags);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }

        // get value
        public static object GetValue(Type type, object obj, string value)
        {
            FieldInfo fieldInfo = type.GetField(value, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            else
            {
                return null;
            }
        }

        // inherit base values
        public static void InheritBaseValues(object _derived, object _base)
        {
            foreach (FieldInfo fi in _base.GetType().GetFields(flags))
            {
                try { _derived.GetType().GetField(fi.Name).SetValue(_derived, fi.GetValue(_base)); } catch { }
            }

            return;
        }
    }

}
