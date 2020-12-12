using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using Photon;

namespace CombatAndDodgeOverhaul
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
            var photon = this.gameObject.AddComponent<PhotonView>();
            photon.viewID = 902;
            Debug.Log("Registered C&D Overhaul with ViewID " + this.photonView.viewID);
        }

        public void RequestSettings()
        {
            //Debug.Log("sending settings request to master client. viewID: " + this.photonView.viewID);
            this.photonView.RPC("RequestSettingsRPC", PhotonNetwork.masterClient, new object[0]);
        }

        [PunRPC]
        private void RequestSettingsRPC()
        {
            StartCoroutine(DelayedSendSettingsRPC());
        }

        private IEnumerator DelayedSendSettingsRPC()
        {
            //Debug.Log("Received settings request, waiting for players to be done loading...");
            while (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
            {
                yield return new WaitForSeconds(0.2f);
            }

            if (!PhotonNetwork.isNonMasterClientInRoom && (EnemyManager.Instance.TimeOfLastSyncSend < 0 || Time.time - EnemyManager.Instance.TimeOfLastSyncSend > 10f))
            {
                EnemyManager.Instance.TimeOfLastSyncSend = Time.time;
                //Debug.Log("Sending settings to all clients. View id: " + this.photonView.viewID);
                this.photonView.RPC("SendSettingsRPC", PhotonTargets.All, new object[]
                {
                    true,
                    (bool)CombatOverhaul.config.GetValue(Settings.All_Enemies_Allied),
                    (bool)CombatOverhaul.config.GetValue(Settings.Enemy_Balancing),
                    (float)CombatOverhaul.config.GetValue(Settings.Enemy_Health),
                    (float)CombatOverhaul.config.GetValue(Settings.Enemy_Damages),
                    (float)CombatOverhaul.config.GetValue(Settings.Enemy_ImpactRes),
                    (float)CombatOverhaul.config.GetValue(Settings.Enemy_Resistances),
                    (float)CombatOverhaul.config.GetValue(Settings.Enemy_ImpactDmg)
                });
            }
        }

        [PunRPC]
        private void SendSettingsRPC(bool modsEnabled, bool enemiesAllied, bool customStats, float healthModifier, float damageModifier, float impactRes, float damageRes, float impactDmg)
        {
            //Debug.Log("Received settings RPC.");
            if (PhotonNetwork.isNonMasterClientInRoom)
            {
                //Debug.Log("We are not host, setting to received infos");
                EnemyManager.Instance.SetSyncInfo(modsEnabled, enemiesAllied, customStats, healthModifier, damageModifier, impactRes, damageRes, impactDmg);
            }
        }
    }
}
