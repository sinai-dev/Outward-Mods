using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using HarmonyLib;

namespace PvP
{
    public class BattleRoyale_Hooks
    {
        // =========== GAMEPLAY HOOKS ================

        [HarmonyPatch(typeof(Character), "Die")]
        public class Character_Die
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance)
            {
                var self = __instance;

                if (PvP.Instance.CurrentGame != PvP.GameModes.BattleRoyale)
                {
                    return;
                }

                if (!self.IsAI)
                {
                    //Debug.Log("Dropping player stuff!");
                    // custom player death (drop items)

                    if (!PhotonNetwork.isNonMasterClientInRoom && At.GetValue(typeof(Character), self, "m_lastDealers") is List<Pair<UID, float>> lastDealers)
                    {
                        float lowest = Time.time;
                        string uid = "";
                        foreach (var entry in lastDealers)
                        {
                            if (entry.Value < lowest)
                            {
                                lowest = entry.Value;
                                uid = entry.Key;
                            }
                        }
                        if (uid != "" && CharacterManager.Instance.GetCharacter(uid) is Character lastDealer)
                        {
                            lastDealers.Clear();
                            At.SetValue(lastDealers, typeof(Character), self, "m_lastDealers");
                            RPCManager.SendMessageToAll(lastDealer.Name + " has defeated " + self.Name);
                        }
                    }

                    foreach (EquipmentSlot equipslot in self.Inventory.Equipment.EquipmentSlots.Where(x => x != null && x.HasItemEquipped))
                    {
                        self.Inventory.DropItem(equipslot.EquippedItem.UID);
                    }
                }
                else
                {
                    if (self.Inventory == null || self.GetComponent<LootableOnDeath>() == null)
                    {
                        return;
                    }

                    if (!self.Inventory.Pouch || !self.Inventory.Pouch.FullyInitialized)
                    {
                        self.Inventory.ProcessStart();
                    }

                    // SPECIFIC TO MONSOON MAP

                    if (!PhotonNetwork.isNonMasterClientInRoom)
                    {
                        if (self.Name.ToLower().Contains("butcher"))
                        {
                            BattleRoyale.Instance.AddItemsToContainer(Templates.BR_Templates.Skills_High, 3, self.Inventory.Pouch.transform);
                            BattleRoyale.Instance.AddItemsToContainer(Templates.BR_Templates.Weapons_High, 1, self.Inventory.Pouch.transform);
                        }
                        else if (self.Name == "Immaculate" || self.name == "Shell Horror")
                        {
                            BattleRoyale.Instance.AddItemsToContainer(Templates.BR_Templates.Skills_High, 1, self.Inventory.Pouch.transform);
                            BattleRoyale.Instance.AddItemsToContainer(Templates.BR_Templates.Skills_Low, 2, self.Inventory.Pouch.transform);
                        }
                        else
                        {
                            BattleRoyale.Instance.AddItemsToContainer(Templates.BR_Templates.Skills_Low, 3, self.Inventory.Pouch.transform);
                        }
                    }
                }

                if (__instance.IsLocalPlayer)
                {
                    // begin spectate
                    __instance.OwnerPlayerSys.gameObject.AddComponent<Spectate>();
                }
            }
        }

        // ============= SKILL HOOKS ===============

        [HarmonyPatch(typeof(ItemContainer), "AddItem", new Type[] { typeof(Item), typeof(bool) })]
        public class ItemContainer_AddItem
        {
            [HarmonyPrefix]
            public static bool Prefix(ItemContainer __instance, Item _item, ref bool __result)
            {
                var self = __instance;

                if (PhotonNetwork.isNonMasterClientInRoom)
                {
                    return true;
                }

                if (BattleRoyale.Instance.IsGameplayStarting || PvP.Instance.CurrentGame == PvP.GameModes.BattleRoyale)
                {
                    if (_item is Skill && self.OwnerCharacter != null && !self.OwnerCharacter.IsAI)
                    {
                        if (!self.OwnerCharacter.Inventory.SkillKnowledge.IsItemLearned(_item.ItemID))
                            _item.ChangeParent(self.OwnerCharacter.Inventory.SkillKnowledge.transform);

                        __result = true;
                        return false;
                    }

                    if (_item is Weapon && self.OwnerCharacter != null)
                    {
                        if (Templates.Weapon_Skills.ContainsKey((int)(_item as Weapon).Type))
                        {
                            int id = Templates.Weapon_Skills[(int)(_item as Weapon).Type];
                            if (self.OwnerCharacter.Inventory.SkillKnowledge is CharacterSkillKnowledge skills && !skills.IsItemLearned(id))
                            {
                                Item skill = ItemManager.Instance.GenerateItemNetwork(id);
                                skill.ChangeParent(skills.transform);
                                List<Item> learnedItems = At.GetValue(typeof(CharacterKnowledge), skills, "m_learnedItems") as List<Item>;
                                learnedItems.Add(skill as Item);
                                At.SetValue(learnedItems, typeof(CharacterKnowledge), skills, "m_learnedItems");
                            }
                        }
                    }
                }

                return true;
            }
        }

        // ================ BUG FIXERS ================

        [HarmonyPatch(typeof(PlayerCharacterStats), "UpdateWeight")]
        public class PlayerCharacterStats_UpdateWeight
        {
            [HarmonyFinalizer]
            public static Exception Finalizer()
            {
                return null;
            }
        }
    }
}
