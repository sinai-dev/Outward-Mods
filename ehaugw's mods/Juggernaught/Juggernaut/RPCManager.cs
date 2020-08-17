using System;
using System.Threading.Tasks;
using Photon;
using TinyHelper;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000002 RID: 2
	internal class RPCManager : Photon.MonoBehaviour
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		internal void Start()
		{
			RPCManager.Instance = this;
			PhotonView photonView = base.gameObject.AddComponent<PhotonView>();
			photonView.viewID = 950;
			Debug.Log("Registered Juggernaut RPC with ViewID " + base.photonView.viewID.ToString());
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020A0 File Offset: 0x000002A0
		[PunRPC]
		public void GetJuggernautTrainerFromServer(string trainerUID, int sceneViewID, int recursionCount)
		{
			int num = 100;
			Character character = CharacterManager.Instance.GetCharacter(trainerUID);
			bool flag = character != null;
			if (flag)
			{
				JuggernautTrainer.SetupTrainerClientSide(character.gameObject, sceneViewID);
			}
			else
			{
				bool flag2 = recursionCount * num < 5000;
				if (flag2)
				{
					DelayedTask.GetTask(num).ContinueWith(delegate(Task _)
					{
						this.GetJuggernautTrainerFromServer(trainerUID, sceneViewID, recursionCount + 1);
					});
					Console.Read();
				}
				else
				{
					Debug.Log("Juggernaut could not fetch trainer ID from Server");
				}
			}
		}

		// Token: 0x04000001 RID: 1
		public static RPCManager Instance;
	}
}
