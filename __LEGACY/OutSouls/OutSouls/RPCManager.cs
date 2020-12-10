using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon;
using UnityEngine;
//using SinAPI;

namespace OutSoulsMod
{
    public class RPCManager : Photon.MonoBehaviour
    {
        public static RPCManager Instance;
        
        internal void Awake()
        {
            Instance = this;
        }

        internal void Start()
        {
            var view = this.gameObject.AddComponent<PhotonView>();
            view.viewID = 903;
            Debug.Log("Registered OutSouls with ViewID " + this.photonView.viewID);
        }

        //for calling directly
        public void SendBonfire(Vector3 position, string uid)
        {
            this.photonView.RPC("SendBonfireRPC", PhotonTargets.All, new object[] { position.x, position.y, position.z, uid });
        }

        [PunRPC]
        private void SendBonfireRPC(float x, float y, float z, string uid)
        {
            StartCoroutine(BonfireManager.Instance.SetupBonfireLocal(new Vector3(x, y, z), uid));
        }

        //for calling directly
        public void RequestBonfireSyncInfo(string _askerUID)
        {
            this.photonView.RPC("RequestBonfiresRPC", PhotonTargets.MasterClient, new object[] { _askerUID });
        }

        [PunRPC]
        private void RequestBonfiresRPC(string _askerUID)
        {
            BonfireManager.Instance.SendSyncInfo(_askerUID);
        }

        //for calling directly
        public void SendSyncInfo(string _askerUID, List<BonfireInfo> _bonfires)
        {
            List<string> uids = new List<string>();
            List<float> x = new List<float>();
            List<float> y = new List<float>();
            List<float> z = new List<float>();
            foreach (BonfireInfo info in _bonfires)
            {
                uids.Add(info.uid);
                x.Add(info.position.x);
                y.Add(info.position.y);
                z.Add(info.position.z);
            }

            this.photonView.RPC("SendSyncInfoRPC", PhotonTargets.Others, new object[] 
            {
                _askerUID,
                uids.ToArray() as object,
                x.ToArray() as object,
                y.ToArray() as object,
                z.ToArray() as object
            });
        }

        [PunRPC]
        private void SendSyncInfoRPC(string _askerUID, string[] _uids, float[] x, float[] y, float[] z)
        {
            if (CharacterManager.Instance.GetCharacter(_askerUID) is Character c && c.IsLocalPlayer)
            {
                Debug.Log("Received sync infos! Count: " + _uids.Length);
                List<BonfireInfo> bonfires = new List<BonfireInfo>();
                for (int i = 0; i < _uids.Length; i++)
                {
                    bonfires.Add(new BonfireInfo()
                    {
                        uid = _uids[i],
                        position = new Vector3(x[i], y[i], z[i])
                    });
                }
                BonfireManager.Instance.ReceiveSyncInfo(bonfires);
            }
        }



        // for calling directly
        public void SendBonfireReset()
        {
            this.photonView.RPC("SendBonfireResetRPC", PhotonTargets.All, new object[0]);
        }

        // coroutine just to work with the Silver Price Button things. its not really a coroutine, just immediately calls SendBonfireReset.
        public IEnumerator BonfireReset()
        {
            SendBonfireReset();
            yield return null;
        }

        [PunRPC]
        private void SendBonfireResetRPC()
        {
            StartCoroutine(BonfireManager.Instance.ResetArea());
        }

        // for calling directly
        public void SendTeleport(string SceneName, int SpawnPoint)
        {
            this.photonView.RPC("SendTeleportRPC", PhotonTargets.All, new object[] { SceneName, SpawnPoint });
        }

        [PunRPC]
        private void SendTeleportRPC(string SceneName, int SpawnPoint)
        {
            Vector3 position = BonfireManager.Instance.bonfirePositions[SceneName][SpawnPoint];
            StartCoroutine(BonfireManager.Instance.Teleport(SceneName, position));
        }
    }
}
