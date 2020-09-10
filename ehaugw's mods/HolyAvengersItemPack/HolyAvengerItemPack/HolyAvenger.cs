using System;
using System.Collections.Generic;
using SideLoader;
using UnityEngine;

namespace HolyAvengerItemPack
{
	// Token: 0x02000003 RID: 3
	public class HolyAvenger
	{
		// Token: 0x06000004 RID: 4 RVA: 0x000023D8 File Offset: 0x000005D8
		public static Item MakeItem()
		{
			float[] array = new float[]
			{
				4.8f,
				25f,
				0.9f,
				48f
			};
			SL_Weapon sl_Weapon = new SL_Weapon
			{
				Name = "Holy Avenger",
				Target_ItemID = 2000010,
				New_ItemID = 2501029,
				Description = "Augments lightning damage infusions that are applied to it and inflicts Doomed on enemies.",
				Tags = HolyAvengerItemPack.GetSafeTags(new List<string>
				{
					"Bastard",
					"Holy",
					"Blade",
					"Weapon",
					"Item"
				}),
				StatsHolder = new SL_WeaponStats
				{
					BaseValue = new int?(2000),
					RawWeight = new float?(4.5f),
					MaxDurability = new int?(-1),
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
				SwingSound = new SwingSoundWeapon?(SwingSoundWeapon.Weapon_2H),
				SLPackName = "HolyAvengerItemPack",
				SubfolderName = "HolyAvenger",
				ItemVisuals = new SL_ItemVisual
				{
					Prefab_Name = "holyavenger_Prefab",
					Prefab_AssetBundle = "holyavenger",
					Prefab_SLPack = "HolyAvengerItemPack",
					PositionOffset = new Vector3?(new Vector3(-0.03f, 0f, 0f))
				}
			};
			return CustomItems.CreateCustomItem(sl_Weapon.Target_ItemID, sl_Weapon.New_ItemID, sl_Weapon.Name, sl_Weapon);
		}

		// Token: 0x04000003 RID: 3
		public const string SubfolderName = "HolyAvenger";

		// Token: 0x04000004 RID: 4
		public const string ItemName = "Holy Avenger";

		// Token: 0x04000005 RID: 5
		private static float[] weaponData = new float[]
		{
			4.8f,
			25f,
			0.9f,
			48f
		};
	}
}
