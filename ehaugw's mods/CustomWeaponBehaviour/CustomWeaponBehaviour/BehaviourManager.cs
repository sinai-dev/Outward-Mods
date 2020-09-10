using System;
using System.Threading.Tasks;
using SideLoader;
using UnityEngine;

namespace CustomWeaponBehaviour
{
	// Token: 0x02000002 RID: 2
	public class BehaviourManager
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static Weapon.WeaponType? GetBastardType(Weapon.WeaponType type)
		{
			Weapon.WeaponType? result;
			switch (type)
			{
			case Weapon.WeaponType.Sword_1H:
				result = new Weapon.WeaponType?(Weapon.WeaponType.Sword_2H);
				break;
			case Weapon.WeaponType.Axe_1H:
				result = new Weapon.WeaponType?(Weapon.WeaponType.Axe_2H);
				break;
			case Weapon.WeaponType.Mace_1H:
				result = new Weapon.WeaponType?(Weapon.WeaponType.Mace_2H);
				break;
			default:
				result = new Weapon.WeaponType?(type);
				break;
			}
			return result;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020A0 File Offset: 0x000002A0
		public static Weapon.WeaponType GetCurrentAnimationType(Weapon weapon)
		{
			Animator animator = null;
			bool flag;
			if (weapon.IsEquipped)
			{
				Character ownerCharacter = weapon.OwnerCharacter;
				animator = ((ownerCharacter != null) ? ownerCharacter.Animator : null);
				flag = (animator != null);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			Weapon.WeaponType result;
			if (flag2)
			{
				result = (Weapon.WeaponType)animator.GetInteger("WeaponType");
			}
			else
			{
				result = weapon.Type;
			}
			return result;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000020F0 File Offset: 0x000002F0
		public static bool IsBastardMode(Weapon weapon)
		{
			bool isEquipped = weapon.IsEquipped;
			if (isEquipped)
			{
				bool flag;
				if (weapon.HasTag(CustomWeaponBehaviour.BastardTag))
				{
					Character ownerCharacter = weapon.OwnerCharacter;
					if (((ownerCharacter != null) ? ownerCharacter.LeftHandEquipment : null) == null)
					{
						flag = !weapon.TwoHanded;
						goto IL_3F;
					}
				}
				flag = false;
				IL_3F:
				bool flag2 = flag;
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000214C File Offset: 0x0000034C
		public static bool IsHalfHandedMode(Weapon weapon)
		{
			bool isEquipped = weapon.IsEquipped;
			if (isEquipped)
			{
				bool flag;
				if (weapon.HasTag(CustomWeaponBehaviour.HalfHandedTag))
				{
					Character ownerCharacter = weapon.OwnerCharacter;
					if (((ownerCharacter != null) ? ownerCharacter.LeftHandEquipment : null) == null)
					{
						flag = !weapon.TwoHanded;
						goto IL_3F;
					}
				}
				flag = false;
				IL_3F:
				bool flag2 = flag;
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000021A8 File Offset: 0x000003A8
		public static bool IsHolyWeaponMode(Weapon weapon)
		{
			bool flag = weapon.HasTag(CustomWeaponBehaviour.HolyWeaponTag) && weapon.Imbued;
			if (flag)
			{
				foreach (Effect effect in weapon.FirstImbue.ImbuedEffects)
				{
					bool flag2 = effect is WeaponDamage && ((WeaponDamage)effect).Damages[0].Type == DamageType.Types.Electric;
					if (flag2)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002228 File Offset: 0x00000428
		public static bool IsComboAttack(Character __instance, int _type, int _id)
		{
			bool flag = _type == 0;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = _type == 1;
				result = (flag2 && __instance.NextAtkAllowed == 2);
			}
			return result;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000225C File Offset: 0x0000045C
		public static bool AdaptGrip(Character __instance, ref int _type, ref int _id)
		{
			Weapon weapon = (__instance != null) ? __instance.CurrentWeapon : null;
			bool flag = weapon == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = _type == 2502;
				if (flag2)
				{
					MeleeSkill meleeSkill = At.GetValue(typeof(Character), __instance, "m_lastUsedSkill") as MeleeSkill;
					bool flag3 = meleeSkill != null && meleeSkill.ActivateEffectAnimType == Character.SpellCastType.WeaponSkill1;
					if (flag3)
					{
						bool flag4 = meleeSkill.RequiredWeaponTypes[0] != weapon.Type;
						if (flag4)
						{
							CustomWeaponBehaviour.ChangeGrip(__instance, meleeSkill.RequiredWeaponTypes[0]);
							return true;
						}
					}
				}
				bool flag5 = weapon.HasTag(CustomWeaponBehaviour.FinesseTag);
				if (flag5)
				{
					bool flag6 = _type == 1 && !BehaviourManager.IsComboAttack(__instance, _type, _id);
					if (flag6)
					{
						CustomWeaponBehaviour.ChangeGrip(__instance, Weapon.WeaponType.Axe_1H);
						return true;
					}
				}
				bool flag7 = BehaviourManager.IsHalfHandedMode(weapon);
				if (flag7)
				{
					bool flag8 = _type == 1 && !BehaviourManager.IsComboAttack(__instance, _type, _id);
					if (flag8)
					{
						CustomWeaponBehaviour.ChangeGrip(__instance, Weapon.WeaponType.Spear_2H);
						return true;
					}
				}
				bool flag9 = BehaviourManager.IsBastardMode(weapon);
				if (flag9)
				{
					Weapon.WeaponType? bastardType = BehaviourManager.GetBastardType(weapon.Type);
					Weapon.WeaponType valueOrDefault = 0;
					bool flag10;
					if (bastardType != null)
					{
						valueOrDefault = bastardType.GetValueOrDefault();
						flag10 = true;
					}
					else
					{
						flag10 = false;
					}
					bool flag11 = flag10;
					if (flag11)
					{
						bool flag12 = _type != 2502;
						if (flag12)
						{
							CustomWeaponBehaviour.ChangeGrip(__instance, valueOrDefault);
							return true;
						}
					}
				}
				result = false;
			}
			return result;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000023DE File Offset: 0x000005DE
		internal static Task<object> Delay(int v)
		{
			throw new NotImplementedException();
		}
	}
}
