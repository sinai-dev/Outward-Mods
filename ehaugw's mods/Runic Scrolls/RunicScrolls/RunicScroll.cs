using System;
using System.Collections.Generic;
using SideLoader;

namespace RunicScrolls
{
	// Token: 0x02000005 RID: 5
	internal class RunicScroll
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000022EC File Offset: 0x000004EC
		public static Item MakeItem()
		{
			SL_Weapon sl_Weapon = new SL_Weapon
			{
				Name = "Runic Scroll",
				Target_ItemID = 5200001,
				New_ItemID = 2501017,
				Description = "A magic scroll inscribed with runes.\n\nA Runic Scroll can be used to cast a Rune spell without a lexicon, but it will be consumed in the process.",
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				Tags = new string[]
				{
					"Lexicon",
					"Item",
					"Consummable",
					"Misc"
				},
				EquipSlot = EquipmentSlot.EquipmentSlotIDs.Quiver,
				TwoHandType = Equipment.TwoHandedType.None,
				WeaponType = Weapon.WeaponType.Arrow,
				ItemVisuals = new SL_ItemVisual
				{
					Prefab_Name = "scrollquiver_Prefab",
					Prefab_AssetBundle = "scrollquiver",
					Prefab_SLPack = "RunicScrolls"
				},
				SpecialItemVisuals = new SL_ItemVisual
				{
					Prefab_Name = "scrollquiverequipped_Prefab",
					Prefab_AssetBundle = "scrollquiver",
					Prefab_SLPack = "RunicScrolls"
				},
				SLPackName = "RunicScrolls",
				SubfolderName = "RunicScroll"
			};
			return CustomItems.CreateCustomItem(sl_Weapon.Target_ItemID, sl_Weapon.New_ItemID, sl_Weapon.Name, sl_Weapon);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002424 File Offset: 0x00000624
		public static Item MakeRecipes()
		{
			string text = "com.ehaugw.runicscrolls." + "RunicScroll".ToLower() + "recipe";
			new SL_Recipe
			{
				StationType = Recipe.CraftingType.Alchemy,
				Results = new List<SL_Recipe.ItemQty>
				{
					new SL_Recipe.ItemQty
					{
						Quantity = 1,
						ItemID = 2501017
					}
				},
				Ingredients = new List<SL_Recipe.Ingredient>
				{
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 6400130
					},
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 6500090
					}
				},
				UID = text
			}.ApplyRecipe();

			SL_RecipeItem sl_RecipeItem = new SL_RecipeItem
			{
				Name = "Alchemy: Runic Scroll",
				Target_ItemID = 5700076,
				New_ItemID = 2501030,
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				RecipeUID = text
			};

			return CustomItems.CreateCustomItem(sl_RecipeItem.Target_ItemID, sl_RecipeItem.New_ItemID, sl_RecipeItem.Name, sl_RecipeItem);
		}

		// Token: 0x04000001 RID: 1
		public const string SubfolderName = "RunicScroll";

		// Token: 0x04000002 RID: 2
		public const string ItemName = "Runic Scroll";
	}
}
