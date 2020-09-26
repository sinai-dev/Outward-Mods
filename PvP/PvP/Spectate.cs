using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PvP
{
    public class Spectate : MonoBehaviour
    {
        public static Transform CameraTransform => VideoCamera.Instance.VideoCameraTrans;

        public static Camera FreeCamera
        {
            get
            {
                if (!m_camera)
                {
                    m_camera = (Camera)At.GetValue(typeof(VideoCamera), VideoCamera.Instance, "m_camera");
                }
                return m_camera;
            }
        }
        private static Camera m_camera;

        private Character m_ownerCharacter;
        private Vector3 m_startPosition;

        public Character[] AvailableTargets => PlayerManager.Instance.GetRemainingPlayers().ToArray();
        private Character m_target;
        private int m_spectateTargetIndex;
        private bool hasTarget;

        internal void Awake()
        {
            m_ownerCharacter = this.gameObject.GetComponent<PlayerSystem>().ControlledCharacter;
            m_startPosition = m_ownerCharacter.transform.position;

            // get closest character to spectate
            var list = new List<Character>();
            CharacterManager.Instance.FindCharactersInRange(m_ownerCharacter.transform.position, 10f, ref list);
            if (list.Contains(m_ownerCharacter)) list.Remove(m_ownerCharacter);

            // disable character
            RPCManager.SendSetPlayerActive(m_ownerCharacter.UID, false);

            // enable free camera
            SetFreeCam(true);

            // pick any closest character to spectate
            if (list.Count > 0) SetTarget(list[0]);
        }

        private void SetFreeCam(bool active)
        {
            At.Call(typeof(VideoCamera), VideoCamera.Instance, "SetCameraActive", null, active);
            
            if (active)
            {
                At.SetValue(true, typeof(VideoCamera), VideoCamera.Instance, "m_flyMode");
                Global.LockCursor(true);
            }
        }

        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                CycleTarget(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                CycleTarget(1);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                m_spectateTargetIndex = 0;
                ReleaseTarget();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && !PvPGUI.Instance.showGui)
            {
                ReleaseTarget();
                PvPGUI.Instance.showGui = true;
                Global.LockCursor(false);
            }

            if (hasTarget)
            {
                UpdateTarget();
            }
        }

        private void UpdateTarget()
        {
            if (!m_target || m_target.IsDead)
            {
                ReleaseTarget();
                return;
            }

            var pos = m_target.transform.position + (Vector3.up * 4f) + (m_target.transform.forward * -6f);            

            FreeCamera.transform.position = pos;
            CameraTransform.position = pos;

            FreeCamera.transform.LookAt(m_target.transform);
            CameraTransform.LookAt(m_target.transform);
        }

        private void CycleTarget(int change)
        {
            if (change == -1)
            {
                if (m_spectateTargetIndex > 0) m_spectateTargetIndex--;
                else m_spectateTargetIndex = AvailableTargets.Length - 1;
            }
            else
            {
                if (m_spectateTargetIndex < AvailableTargets.Length - 1) m_spectateTargetIndex++;
                else m_spectateTargetIndex = 0;
            }            

            SetTarget(AvailableTargets[m_spectateTargetIndex]);
        }

        private void SetTarget(Character target)
        {
            hasTarget = true;
            m_target = target;
            Global.LockCursor(true);

            RPCManager.Instance.SendUIMessageLocal(m_ownerCharacter, "Spectating " + target.Name);

            At.SetValue(target.transform, typeof(VideoCamera), VideoCamera.Instance, "m_targetTrans");
            At.SetValue(VideoCamera.VideoCamState.FOLLOW_POS, typeof(VideoCamera), VideoCamera.Instance, "m_state");
        }

        private void ReleaseTarget()
        {
            hasTarget = false;
            m_target = null;

            At.SetValue((Transform)null, typeof(VideoCamera), VideoCamera.Instance, "m_targetTrans");
            At.SetValue(VideoCamera.VideoCamState.NORMAL, typeof(VideoCamera), VideoCamera.Instance, "m_state");
        }

        public void EndSpectate()
        {
            // end free camera
            ReleaseTarget();
            SetFreeCam(false);

            // enable character
            RPCManager.SendSetPlayerActive(m_ownerCharacter.UID, true);
            m_ownerCharacter.Teleport(m_startPosition, Quaternion.identity);

            Destroy(this);
        }
    }
}
