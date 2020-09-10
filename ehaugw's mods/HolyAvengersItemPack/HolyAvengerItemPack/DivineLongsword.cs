using System;
using System.Collections.Generic;
using SideLoader;

namespace HolyAvengerItemPack
{
	// Token: 0x02000004 RID: 4
	public class DivineLongsword
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000026B0 File Offset: 0x000008B0
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
				Name = "Divine Longsword",
				Target_ItemID = 2000010,
				New_ItemID = 2501012,
				Description = "Augments lightning damage infusions that are applied to it and inflicts Doomed on enemies.",
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
					BaseValue = new int?(1500),
					RawWeight = new float?(3.5f),
					MaxDurability = new int?(650),
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
				EffectTransforms = new SL_EffectTransform[]
				{
					new SL_EffectTransform
					{
						TransformName = "HitEffects",
						Effects = new SL_Effect[]
						{
							new SL_AddStatusEffectBuildUp
							{
								StatusEffect = "Doom",
								Buildup = 60f
							}
						}
					}
				},
				ItemVisuals = new SL_ItemVisual
				{
					Prefab_Name = "keenlongsword_Prefab",
					Prefab_AssetBundle = "keenlongsword",
					Prefab_SLPack = "HolyAvengerItemPack"
				},
				SLPackName = "HolyAvengerItemPack",
				SubfolderName = "DivineLongsword",
				SwingSound = new SwingSoundWeapon?(SwingSoundWeapon.Weapon_2H),
				LegacyItemID = new int?(2501029)
			};
			return CustomItems.CreateCustomItem(sl_Weapon.Target_ItemID, sl_Weapon.New_ItemID, sl_Weapon.Name, sl_Weapon);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002950 File Offset: 0x00000B50
		public static Item MakeRecipes()
		{
			string text = "com.ehaugw.holyavengeritempack." + "DivineLongsword".ToLower() + "recipe";
			new SL_Recipe
			{
				StationType = Recipe.CraftingType.Survival,
				Results = new List<SL_Recipe.ItemQty>
				{
					new SL_Recipe.ItemQty
					{
						Quantity = 1,
						ItemID = 2501012
					}
				},
				Ingredients = new List<SL_Recipe.Ingredient>
				{
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 2501013
					},
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 6600224
					},
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 6600222
					},
					new SL_Recipe.Ingredient
					{
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						Ingredient_ItemID = 6600223
					}
				},
				UID = text
			}.ApplyRecipe();
			SL_RecipeItem sl_RecipeItem = new SL_RecipeItem
			{
				Name = "Crafting: Divine Longsword",
				Target_ItemID = 5700024,
				New_ItemID = 2501027,
				EffectBehaviour = EffectBehaviours.OverrideEffects,
				RecipeUID = text
			};
			return CustomItems.CreateCustomItem(sl_RecipeItem.Target_ItemID, sl_RecipeItem.New_ItemID, sl_RecipeItem.Name, sl_RecipeItem);
		}

		// Token: 0x04000006 RID: 6
		public const string SubfolderName = "DivineLongsword";

		// Token: 0x04000007 RID: 7
		public const string ItemName = "Divine Longsword";
	}
}
