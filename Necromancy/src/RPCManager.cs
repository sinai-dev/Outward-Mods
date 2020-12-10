//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Photon;
//using UnityEngine;

//namespace NecromancerSkills
//{
//    public class RPCManager : Photon.MonoBehaviour
//    {
//        // This class is not used much, just for a little bit of custom Photon functions for setting up summoned AIs and custom trainer NPCs.

//        // If you want to a function to be remote-callable, you must declare the [PunRPC] tag above it.
//        // This will allow you to do a photonView.RPC("MethodName", PhotonTargets.All, new object[] { parameters for method go here });
//        // Note: your parameter arguments MUST be primitive types! (string, int, float, bool, etc).

//        public static GameObject obj;
//        public static RPCManager Instance;

//        internal void Start()
//        {
//            Instance = this;

//            var view = this.gameObject.AddComponent<PhotonView>();
//            view.viewID = 900;
//            Debug.Log("Registered NecromancerSkills with ViewID " + this.photonView.viewID);
//        }

//        // ===================== SUMMON MANAGER RPC ===================== // 

//        public void SendSummonSpawn(string ownerUID, string summonUID)
//        {
//            this.photonView.RPC("RPCSendSummonSpawn", PhotonTargets.All, new object[] { ownerUID, summonUID });
//        }

//        [PunRPC]
//        private void RPCSendSummonSpawn(string ownerUID, string summonUID)
//        {
//            SummonManager.Instance.AddLocalSummon(ownerUID, summonUID);
//        }

//        // ===================== TRAINER MANAGER RPC ===================== // 

//        public void RPCSendTrainerSpawn()
//        {
//            this.photonView.RPC("SendTrainerSpawn", PhotonTargets.All, new object[0]);
//        }

//        [PunRPC]
//        private void SendTrainerSpawn()
//        {
//            //Debug.Log("TrainerSpawn received with UID: " + trainerUID + " and scene view ID: " + sceneViewID);

//            if (CharacterManager.Instance.GetCharacter(TrainerManager.TRAINER_UID) is Character c)
//            {
//                TrainerManager.Instance.LocalTrainerSetup(c.gameObject);
//            }
//            else
//            {
//                StartCoroutine(TrainerSetupCoroutine());
//            }
//        }

//        private IEnumerator TrainerSetupCoroutine()
//        {
//            // Debug.Log("Couldn't get character immediately, starting coroutine to find character...");

//            float t = Time.time;

//            while (Time.time - t < 5)
//            {
//                if (CharacterManager.Instance.GetCharacter(TrainerManager.TRAINER_UID) is Character c)
//                {
//                    TrainerManager.Instance.LocalTrainerSetup(c.gameObject);
//                    break;
//                }
//                else
//                {
//                    yield return new WaitForSeconds(0.1f);
//                }
//            }

//            if (!CharacterManager.Instance.GetCharacter(TrainerManager.TRAINER_UID))
//            {
//                Debug.LogWarning("Timed out trying to find character!");
//            }
//        }
//    }
//}
