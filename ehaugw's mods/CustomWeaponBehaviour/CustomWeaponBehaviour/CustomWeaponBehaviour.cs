using System;
using BepInEx;
using HarmonyLib;
using SideLoader;
using TinyHelper;
using UnityEngine;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000010 RID: 16
	[BepInPlugin("com.ehaugw.customweaponbehaviour", "Custom Weapon Behaviour", "2.0.0")]
	[BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("com.ehaugw.tinyhelper", "1.0.0")]
	public class CustomWeaponBehaviour : BaseUnityPlugin
	{
		// Token: 0x0600002C RID: 44 RVA: 0x00002A34 File Offset: 0x00000C34
		internal void Awake()
		{
			CustomWeaponBehaviour.Instance = this;
			Harmony harmony = new Harmony("com.ehaugw.customweaponbehaviour");
			harmony.PatchAll();
			SL.BeforePacksLoaded += this.BeforePacksLoaded;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002A6C File Offset: 0x00000C6C
		private void BeforePacksLoaded()
		{
			CustomWeaponBehaviour.BastardTag = TinyTagManager.GetOrMakeTag("Bastard");
			CustomWeaponBehaviour.FinesseTag = TinyTagManager.GetOrMakeTag("Finesse");
			CustomWeaponBehaviour.HolyWeaponTag = TinyTagManager.GetOrMakeTag("Holy");
			CustomWeaponBehaviour.HalfHandedTag = TinyTagManager.GetOrMakeTag("HalfHanded");
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002AAB File Offset: 0x00000CAB
		public static void ChangeGrip(Character character, Weapon.WeaponType toMoveset)
		{
			if (character != null)
			{
				Animator animator = character.Animator;
				if (animator != null)
				{
					animator.SetInteger("WeaponType", (int)toMoveset);
				}
			}
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002ACC File Offset: 0x00000CCC
		public static void ResetGrip(Character character)
		{
			Weapon weapon = (character != null) ? character.CurrentWeapon : null;
			bool flag = weapon != null;
			if (flag)
			{
				if (character != null)
				{
					Animator animator = character.Animator;
					if (animator != null)
					{
						animator.SetInteger("WeaponType", (int)weapon.Type);
					}
				}
			}
		}

		// Token: 0x04000002 RID: 2
		public const string GUID = "com.ehaugw.customweaponbehaviour";

		// Token: 0x04000003 RID: 3
		public const string VERSION = "2.0.0";

		// Token: 0x04000004 RID: 4
		public const string NAME = "Custom Weapon Behaviour";

		// Token: 0x04000005 RID: 5
		public static CustomWeaponBehaviour Instance;

		// Token: 0x04000006 RID: 6
		public static Tag BastardTag;

		// Token: 0x04000007 RID: 7
		public static Tag FinesseTag;

		// Token: 0x04000008 RID: 8
		public static Tag HolyWeaponTag;

		// Token: 0x04000009 RID: 9
		public static Tag HalfHandedTag;
	}
}
