using System;
using System.Collections.Generic;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using SideLoader;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000003 RID: 3
	public class JuggernautTrainer
	{
		// Token: 0x06000004 RID: 4 RVA: 0x0000214C File Offset: 0x0000034C
		public static void OnSceneChange()
		{
			GameObject gameObject = GameObject.Find("UNPC_The Juggernaut");
			bool flag = gameObject != null;
			if (flag)
			{
				bool flag2 = SceneManagerHelper.ActiveSceneName != "Berg";
				if (flag2)
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
			}
			else
			{
				bool flag3 = SceneManagerHelper.ActiveSceneName == "Berg" && !PhotonNetwork.isNonMasterClientInRoom;
				if (flag3)
				{
					JuggernautTrainer.SetupTrainerServerSide();
				}
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000021B0 File Offset: 0x000003B0
		public static GameObject SetupTrainerServerSide()
		{
			string text = UID.Generate().ToString();
			int num = PhotonNetwork.AllocateSceneViewID();
			GameObject gameObject = CustomCharacters.CreateCharacter(JuggernautTrainer.TrainerLocation, text, "SL_Character", false, null);
			Character component = gameObject.GetComponent<Character>();
			At.SetValue<CharacterManager.CharacterInstantiationTypes>(CharacterManager.CharacterInstantiationTypes.Item, typeof(Character), component, "m_instantiationType");
			foreach (int itemID in JuggernautTrainer.TrainerEquipment)
			{
				component.Inventory.Equipment.EquipInstantiate(ResourcesPrefabManager.Instance.GetItemPrefab(itemID) as Equipment);
			}
			component.ChangeFaction(Character.Factions.NONE, true);
			gameObject.SetActive(true);
			bool offlineMode = PhotonNetwork.offlineMode;
			if (offlineMode)
			{
				JuggernautTrainer.SetupTrainerClientSide(gameObject, num);
			}
			else
			{
				RPCManager.Instance.photonView.RPC("GetJuggernautTrainerFromServer", 0, new object[]
				{
					text.ToString(),
					num,
					0
				});
			}
			return gameObject;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000022B4 File Offset: 0x000004B4
		public static void SetupTrainerClientSide(GameObject juggernautGameObject, int trainerViewID)
		{
			GameObject gameObject = new GameObject("UNPC_The Juggernaut");
			gameObject.transform.position = JuggernautTrainer.TrainerLocation;
			juggernautGameObject.transform.parent = gameObject.transform;
			juggernautGameObject.transform.position = gameObject.transform.position;
			juggernautGameObject.transform.rotation = Quaternion.Euler(0f, 220.5518f, 0f);
			juggernautGameObject.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
			UnityEngine.Object.DestroyImmediate(juggernautGameObject.GetComponent<StartingEquipment>());
			Character component = juggernautGameObject.GetComponent<Character>();
			component.Stats.enabled = false;
			Weapon currentWeapon = component.CurrentWeapon;
			bool flag = currentWeapon != null && currentWeapon.TwoHanded;
			if (flag)
			{
				component.LeftHandEquipment = component.CurrentWeapon;
				component.LeftHandChanged();
			}
			component.Sheathed = false;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load("editor/templates/TrainerTemplate")) as GameObject;
			gameObject2.transform.parent = juggernautGameObject.transform;
			gameObject2.transform.position = juggernautGameObject.transform.position;
			gameObject2.transform.rotation = juggernautGameObject.transform.rotation;
			DialogueActor componentInChildren = gameObject2.GetComponentInChildren<DialogueActor>();
			componentInChildren.SetName("The Juggernaut");
			Trainer componentInChildren2 = gameObject2.GetComponentInChildren<Trainer>();
			At.SetValue<UID>(JuggernautMod.juggernautTreeInstance.UID, typeof(Trainer), componentInChildren2, "m_skillTreeUID");
			DialogueTreeController componentInChildren3 = gameObject2.GetComponentInChildren<DialogueTreeController>();
			Graph graph = componentInChildren3.graph;
			List<DialogueTree.ActorParameter> list = At.GetValue(typeof(DialogueTree), graph as DialogueTree, "_actorParameters") as List<DialogueTree.ActorParameter>;
			list[0].actor = componentInChildren;
			list[0].name = componentInChildren.name;
			List<Node> list2 = At.GetValue(typeof(Graph), graph, "_nodes") as List<Node>;
			StatementNodeExt statementNodeExt = graph.AddNode<StatementNodeExt>();
			statementNodeExt.statement = new Statement("What do you want, peasant?");
			statementNodeExt.SetActorName(componentInChildren.name);
			MultipleChoiceNodeExt multipleChoiceNodeExt = graph.AddNode<MultipleChoiceNodeExt>();
			multipleChoiceNodeExt.availableChoices.Add(new MultipleChoiceNodeExt.Choice
			{
				statement = new Statement
				{
					text = "I wish to become a legend like you!"
				}
			});
			multipleChoiceNodeExt.availableChoices.Add(new MultipleChoiceNodeExt.Choice
			{
				statement = new Statement
				{
					text = "Who are you?"
				}
			});
			ActionNode actionNode = list2[1] as ActionNode;
			(actionNode.action as TrainDialogueAction).Trainer = new BBParameter<Trainer>(componentInChildren2);
			StatementNodeExt statementNodeExt2 = graph.AddNode<StatementNodeExt>();
			statementNodeExt2.statement = new Statement("Hah! Like you don't know... Everyone knows me, I'm a living legend known as \"The Juggernaut\"!");
			statementNodeExt2.SetActorName(componentInChildren.name);
			list2.Clear();
			list2.Add(statementNodeExt);
			list2.Add(multipleChoiceNodeExt);
			list2.Add(actionNode);
			list2.Add(statementNodeExt2);
			graph.primeNode = statementNodeExt;
			graph.ConnectNodes(statementNodeExt, multipleChoiceNodeExt, -1, -1);
			graph.ConnectNodes(multipleChoiceNodeExt, actionNode, 0, -1);
			graph.ConnectNodes(multipleChoiceNodeExt, statementNodeExt2, 1, -1);
			graph.ConnectNodes(statementNodeExt2, statementNodeExt, -1, -1);
			gameObject.SetActive(true);
		}

		// Token: 0x04000002 RID: 2
		public const string TrainerName = "The Juggernaut";

		// Token: 0x04000003 RID: 3
		public const string TrainerSceneName = "Berg";

		// Token: 0x04000004 RID: 4
		public static Vector3 TrainerLocation = new Vector3(1207.5f, -13.72215f, 1378.747f);

		// Token: 0x04000005 RID: 5
		public static int[] TrainerEquipment = new int[]
		{
			2100000,
			3000035,
			3100061,
			3100095
		};
	}
}
