using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using SideLoader;
using UnityEngine;

namespace HolyAvengerItemPack
{
	[BepInPlugin("com.ehaugw.holyavengeritempack", "Holy Avenger Item Pack", "2.0.0")]
	[BepInDependency("com.ehaugw.customweaponbehaviour", BepInDependency.DependencyFlags.HardDependency)]
	public class HolyAvengerItemPack : BaseUnityPlugin
	{
		internal void Awake()
		{
			SL.OnPacksLoaded += this.OnPackLoaded;
			SL.OnSceneLoaded += this.OnSceneLoaded;
		}

		public static string[] GetSafeTags(List<string> tags)
		{
			List<string> list = new List<string>();
			using (List<string>.Enumerator enumerator = tags.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string tag = enumerator.Current;
					TagSourceManager.Instance.DbTags.FirstOrDefault((Tag x) => x.TagName == tag || x.UID == tag);
					bool flag = true;
					if (flag)
					{
						list.Add(tag);
					}
				}
			}
			return list.ToArray();
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002B54 File Offset: 0x00000D54
		private void OnSceneLoaded()
		{
			foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>().Where(delegate(GameObject x)
			{
				bool result;
				if (x.name == "HumanSNPC_Blacksmith")
				{
					Merchant componentInChildren = x.GetComponentInChildren<Merchant>();
					result = ((((componentInChildren != null) ? componentInChildren.ShopName : null) ?? "") == "Vyzyrinthrix the Blacksmith");
				}
				else
				{
					result = false;
				}
				return result;
			}))
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
							this.blessedLongswordRecipeInstance,
							this.divineLongswordRecipeInstance
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

		// Token: 0x0600000D RID: 13 RVA: 0x00002C8C File Offset: 0x00000E8C
		private void OnPackLoaded()
		{
			this.blessedLongswordInstance = BlessedLongsword.MakeItem();
			this.divineLongswordInstance = DivineLongsword.MakeItem();
			this.holyAvengerInstance = HolyAvenger.MakeItem();
			this.divineLongswordRecipeInstance = DivineLongsword.MakeRecipes();
			this.blessedLongswordRecipeInstance = BlessedLongsword.MakeRecipes();
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002CC8 File Offset: 0x00000EC8
		public static WeaponStats.AttackData MakeAttackData(float stamCost, float knockback, float attackSpeed, float damage, float stamCostMult, float knockbackMult)
		{
			return new WeaponStats.AttackData
			{
				StamCost = stamCost * stamCostMult,
				Knockback = knockback * knockbackMult,
				AttackSpeed = attackSpeed,
				Damage = new List<float>
				{
					damage * stamCostMult
				}
			};
		}

		// Token: 0x04000008 RID: 8
		public const string GUID = "com.ehaugw.holyavengeritempack";

		// Token: 0x04000009 RID: 9
		public const string VERSION = "2.0.0";

		// Token: 0x0400000A RID: 10
		public const string NAME = "Holy Avenger Item Pack";

		// Token: 0x0400000B RID: 11
		public const string sideloaderFolder = "HolyAvengerItemPack";

		// Token: 0x0400000C RID: 12
		public Item divineLongswordInstance;

		// Token: 0x0400000D RID: 13
		public Item blessedLongswordInstance;

		// Token: 0x0400000E RID: 14
		public Item holyAvengerInstance;

		// Token: 0x0400000F RID: 15
		public Item divineLongswordRecipeInstance;

		// Token: 0x04000010 RID: 16
		public Item blessedLongswordRecipeInstance;

		// Token: 0x04000011 RID: 17
		public GameObject gameObj;
	}
}
