using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using SideLoader;
using UnityEngine;

namespace TsarRing
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class TsarRing : BaseUnityPlugin
    {
        public const string GUID = "com.sinai.tsarring";
        public const string NAME = "Tsar Ring";
        public const string VERSION = "1.0.0";

        public const int TSAR_RING_ITEMID = 5500999;

        internal void Awake()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            SL.BeforePacksLoaded += SL_BeforePacksLoaded;
            SL.OnPacksLoaded += SL_OnPacksLoaded;
        }

        private void SL_BeforePacksLoaded()
        {
            CustomTags.CreateTag("Ring");
        }

        private void SL_OnPacksLoaded()
        {
            var ringItem = new SL_Equipment
            {
                Target_ItemID = 5100500,
                New_ItemID = TSAR_RING_ITEMID,
                Name = "Tsar Ring",
                Description = "An ornately-decorated ring, jeweled with an impressive Tsar.\n\nSome say this ring was summoned by the Creator of Aurai itself.",
                IKType = Equipment.IKMode.None,
                EquipSlot = EquipmentSlot.EquipmentSlotIDs.LeftHand,
                Tags = new string[] { "Ring" },
                StatsHolder = new SL_EquipmentStats
                {
                    BaseValue = 999,
                    MaxDurability = -1,
                    RawWeight = 0.1f,
                    Mana_Use_Modifier = -5f
                },
                EffectBehaviour = EffectBehaviours.DestroyEffects,

                SLPackName = "TsarRing",
                SubfolderName = "TsarRing",

                ItemVisuals = new SL_ItemVisual
                {
                    Prefab_SLPack = "TsarRing",
                    Prefab_AssetBundle = "tsarring",
                    Prefab_Name = "mdl_itm_Tsar_Ring_v",

                    Position = new Vector3(-0.018f, 0.011f, 0.037f),
                    Rotation = new Vector3(347.014f, 357.770f, 0f),
                },
            };

            CustomItems.CreateCustomItem(ringItem);

            var ringEffects = new SL_EffectTransform[]
            {
                new SL_EffectTransform
                {
                    TransformName = "ActivationEffects",
                    Effects = new SL_Effect[]
                    {
                        new SL_PlaySoundEffect
                        {
                            SyncType = Effect.SyncTypes.OwnerSync,
                            Sounds = new List<GlobalAudioManager.Sounds>
                            {
                                GlobalAudioManager.Sounds.CS_Mantis_ManaBlast_Whoosh1
                            }
                        }
                    }
                },
                new SL_EffectTransform
                {
                    TransformName = "Effects",
                    Effects = new SL_Effect[]
                    {
                        new SL_ShootConeBlast
                        {
                            BaseBlast = SL_ShootBlast.BlastPrefabs.EliteImmaculateLaser,
                            CastPosition = Shooter.CastPositionType.Transform,
                            NoAim = true,
                            TargetType = Shooter.TargetTypes.Enemies,
                            TransformName = "ShooterTransform",
                            Radius = 0.8f,
                            RefreshTime = 0.1f,
                            BlastLifespan = 1.0f,
                            InstantiatedAmount = 1,
                            Interruptible = true,
                            MaxHitTargetCount = -1,
                            HitOnShoot = true,
                            NoTargetForwardMultiplier = 5.0f,
                            ImpactSoundMaterial = EquipmentSoundMaterials.Fire,
                            EffectBehaviour = EffectBehaviours.OverrideEffects,
                            BlastEffects = new SL_EffectTransform[]
                            {
                                new SL_EffectTransform
                                {
                                    TransformName = "Effects",
                                    Effects = new SL_Effect[]
                                    {
                                        new SL_PunctualDamage
                                        {
                                            Damage = new List<SL_Damage>
                                            {
                                                new SL_Damage
                                                {
                                                    Type = DamageType.Types.Ethereal,
                                                    Damage = 5.0f,
                                                }
                                            },
                                            Knockback = 2f,
                                            HitInventory = true
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var ringSkill = new SL_AttackSkill
            {
                Target_ItemID = 8300251,
                New_ItemID = 8500999,
                Name = "Ring Laser",
                Description = "Requires: Tsar Ring\n\nInvoke the power of the ring, disintegrating anything in it's path.",
                RequiredWeaponTags = new string[] { "Ring" },
                Cooldown = 10,
                ManaCost = 10,
                StaminaCost = 0,
                DurabilityCost = 0,
                DurabilityCostPercent = 0,
                CastType = Character.SpellCastType.Flamethrower,
                EffectBehaviour = EffectBehaviours.OverrideEffects,
                StatsHolder = null,

                SLPackName = "TsarRing",
                SubfolderName = "RingLaser",

                EffectTransforms = ringEffects
            };

            CustomItems.CreateCustomItem(ringSkill);
        }
    }
}
