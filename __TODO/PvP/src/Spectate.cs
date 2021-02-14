using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
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
                    m_camera = (Camera)At.GetField(VideoCamera.Instance, "m_camera");
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
            At.Invoke(VideoCamera.Instance, "SetCameraActive", active);
            
            if (active)
            {
                At.SetField(VideoCamera.Instance, "m_flyMode", true);
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
            else if (Input.GetKeyDown(KeyCode.Escape) && !PvPGUI.Instance.ShowGUI)
            {
                ReleaseTarget();
                PvPGUI.Instance.ShowGUI = true;
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

            At.SetField(VideoCamera.Instance, "m_targetTrans", target.transform);
            At.SetField(VideoCamera.Instance, "m_state", VideoCamera.VideoCamState.FOLLOW_POS);
        }

        private void ReleaseTarget()
        {
            hasTarget = false;
            m_target = null;

            At.SetField(VideoCamera.Instance, "m_targetTrans", null);
            At.SetField(VideoCamera.Instance, "m_state", VideoCamera.VideoCamState.NORMAL);
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
