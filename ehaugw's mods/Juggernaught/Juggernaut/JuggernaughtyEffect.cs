using System;
using SideLoader;

namespace Juggernaut
{
	// Token: 0x02000008 RID: 8
	internal class JuggernaughtyEffect : Effect
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002A34 File Offset: 0x00000C34
		public static void StaticActivate(Character character, object[] _infos, Effect instance)
		{
			Trainer trainer = new Trainer();
			At.SetValue<UID>(UID.Generate(), typeof(Trainer), trainer, "m_uid");
			At.SetValue<UID>(JuggernautMod.juggernautTreeInstance.UID, typeof(Trainer), trainer, "m_skillTreeUID");
			trainer.StartTraining(character);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002A8B File Offset: 0x00000C8B
		protected override void ActivateLocally(Character character, object[] _infos)
		{
			JuggernaughtyEffect.StaticActivate(character, _infos, this);
		}
	}
}
