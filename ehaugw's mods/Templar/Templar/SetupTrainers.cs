using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x0200000F RID: 15
	public class SetupTrainers
	{
		// Token: 0x0600001B RID: 27 RVA: 0x000033B0 File Offset: 0x000015B0
		public static void SetupRufusInteraction()
		{
			GameObject gameObject = GameObject.Find("Dialogue_Rufus/NPC/DialogueTree_1_Initial");
			bool flag = gameObject != null;
			if (flag)
			{
				DialogueTreeController component = gameObject.GetComponent<DialogueTreeController>();
				bool flag2 = component != null;
				if (flag2)
				{
					Graph graph = component.graph;
					List<Node> allNodes = graph.allNodes;
					StatementNodeExt statementNodeExt = allNodes.FirstOrDefault((Node n) => n != null && n is StatementNodeExt && ((StatementNodeExt)n).statement.text.Contains("Elatt however... I’ve had the honor to speak with him. He is very real. Having a god that was once man is comforting.")) as StatementNodeExt;
					bool flag3 = statementNodeExt != null;
					if (flag3)
					{
						ActionNode actionNode = graph.AddNode<ActionNode>();
						GiveReward giveReward = new GiveReward();
						actionNode.action = giveReward;
						giveReward.RewardReceiver = GiveReward.Receiver.Instigator;
						NodeCanvas.Tasks.Actions.ItemQuantity itemQuantity = new NodeCanvas.Tasks.Actions.ItemQuantity();
						itemQuantity.Item = new BBParameter<ItemReference>(new ItemReference
						{
							ItemID = Templar.Instance.prayerInstance.ItemID
						});
						itemQuantity.Quantity = 1;
						giveReward.ItemReward = new List<NodeCanvas.Tasks.Actions.ItemQuantity>
						{
							itemQuantity
						};
						statementNodeExt.outConnections[0] = Connection.Create(statementNodeExt, actionNode, -1, -1);
						FinishNode finishNode = graph.AddNode<FinishNode>();
						allNodes.Add(finishNode);
						graph.ConnectNodes(actionNode, finishNode, -1, -1);
					}
				}
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000034EC File Offset: 0x000016EC
		public static void SetupAltarInteraction(ref Trainer altarTrainer, ref SkillSchool templarTreeInstance)
		{
			GameObject gameObject = GameObject.Find("DialogueAltar/NPC/InteractionActivatorSettings");
			bool flag = gameObject != null;
			if (flag)
			{
				NPCInteraction componentInChildren = gameObject.GetComponentInChildren<NPCInteraction>();
				bool flag2 = componentInChildren != null;
				if (flag2)
				{
					bool flag3 = componentInChildren.ActorLocKey == "name_unpc_altar_01";
					if (flag3)
					{
						DialogueTreeController dialogueController = componentInChildren.NPCDialogue.DialogueController;
						Graph graph = dialogueController.graph;
						List<Node> allNodes = graph.allNodes;
						MultipleChoiceNodeExt multipleChoiceNodeExt = allNodes.FirstOrDefault(delegate(Node n)
						{
							bool result;
							if (n != null && n is MultipleChoiceNodeExt)
							{
								result = ((from m in ((MultipleChoiceNodeExt)n).availableChoices
								where m.statement.text == "(Offer a prayer to Elatt)"
								select m).ToList<MultipleChoiceNodeExt.Choice>().Count > 0);
							}
							else
							{
								result = false;
							}
							return result;
						}) as MultipleChoiceNodeExt;
						bool flag4 = multipleChoiceNodeExt == null;
						if (!flag4)
						{
							string text = "(Swear an oath to Elatt.)";
							MultipleChoiceNodeExt.Choice choice = new MultipleChoiceNodeExt.Choice();
							choice.statement = new Statement();
							choice.statement.text = text;
							choice.isUnfolded = true;
							multipleChoiceNodeExt.availableChoices.Insert(0, choice);
							ActionNode actionNode = graph.AddNode<ActionNode>();
							allNodes.Add(actionNode);
							altarTrainer = new Trainer();
							At.SetValue<UID>(UID.Generate(), typeof(Trainer), altarTrainer, "m_uid");
							At.SetValue<UID>(templarTreeInstance.UID, typeof(Trainer), altarTrainer, "m_skillTreeUID");
							actionNode.action = new TrainDialogueAction();
							((TrainDialogueAction)actionNode.action).Trainer = new BBParameter<Trainer>(altarTrainer);
							BBParameter<Character> bbparameter = new BBParameter<Character>();
							bbparameter.name = "gInstigator";
							((TrainDialogueAction)actionNode.action).PlayerCharacter = bbparameter;
							graph.ConnectNodes(multipleChoiceNodeExt, actionNode, 0, -1);
							FinishNode finishNode = graph.AddNode<FinishNode>();
							allNodes.Add(finishNode);
							graph.ConnectNodes(actionNode, finishNode, -1, -1);
						}
					}
				}
			}
		}
	}
}
