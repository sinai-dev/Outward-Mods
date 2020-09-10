using System;
using System.Collections.Generic;
using SideLoader;

namespace HolyAvengerItemPack
{
	// Token: 0x02000002 RID: 2
	public class BlessedLongsword
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static Item MakeItem()
		{
			float[] array = new float[]
			{
				4f,
				25f,
				1.1f,
				35f
			};
			SL_Weapon sl_Weapon = new SL_Weapon
			{
				Name = "Blessed Longsword",
				Target_ItemID = 2000010,
				New_ItemID = 2501013,
				Description = "Augments lightning damage infusions that are applied to it.",
				Tags = HolyAvengerItemPack.GetSafeTags(new List<string>
				{
					"Finesse",
					"Holy",
					"Blade",
					"Weapon",
					"Item"
				}),
				StatsHolder = new SL_WeaponStats
				{
					BaseValue = new int?(1000),
					RawWeight = new float?(3.5f),
					MaxDurability = new int?(500),
					AttackSpeed = new float?(array[2]),
					BaseDamage = new List<SL_Damage>
					{
						new SL_Damage
						{
							Damage = array[3],
							Type = DamageType.Types.Physical
						}
					},
					Impact = new float?(array[1]),
					Attacks = new WeaponStats.AttackData[]
					{
						HolyAvengerItemPack.MakeAttackData(array[0], array[1], array[2], array[3], 1f, 1f),
						HolyAvengerItemPack.MakeAttackData(array[0], array[1], array[2], array[3], 1f, 1f),
						HolyAvengerItemPack.MakeAttackData(array[0], array[1], array[2] + 0.1f, array[3], 1.2f, 1.3f),
						HolyAvengerItemPack.MakeAttackData(array[0], array[1], array[2] + 0.1f, array[3], 1.1f, 1.1f),
						HolyAvengerItemPack.MakeAttackData(array[0], array[1], array[2] + 0.1f, array[3], 1.1f, 1.1f)
					}
				},
				ItemVisuals = new SL_ItemVisual
				{
					Prefab_Name = "keenlongsword_Prefab",
					Prefab_AssetBundle = "keenlongsword",
					Prefab_SLPack = "HolyAvengerItemPack"
				},
				SLPackName = "HolyAvengerItemPack",
				SubfolderName = "BlessedLongsword",
				SwingSound = new SwingSoundWeapon?(SwingSoundWeapon.Weapon_2H)
			};
			return CustomItems.CreateCustomItem(sl_Weapon.Target_ItemID, sl_Weapon.New_ItemID, "Blessed Longsword", sl_Weapon);
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002290 File Offset: 0x00000490
		public static Item MakeRecipes()
		{
			string text = "com.ehaugw.holyavengeritempack." + "BlessedLongsword".ToLower() + "recipe";
			new SL_Recipe
			{
				StationType = Recipe.CraftingType.Survival,
				Results = new List<SL_Recipe.ItemQty>
				{
					new SL_Recipe.ItemQty
					{
						Quantity = 1,
						ItemID = 2501013
					}
				},
				Ingredients = new List<SL_Recipe.Ingredient>
				{
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 2000160
					},
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 5300170
					},
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 2020091
					},
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 6600210
					}
				},
				UID = text
			}.ApplyRecipe();
			SL_RecipeItem sl_RecipeItem = new SL_RecipeItem
			{
				Name = "Crafting: Blessed Longsword",
				Target_ItemID = 5700024,
				New_ItemID = 2501026,
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				RecipeUID = text
			};
			return CustomItems.CreateCustomItem(sl_RecipeItem.Target_ItemID, sl_RecipeItem.New_ItemID, sl_RecipeItem.Name, sl_RecipeItem);
		}

		// Token: 0x04000001 RID: 1
		public const string SubfolderName = "BlessedLongsword";

		// Token: 0x04000002 RID: 2
		public const string ItemName = "Blessed Longsword";
	}
}
