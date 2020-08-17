using System;
using SideLoader;
using UnityEngine;

namespace Templar
{
	// Token: 0x02000010 RID: 16
	internal class PrayerEffect : Effect
	{
		// Token: 0x0600001E RID: 30 RVA: 0x000036BC File Offset: 0x000018BC
		protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
		{
			bool flag = false;
			StatusEffect parentStatusEffect = this.m_parentStatusEffect;
			bool flag2 = parentStatusEffect != null;
			if (flag2)
			{
				bool flag3 = parentStatusEffect.Age > 10f && !this.buffsWereReceived;
				if (flag3)
				{
					bool flag4 = !_affectedCharacter.StatusEffectMngr.HasStatusEffect(Templar.Instance.prayerCooldownStatusEffectInstance.IdentifierName);
					if (flag4)
					{
						_affectedCharacter.StatusEffectMngr.AddStatusEffect(ResourcesPrefabManager.Instance.GetStatusEffectPrefab("Bless"), _affectedCharacter);
						_affectedCharacter.StatusEffectMngr.AddStatusEffect(Templar.Instance.prayerCooldownStatusEffectInstance, _affectedCharacter);
						bool flag5 = _affectedCharacter.Inventory.SkillKnowledge.IsItemLearned(2502002);
						if (flag5)
						{
							_affectedCharacter.StatusEffectMngr.AddStatusEffect(Templar.Instance.burstOfDivinityInstance, _affectedCharacter);
						}
					}
					else
					{
						bool isLocalPlayer = _affectedCharacter.IsLocalPlayer;
						if (isLocalPlayer)
						{
							_affectedCharacter.CharacterUI.ShowInfoNotification(Templar.Instance.prayerCooldownStatusEffectInstance.Description);
						}
					}
					this.buffsWereReceived = true;
				}
				bool flag6 = parentStatusEffect.Age > 10f && !this.trainerWasOpened;
				if (flag6)
				{
					bool isLocalPlayer2 = _affectedCharacter.IsLocalPlayer;
					if (isLocalPlayer2)
					{
						bool flag7 = false;
						Vector3 position = _affectedCharacter.transform.position;
						Vector3 eulerAngles = _affectedCharacter.transform.rotation.eulerAngles;
						string activeSceneName = SceneManagerHelper.ActiveSceneName;
						string text = activeSceneName;
						if (text != null)
						{
							if (!(text == "Chersonese_Dungeon4_HolyMission"))
							{
								if (text == "Monsoon")
								{
									bool flag8 = position.x < -174f && (double)position.x > -176.5 && position.z < 755f && position.z > 753f;
									if (flag8)
									{
										bool flag9 = eulerAngles.y < 24f || eulerAngles.y > 330f;
										if (flag9)
										{
											flag7 = true;
										}
									}
									bool flag10 = position.x < -374f && (double)position.x > -375.8 && (double)position.z < 766.5 && (double)position.z > 764.5;
									if (flag10)
									{
										bool flag11 = eulerAngles.y < 190f && eulerAngles.y > 140f;
										if (flag11)
										{
											flag7 = true;
										}
									}
								}
							}
							else
							{
								bool flag12 = position.x < -12.5f && (double)position.x > -16.5 && position.z < -18f && position.z > -20f;
								if (flag12)
								{
									bool flag13 = eulerAngles.y < 10f || eulerAngles.y > 320f;
									if (flag13)
									{
										flag7 = true;
									}
								}
							}
						}
						bool flag14 = flag7;
						if (flag14)
						{
							Trainer trainer = new Trainer();
							At.SetValue<UID>(UID.Generate(), typeof(Trainer), trainer, "m_uid");
							At.SetValue<UID>(Templar.templarTreeInstance.UID, typeof(Trainer), trainer, "m_skillTreeUID");
							trainer.StartTraining(_affectedCharacter);
							this.trainerWasOpened = true;
						}
					}
				}
				bool flag15 = (double)((_affectedCharacter != null) ? _affectedCharacter.AnimMoveSqMagnitude : 0f) > 0.1 && parentStatusEffect.Age > 0.5f;
				if (flag15)
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			bool flag16 = flag;
			if (flag16)
			{
				StatusEffectManager statusEffectMngr = _affectedCharacter.StatusEffectMngr;
				if (statusEffectMngr != null)
				{
					statusEffectMngr.CleanseStatusEffect("Prayer");
				}
				_affectedCharacter.ForceCancel(true, true);
			}
		}

		// Token: 0x04000003 RID: 3
		private bool buffsWereReceived = false;

		// Token: 0x04000004 RID: 4
		private bool trainerWasOpened = false;
	}
}
