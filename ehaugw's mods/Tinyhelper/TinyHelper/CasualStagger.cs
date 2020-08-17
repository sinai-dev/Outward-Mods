using System;

namespace TinyHelper
{
	// Token: 0x0200000B RID: 11
	public class CasualStagger : Effect
	{
		// Token: 0x06000022 RID: 34 RVA: 0x00002C7A File Offset: 0x00000E7A
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			CasualStagger.Stagger(_affectedCharacter);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002C84 File Offset: 0x00000E84
		public static void Stagger(Character character)
		{
			At.SetValue<Character.HurtType>(Character.HurtType.Knockback, typeof(Character), character, "m_hurtType");
			character.Animator.SetTrigger("Knockback");
			character.ForceCancel(false, true);
			character.Invoke("DelayedForceCancel", 0.3f);
			bool flag = character.CharacterUI;
			if (flag)
			{
				character.CharacterUI.OnPlayerKnocked();
			}
		}
	}
}
