using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using SideLoader;
using UnityEngine;
using UnityEngine.Events;

namespace RunicScrolls
{
	// Token: 0x02000006 RID: 6
	[BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
	[BepInPlugin("com.ehaugw.runicscrolls", "Runic Scrolls", "2.0.2")]
	public class RunicScrolls : BaseUnityPlugin
	{
		// Token: 0x0600000A RID: 10 RVA: 0x00002530 File Offset: 0x00000730
		internal void Awake()
		{
			SL.OnPacksLoaded += new UnityAction(this.OnPackLoaded);
			SL.OnSceneLoaded += new UnityAction(this.OnSceneLoaded);
			Harmony harmony = new Harmony("com.ehaugw.runicscrolls");
			harmony.PatchAll();
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002574 File Offset: 0x00000774
		private void OnSceneLoaded()
		{
			foreach (GameObject gameObject in from x in Resources.FindObjectsOfTypeAll<GameObject>()
			where x.name == "UNPC_LaineAberforthA" || x.name == "HumanSNPC_CounterAlchemist"
			select x)
			{
				GuaranteedDrop[] componentsInChildren = gameObject.GetComponentsInChildren<GuaranteedDrop>();
				GuaranteedDrop guaranteedDrop;
				if (componentsInChildren == null)
				{
					guaranteedDrop = null;
				}
				else
				{
					guaranteedDrop = componentsInChildren.FirstOrDefault((GuaranteedDrop table) => table.ItemGenatorName == "Recipes");
				}
				GuaranteedDrop guaranteedDrop2 = guaranteedDrop;
				bool flag = guaranteedDrop2 != null;
				if (flag)
				{
					List<BasicItemDrop> list = At.GetValue(typeof(GuaranteedDrop), guaranteedDrop2, "m_itemDrops") as List<BasicItemDrop>;
					bool flag2 = list != null;
					if (flag2)
					{
						foreach (Item droppedItem in new Item[]
						{
							this.runicScrollRecipeInstance
						})
						{
							list.Add(new BasicItemDrop
							{
								DroppedItem = droppedItem,
								MaxDropCount = 1,
								MinDropCount = 1
							});
						}
					}
				}
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000026A0 File Offset: 0x000008A0
		private void OnPackLoaded()
		{
			this.runicScrollInstance = RunicScroll.MakeItem();
			this.runicScrollRecipeInstance = RunicScroll.MakeRecipes();
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000026BC File Offset: 0x000008BC
		public static bool IsRune(Item item)
		{
			return item.ItemID == 8100210 || item.ItemID == 8100220 || item.ItemID == 8100230 || item.ItemID == 8100240;
		}

		// Token: 0x04000003 RID: 3
		public const string GUID = "com.ehaugw.runicscrolls";

		// Token: 0x04000004 RID: 4
		public const string VERSION = "2.0.2";

		// Token: 0x04000005 RID: 5
		public const string NAME = "Runic Scrolls";

		// Token: 0x04000006 RID: 6
		public Item runicScrollInstance;

		// Token: 0x04000007 RID: 7
		public Item runicScrollRecipeInstance;

		// Token: 0x04000008 RID: 8
		public const string sideloaderFolder = "RunicScrolls";
	}
}
