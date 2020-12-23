using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using SideLoader;

namespace BetterTargeting
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BetterTargeting : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.bettertargeting";
        public const string NAME = "Better Targeting";
        public const string VERSION = "1.6.0";

        public static BetterTargeting Instance;

        private const string TOGGLE_KEY = "Toggle Target";

        internal void Awake()
        {
            Instance = this;

            var harmony = new Harmony("com.sinai.bettertargeting");
            harmony.PatchAll();

            CustomKeybindings.AddAction(TOGGLE_KEY, KeybindingsCategory.CustomKeybindings, ControlType.Both);
        }

        internal void Update()
        {
            if (CustomKeybindings.GetKeyDown(TOGGLE_KEY, out int playerID))
            {
                var player = SplitScreenManager.Instance.LocalPlayers[playerID].AssignedCharacter;
                CustomToggleTarget(player);
            }
        }

        private void CustomToggleTarget(Character character)
        {
            var localControl = character.CharacterControl as LocalCharacterControl;

            if (!character.CharacterCamera.InZoomMode && !character.TargetingSystem.LockedCharacter)
            {
                // If not locked, we will just get a target (same as vanilla method)

                At.Invoke(localControl, "AcquireTarget");

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
                        At.Invoke(character.CharacterControl as LocalCharacterControl, "ReleaseTarget");
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

                var m_character = At.GetField(self as CharacterControl, "m_character") as Character;
                var m_targetingSystem = m_character.TargetingSystem;

                bool m_lockHoldUp = false;

                if (!m_character.CharacterCamera.InZoomMode
                    && (ControlsInput.LockToggle(m_character.OwnerPlayerSys.PlayerID) || ControlsInput.LockHoldDown(m_character.OwnerPlayerSys.PlayerID)))
                {
                    At.SetField(self, "m_lockHoldUp", false);

                    if (m_targetingSystem.Locked)
                    {
                        At.Invoke(self, "ReleaseTarget");

                        if (self.ControlMode == LocalCharacterControl.CameraControlMode.Classic)
                        {
                            self.FaceLikeCamera = false;
                        }
                    }
                    else
                    {
                        At.Invoke(self, "AcquireTarget");

                        if (m_targetingSystem.Locked && self.ControlMode == LocalCharacterControl.CameraControlMode.Classic)
                        {
                            self.FaceLikeCamera = true;
                        }
                    }
                }

                if (ControlsInput.LockHoldUp(m_character.OwnerPlayerSys.PlayerID))
                {
                    m_lockHoldUp = true;
                    At.SetField(self, "m_lockHoldUp", true);
                }

                if (!m_character.CharacterCamera.InZoomMode && m_lockHoldUp)
                {
                    At.Invoke(self, "ReleaseTarget");
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
                            At.Invoke(self, "SwitchTarget", new object[] { lockingPoint });
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

                        float m_lastTargetSwitchTime = (float)At.GetField(self, "m_lastTargetSwitchTime");

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
                                var m_timeOfLastAimOffset = (float)At.GetField(self, "m_timeOfLastAimOffset");
                                var m_timeToNextAimOffset = (float)At.GetField(self, "m_timeToNextAimOffset");
                                var m_aimOffsetRandom = (Vector2)At.GetField(self, "m_aimOffsetRandom");

                                if (ControlsInput.IsLastActionGamepad(m_character.OwnerPlayerSys.PlayerID))
                                {
                                    Vector2 a = vector;
                                    a.x *= -1f;
                                    if (Time.time - m_timeOfLastAimOffset > m_timeToNextAimOffset)
                                    {
                                        m_aimOffsetRandom = UnityEngine.Random.insideUnitCircle;
                                        At.SetField(self, "m_aimOffsetRandom", m_aimOffsetRandom);
                                        At.SetField(self, "m_timeOfLastAimOffset", Time.time);
                                        At.SetField(self, "m_timeToNextAimOffset", UnityEngine.Random.Range(0.1f, 0.3f));
                                    }

                                    a += m_aimOffsetRandom * ((Vector3)At.GetField(self, "m_modifMoveInput")).magnitude * Time.deltaTime * 0.5f;

                                    m_character.TargetingSystem.LockingPointOffset = Vector2.Scale(a, new Vector2(-1f, 1f));
                                }
                                else
                                {
                                    Vector2 vector2 = vector * self.LockAimMouseSense;
                                    vector2.x *= -1f;
                                    if (Time.time - m_timeOfLastAimOffset > m_timeToNextAimOffset)
                                    {
                                        m_aimOffsetRandom = UnityEngine.Random.insideUnitCircle;
                                        At.SetField(self, "m_aimOffsetRandom", m_aimOffsetRandom);
                                        At.SetField(self, "m_timeOfLastAimOffset", Time.time);
                                        At.SetField(self, "m_timeToNextAimOffset", UnityEngine.Random.Range(0.1f, 0.3f));
                                    }
                                    vector2 += m_aimOffsetRandom * ((Vector3)At.GetField(self, "m_modifMoveInput")).magnitude * Time.deltaTime * 0.5f;
                                    m_character.TargetingSystem.LockingPointOffset -= new Vector3(vector2.x, vector2.y, 0);
                                    m_character.TargetingSystem.LockingPointOffset = Vector3.ClampMagnitude(m_character.TargetingSystem.LockingPointOffset, 1f);
                                }
                            }
                            At.SetField(self, "m_previousInput", vector);
                        }
                        else if (ControlsInput.IsLastActionGamepad(m_character.OwnerPlayerSys.PlayerID) && magnitude == 0f)
                        {
                            At.SetField(self, "m_lastTargetSwitchTime", 0f);
                        }
                    }
                    else if (self.TargetMode == LocalCharacterControl.TargetingMode.Aim)
                    {
                        Global.LockCursor(false);
                    }

                    Vector3 lockedPointPos = m_targetingSystem.LockedPointPos;
                    float m_lastInSightTime = (float)At.GetField(self, "m_lastInSightTime");
                    if (!Physics.Linecast(m_character.CenterPosition, lockedPointPos, Global.SightHideMask))
                    {
                        m_lastInSightTime = Time.time;
                        At.SetField(self, "m_lastInSightTime", m_lastInSightTime);
                    }

                    bool isLocked = m_targetingSystem.LockedCharacter != null && !m_targetingSystem.LockedCharacter.Alive;
                    if (Vector3.Distance(lockedPointPos, m_character.CenterPosition) > m_targetingSystem.TrueRange + 2f || Time.time - m_lastInSightTime > 1f || isLocked)
                    {
                        At.Invoke(self, "ReleaseTarget");
                        self.Invoke("AcquireTarget", 0.5f);
                    }
                }
                else
                {
                    m_targetingSystem.LockingPointOffset = Vector3.zero;
                    if (m_character.CharacterCamera.InZoomMode)
                    {
                        float m_lastFreeAimUpdateTime = (float)At.GetField(self, "m_lastFreeAimUpdateTime");
                        if (Time.time - m_lastFreeAimUpdateTime > 0.05f)
                        {
                            m_lastFreeAimUpdateTime = Time.time;
                            At.SetField(self, "m_lastFreeAimUpdateTime", m_lastFreeAimUpdateTime);

                            bool m_debugFreeAim = (bool)At.GetField(self, "m_debugFreeAim");

                            At.SetField(self, "m_freeAimTargetPos", m_character.CharacterCamera.GetObstaclePos(new Vector3(0.5f, 0.5f, 0f), m_debugFreeAim));
                        }


                        var m_freeAimLockingPoint = At.GetField(self, "m_freeAimLockingPoint") as LockingPoint;
                        var m_freeAimTargetPos = (Vector3)At.GetField(self, "m_freeAimTargetPos");
                        if ((bool)At.GetField(self, "m_wasFreeAiming"))
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

                At.SetField(self, "m_wasFreeAiming", m_character.CharacterCamera.InZoomMode);

                return false;
            }
        }
    }
}
