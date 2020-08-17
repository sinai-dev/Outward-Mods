using System;
using BepInEx;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace Juggernaut
{
	// Token: 0x02000016 RID: 22
	[BepInPlugin("com.ehaugw.juggernautclass", "Juggernaut Class", "2.0.1")]
	[BepInDependency("com.sinai.SideLoader", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("com.ehaugw.tinyhelper", "1.0.0")]
	[BepInDependency("com.ehaugw.customweaponbehaviour", "2.0.0")]
	public class JuggernautMod : BaseUnityPlugin
	{
		// Token: 0x0600004C RID: 76 RVA: 0x00003868 File Offset: 0x00001A68
		internal void Awake()
		{
			GameObject gameObject = new GameObject("JuggernautRPC");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<Juggernaut.RPCManager>();
			SL.OnPacksLoaded += this.OnPackLoaded;
			SL.OnSceneLoaded += this.OnSceneLoaded;
			Harmony harmony = new Harmony("com.ehaugw.juggernautclass");
			harmony.PatchAll();
		}

		// Token: 0x0600004D RID: 77 RVA: 0x000038C8 File Offset: 0x00001AC8
		private void OnPackLoaded()
		{
			this.parryInstance = ParrySpell.Init();
			this.tackleInstance = TackleSpell.Init();
			this.bastardInstance = BastardSpell.Init();
			this.relentlessInstance = RelentlessSkill.Init();
			this.unyieldingInstance = UnyieldingSpell.Init();
			this.vengefulInstance = VengefulSpell.Init();
			this.fortifiedInstance = FortifiedSpell.Init();
			this.ruthlessInstance = RuthlessSpell.Init();
			this.juggernaughtyInstance = JuggernaughtySpell.Init();
			this.hordeBreakerInstance = HordeBreakerSpell.Init();
			this.warCryInstance = WarCrySpell.Init();
			JuggernautSkillTree.SetupSkillTree(ref JuggernautMod.juggernautTreeInstance);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x0000395A File Offset: 0x00001B5A
		private void OnSceneLoaded()
		{
			JuggernautTrainer.OnSceneChange();
		}

		// Token: 0x0400000C RID: 12
		public const string GUID = "com.ehaugw.juggernautclass";

		// Token: 0x0400000D RID: 13
		public const string VERSION = "2.0.1";

		// Token: 0x0400000E RID: 14
		public const string NAME = "Juggernaut Class";

		// Token: 0x0400000F RID: 15
		public Skill bastardInstance;

		// Token: 0x04000010 RID: 16
		public Skill parryInstance;

		// Token: 0x04000011 RID: 17
		public Skill tackleInstance;

		// Token: 0x04000012 RID: 18
		public Skill relentlessInstance;

		// Token: 0x04000013 RID: 19
		public Skill unyieldingInstance;

		// Token: 0x04000014 RID: 20
		public Skill vengefulInstance;

		// Token: 0x04000015 RID: 21
		public Skill fortifiedInstance;

		// Token: 0x04000016 RID: 22
		public Skill ruthlessInstance;

		// Token: 0x04000017 RID: 23
		public Skill juggernaughtyInstance;

		// Token: 0x04000018 RID: 24
		public Skill hordeBreakerInstance;

		// Token: 0x04000019 RID: 25
		public Skill warCryInstance;

		// Token: 0x0400001A RID: 26
		public static SkillSchool juggernautTreeInstance;

		// Token: 0x0400001B RID: 27
		public Trainer juggernautTrainer;
	}
}
