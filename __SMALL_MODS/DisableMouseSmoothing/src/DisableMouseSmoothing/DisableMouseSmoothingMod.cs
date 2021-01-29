using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace DisableMouseSmoothing
{
    [BepInPlugin("com.sinai.mousesmoothfix", "Sinai", "1.0")]
    public class DisableMouseSmoothingMod : BaseUnityPlugin
    {
        internal static bool Enabled = true;

        internal void Awake()
        {
            new Harmony("com.sinai.mousesmoothfix").PatchAll();
        }
    }

    [HarmonyPatch(typeof(CharacterCamera), "Update")]
    public class CharacterCamera_Update
    {
        [HarmonyPrefix]
        public static bool Prefix(CharacterCamera __instance, ref Vector2 ___m_cameraSmoothAutoInput, ref Vector2 ___m_cameraSmoothInput,
            bool ___m_invertedVer, bool ___m_invertedHor, ref Vector2 ___m_smoothCameraInput, Transform ___m_cameraVertHolder, Transform ___m_horiControl,
            Transform ___m_vertControl, CameraShaker ___m_shaker)
        {
            if (DisableMouseSmoothingMod.Enabled)
            {
                Override(__instance, ref ___m_cameraSmoothAutoInput, ref ___m_cameraSmoothInput, ___m_invertedVer, ___m_invertedHor,
                ref ___m_smoothCameraInput, ___m_cameraVertHolder, ___m_horiControl, ___m_vertControl, ___m_shaker);

                return false;
            }

            return true;
        }

        internal static void Override(CharacterCamera __instance, ref Vector2 ___m_cameraSmoothAutoInput, ref Vector2 ___m_cameraSmoothInput,
            bool ___m_invertedVer, bool ___m_invertedHor, ref Vector2 ___m_smoothCameraInput, Transform ___m_cameraVertHolder, Transform ___m_horiControl,
            Transform ___m_vertControl, CameraShaker ___m_shaker)
        {
            if (__instance.TargetCharacter == null || !NetworkLevelLoader.Instance.IsOverallLoadingDone || MenuManager.Instance.IsReturningToMainMenu)
                return;

            if (__instance.transform.position.y < -500f)
                __instance.CameraScript.farClipPlane = 200f;
            else
                __instance.CameraScript.farClipPlane = 26000f;
            
            float delta = Time.deltaTime;
            if (delta > 0.04f)
                delta = 0.04f;
            
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // CHANGE: Removed ' * 0.5f ' from the RotateCameraVertical value.
            Vector2 cameraMove = new Vector2(ControlsInput.RotateCameraHorizontal(__instance.TargetCharacter.OwnerPlayerSys.PlayerID), 
                                             ControlsInput.RotateCameraVertical(__instance.TargetCharacter.OwnerPlayerSys.PlayerID)); // * 0.5f));
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            if (__instance.TargetCharacter.CharacterUI)
            {
                float mouseSens = OptionManager.Instance.GetMouseSense(__instance.TargetCharacter.OwnerPlayerSys.PlayerID);
                mouseSens /= 5f;
                cameraMove *= mouseSens;
            }

            cameraMove.x = Mathf.Clamp(cameraMove.x, -30f, 30f);
            cameraMove.y = Mathf.Clamp(cameraMove.y, -20f, 20f);

            // Controller smoothing
            if (ControlsInput.IsLastActionGamepad(__instance.TargetCharacter.OwnerPlayerSys.PlayerID))
            {
                if (cameraMove == Vector2.zero)
                {
                    Vector3 targetPos = Vector3.zero;
                    
                    if (!__instance.TargetCharacter.IsDead)
                        targetPos = __instance.TargetCharacter.transform.TransformVector(new Vector3(__instance.TargetCharacter.AnimMove.x, 
                                                                                                   0f, 
                                                                                                   __instance.TargetCharacter.AnimMove.y));
                    
                    targetPos = __instance.transform.InverseTransformVector(targetPos);
                    
                    float angle = Vector3.forward.AngleWithDir(targetPos, Vector3.up);
                    
                    Vector2 lerpTo = Vector2.zero;
                    
                    if (Mathf.Abs(angle) > 15f)
                    {
                        if (angle > 0f && angle < 30f)
                            angle = Mathf.Clamp(angle * 1.5f, 20f, 40f);
                        else if (angle > -30f && angle < 0f)
                            angle = Mathf.Clamp(angle * 1.5f, -20f, -40f);

                        angle /= 360f;
                        angle *= targetPos.magnitude;
                        lerpTo.x = angle * 0.3f;
                    }
                    
                    float t = (lerpTo.magnitude < 0.1f) 
                                ? (Time.deltaTime * 5f) 
                                : Mathf.Clamp(Time.deltaTime * 1f, 0f, 0.1f);
                    
                    ___m_cameraSmoothAutoInput = Vector2.Lerp(___m_cameraSmoothAutoInput, lerpTo, t);
                }
                else
                    ___m_cameraSmoothAutoInput = Vector2.Lerp(___m_cameraSmoothAutoInput, Vector2.zero, Time.deltaTime * 20f);

                cameraMove += ___m_cameraSmoothAutoInput;
            }

            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // CHANGE: Commenting out these next two lines:
            //___m_cameraSmoothInput = Vector2.Lerp(___m_cameraSmoothInput, cameraMove, Mathf.Clamp(Time.deltaTime * 20f, 0f, 0.6f));
            //cameraMove = ___m_cameraSmoothInput;
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            if (Global.ListenToDebugInput && Input.GetKeyDown(KeyCode.KeypadPeriod))
                OptionManager.Instance.ChangeInvertMouseY(__instance.TargetCharacter.OwnerPlayerSys.PlayerID, !___m_invertedVer);

            cameraMove.y *= (float)(___m_invertedVer ? -1 : 1);
            cameraMove.x *= (float)(___m_invertedHor ? -1 : 1);

            if (__instance.LookAtTransform != null)
                cameraMove *= 0.1f;

            if (__instance.InZoomMode)
                cameraMove *= __instance.ZoomSensModifier;

            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // CHANGE: Commenting out these next three lines:
            //___m_smoothCameraInput = Vector2.Lerp(___m_smoothCameraInput, cameraMove, 15f * delta);
            //___m_smoothCameraInput = Vector2.MoveTowards(___m_smoothCameraInput, cameraMove, 5f * delta);
            //cameraMove = ___m_smoothCameraInput;
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            cameraMove *= ((__instance.LookAtTransform == null) 
                            ? __instance.FreeSense 
                            : __instance.LockedSense) * (ControlsInput.IsLastActionGamepad(__instance.TargetCharacter.OwnerPlayerSys.PlayerID) 
                                                            ? delta 
                                                            : 0.016f);
            
            __instance.transform.Rotate(new Vector3(0f, cameraMove.x, 0f));

            ___m_cameraVertHolder.Rotate(new Vector3(-cameraMove.y, 0f, 0f));
            
            // Vertical position / Lock-on 
            if (__instance.LookAtTransform != null || __instance.OverrideTransform != null)
            {
                Vector3 vertPosition;
                if (!__instance.OverrideTransform)
                {
                    Vector3 position = __instance.LookAtTransform.transform.position;
                    position.y -= 0.8f;
                    float diffToPlayer = Mathf.Abs(__instance.LookAtTransform.transform.position.y - __instance.transform.position.y);
                    float diffToTarget = Mathf.Abs(__instance.LookAtTransform.transform.position.y - __instance.TargetCharacter.transform.position.y);
                    float normalize = Mathf.Clamp(diffToPlayer * 0.5f, 1f, 10f);
                    vertPosition = position - __instance.transform.position;
                    
                    float yCurve = __instance.YMinMaxCurve.Evaluate(Vector3.Distance(__instance.TargetCharacter.transform.position, 
                                                                                     __instance.LookAtTransform.transform.position) * 0.1f);
                    
                    float yNormalize = (diffToTarget < 5f) ? __instance.YMinMaxYDiffCurve.Evaluate(diffToTarget) : 1f;
                    
                    yCurve *= yNormalize;
                    
                    vertPosition.y = Mathf.Clamp(vertPosition.y, -yCurve * normalize, yCurve * normalize);
                    vertPosition.y -= 0.5f;
                }
                else
                    vertPosition = __instance.OverrideTransform.forward;

                float vertAngle = Vector3.Angle(___m_cameraVertHolder.forward, vertPosition);
                
                ___m_cameraVertHolder.forward = Vector3.MoveTowards(___m_cameraVertHolder.forward, 
                                                                        vertPosition, 
                                                                        (__instance.RotSpeeds.x + vertAngle * __instance.RotSpeeds.y) * Time.deltaTime);

                __instance.transform.rotation = Quaternion.Euler(0f, ___m_cameraVertHolder.rotation.eulerAngles.y, 0f);
            }

            Vector3 diffToVert = __instance.transform.InverseTransformDirection(___m_cameraVertHolder.forward);
            float diffAngle = new Vector2(diffToVert.z, diffToVert.y).Angle(new Vector2(1f, 0f));
            float normalizedAngle = 75f;

            if (Mathf.Abs(diffAngle) > normalizedAngle)
            {
                normalizedAngle = (diffAngle > 0f) 
                                    ? normalizedAngle 
                                    : (-normalizedAngle);

                ___m_cameraVertHolder.Rotate(diffAngle - normalizedAngle, 0f, 0f, Space.Self);
            }

            ___m_cameraVertHolder.localRotation = Quaternion.Euler(___m_cameraVertHolder.localRotation.eulerAngles.x, 0f, 0f);
            ___m_horiControl.transform.rotation = __instance.transform.rotation;
            ___m_vertControl.transform.rotation = ___m_cameraVertHolder.rotation;

            if (___m_shaker && !Global.GamePaused)
            {
                ___m_shaker.RealPosition = __instance.CameraScript.transform.localPosition;
                ___m_shaker.RealRotation = Quaternion.identity;
            }

            if (s_updateZoomMethod == null)
                s_updateZoomMethod = typeof(CharacterCamera).GetMethod("UpdateZoom", BindingFlags.NonPublic | BindingFlags.Instance);

            s_updateZoomMethod.Invoke(__instance, new object[0]);
        }

        internal static MethodInfo s_updateZoomMethod;
    }
}
