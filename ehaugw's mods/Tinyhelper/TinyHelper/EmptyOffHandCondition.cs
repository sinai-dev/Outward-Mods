using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyHelper
{
	// Token: 0x02000003 RID: 3
	public class EmptyOffHandCondition : EffectCondition
	{
		// Token: 0x06000006 RID: 6 RVA: 0x00002178 File Offset: 0x00000378
		protected override bool CheckIsValid(Character _affectedCharacter)
		{
			int result;
			if (!(_affectedCharacter == null))
			{
				if (_affectedCharacter.LeftHandEquipment == null)
				{
					bool? flag;
					if (_affectedCharacter == null)
					{
						flag = null;
					}
					else
					{
						Weapon currentWeapon = _affectedCharacter.CurrentWeapon;
						flag = ((currentWeapon != null) ? new bool?(currentWeapon.TwoHanded) : null);
					}
					bool? flag2 = flag;
					if (!flag2.GetValueOrDefault())
					{
						goto IL_9D;
					}
				}
				if (!_affectedCharacter.Sheathed && this.AllowDrawnTwoHandedInRight)
				{
					Weapon currentWeapon2 = _affectedCharacter.CurrentWeapon;
					if (currentWeapon2 == null || currentWeapon2.TwoHandedRight)
					{
						goto IL_9D;
					}
				}
				if (_affectedCharacter.Sheathed && this.AllowSheathedTwoHandedInLeft)
				{
					Weapon currentWeapon3 = _affectedCharacter.CurrentWeapon;
					result = ((currentWeapon3 == null || currentWeapon3.TwoHandedRight) ? 1 : 0);
				}
				else
				{
					result = 0;
				}
				return result != 0;
			}
			IL_9D:
			result = 1;
			return result != 0;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002228 File Offset: 0x00000428
		public static Skill.ActivationCondition AddToSkill(Skill skill, bool allowDrawnTwoHandedInRight = false, bool allowSheathedTwoHandedInLeft = false)
		{
			GameObject gameObject = new GameObject("EmptyOffhandCondition");
			Skill.ActivationCondition activationCondition = new Skill.ActivationCondition();
			EmptyOffHandCondition emptyOffHandCondition = gameObject.AddComponent<EmptyOffHandCondition>();
			UnityEngine.Object.DontDestroyOnLoad(emptyOffHandCondition);
			gameObject.SetActive(false);
			activationCondition.Condition = emptyOffHandCondition;
			emptyOffHandCondition.AllowDrawnTwoHandedInRight = allowDrawnTwoHandedInRight;
			emptyOffHandCondition.AllowSheathedTwoHandedInLeft = allowSheathedTwoHandedInLeft;
			At.SetValue<string>("Requires an empty left hand.", typeof(Skill.ActivationCondition), activationCondition, "m_defaultMessage");
			Skill.ActivationCondition[] array = At.GetValue(typeof(Skill), skill, "m_additionalConditions") as Skill.ActivationCondition[];
			List<Skill.ActivationCondition> list = ((array != null) ? array.ToList<Skill.ActivationCondition>() : null) ?? new List<Skill.ActivationCondition>();
			list.Add(activationCondition);
			At.SetValue<Skill.ActivationCondition[]>(list.ToArray(), typeof(Skill), skill, "m_additionalConditions");
			return activationCondition;
		}

		// Token: 0x04000002 RID: 2
		public bool AllowDrawnTwoHandedInRight = false;

		// Token: 0x04000003 RID: 3
		public bool AllowSheathedTwoHandedInLeft = false;
	}
}
