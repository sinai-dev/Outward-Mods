using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using SideLoader;

namespace MixedGrip
{
    public class CharacterInfo
    {
        public string CharacterUID = "";
        public string LastOffhandUID = "";
    }

    public class GripManager : Photon.MonoBehaviour
    {
        public static GripManager Instance;

        public static GameObject obj;

        public List<CharacterInfo> CurrentPlayers = new List<CharacterInfo>();
        
        internal void Awake()
        {
            Instance = this;
            
            var view = this.gameObject.AddComponent<PhotonView>();
            view.viewID = 901;
            Debug.Log("Registered MixedGrip with ViewID " + view.viewID);
        }

        internal void Update()
        {
            // make sure game is running
            if (Global.Lobby.PlayersInLobbyCount < 1 || NetworkLevelLoader.Instance.IsGameplayPaused) 
            { 
                if (CurrentPlayers.Count > 0)
                {
                    CurrentPlayers.Clear();
                }
                return; 
            }

            MixedGripUpdate();            
        }

        private void MixedGripUpdate()
        {
            if (Global.Lobby.PlayersInLobby.Where(x => x.ControlledCharacter.Initialized).Count() != CurrentPlayers.Count())
            {
                RefreshPlayerList();
            }
            else
            {
                if (CustomKeybindings.GetKeyDown(MixedGrip.TOGGLE_KEY, out int playerID))
                {
                    try
                    {
                        var character = SplitScreenManager.Instance.LocalPlayers[playerID].AssignedCharacter;

                        var info = CurrentPlayers.Where(it => it.CharacterUID == character.UID).FirstOrDefault();

                        ToggleGripHotkey(info, character);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Exception on player grip toggle: " + e.ToString());
                    }
                }

                foreach (CharacterInfo info in CurrentPlayers)
                {
                    if (CharacterManager.Instance.GetCharacter(info.CharacterUID) is Character c && c.IsLocalPlayer)
                    {
                        //UpdatePlayerInput(info, c);

                        UpdateCharacterSlots(info, c);
                    }
                }
            }
        }

        //private void UpdatePlayerInput(CharacterInfo charInfo, Character c)
        //{
        //    // grip hotkey
        //    if (CustomKeybindings.m_playerInputManager[c.OwnerPlayerSys.PlayerID].GetButtonDown(MixedGrip.TOGGLE_KEY))
        //    {
        //        ToggleGripHotkey(charInfo, c);
        //    }
        //}

        private void RefreshPlayerList()
        {
            // refresh list update
            CurrentPlayers.Clear();

            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby.Where(x => x.ControlledCharacter.Initialized))
            {
                Debug.Log("[MG]" + Time.time + " | InternalUpdate :: Added " + ps.ControlledCharacter.Name + " to CurrentPlayers");

                CharacterInfo charInfo = new CharacterInfo() { CharacterUID = ps.ControlledCharacter.UID };
                if (ps.ControlledCharacter.LeftHandEquipment)
                {
                    charInfo.LastOffhandUID = ps.ControlledCharacter.LeftHandEquipment.UID;
                }
                CurrentPlayers.Add(charInfo);
            }
        }

        private void UpdateCharacterSlots(CharacterInfo charInfo, Character c)
        {
            // bows and fist not supported currently
            if (c.CurrentWeapon != null && c.CurrentWeapon.Type == Weapon.WeaponType.Bow && c.CurrentWeapon.Type != Weapon.WeaponType.FistW_2H)
            {
                if (charInfo.LastOffhandUID != "") { charInfo.LastOffhandUID = ""; }
                return;
            }

            // update lastOffhand if new off-hand item
            if (c.LeftHandEquipment != null && charInfo.LastOffhandUID != c.LeftHandEquipment.UID)
            {
                charInfo.LastOffhandUID = c.LeftHandEquipment.UID;
            }
            // update previous off-hand item status
            else if (c.LeftHandEquipment == null && charInfo.LastOffhandUID != "" && ItemManager.Instance.GetItem(charInfo.LastOffhandUID) is Item lastOffhand)
            {
                if (lastOffhand.OwnerCharacter == null)
                {
                    // player no longer owns their previous off-hand item.
                    charInfo.LastOffhandUID = "";
                    return;
                }

                // automatic swap to 2H if we have no off-hand item.
                if (MixedGrip.AutoSwapOnEquipChange.Value && c.CurrentWeapon != null && c.CurrentWeapon.TwoHand == Equipment.TwoHandedType.None)
                {
                    SwapGrip(c, c.CurrentWeapon);
                }
            }
        }

        // ================  GRIP SWAPPING ================= //

        private void ToggleGripHotkey(CharacterInfo charInfo, Character c)
        {
            if (c.CurrentWeapon == null || c.CurrentWeapon.IsSummonedEquipment || c.CurrentWeapon.Type == Weapon.WeaponType.Bow || c.CurrentWeapon.Type == Weapon.WeaponType.FistW_2H)
            {
                return;
            }

            SwapGrip(c, c.CurrentWeapon);
            
            // re-equip last offhand
            if (!c.CurrentWeapon.TwoHanded && charInfo.LastOffhandUID != "" && ItemManager.Instance.GetItem(charInfo.LastOffhandUID) is Equipment lastOffhand)
            {
                At.Invoke(c.Inventory.Equipment, "EquipWithoutAssociating", new object[] { lastOffhand, false });
            }
        }

        // Local SwapGrip call. Just determines what the swap should do, then calls it via RPC.
        public void SwapGrip(Character c, Weapon weapon)
        {
            bool setTwoHanded = weapon.TwoHand == Equipment.TwoHandedType.None;
            int newWeaponType = MixedGrip.SwapAnimations.Value ? (int)GetSwappedType(weapon.Type) : (int)weapon.Type;

            // == send RPC swap grip ==
            photonView.RPC("SwapGripRPC", PhotonTargets.All, new object[] 
            { 
                weapon.UID, 
                c.UID.ToString(), 
                setTwoHanded, 
                newWeaponType, 
                MixedGrip.BalanceWeaponsOnSwap.Value 
            });
        }

        [PunRPC]
        public void SwapGripRPC(string weaponUID, string charUID, bool setTwoHanded, int newWeaponType, bool shouldFixStats)
        {
            if (CharacterManager.Instance.GetCharacter(charUID) is Character c && ItemManager.Instance.GetItem(weaponUID) is Weapon weapon)
            {
                // list of items to stop sync (might also add left hand equipment)
                List<Item> itemsToFix = new List<Item> { weapon };

                // set 2H type
                if (setTwoHanded)
                {
                    weapon.TwoHand = Equipment.TwoHandedType.TwoHandedRight;

                    if (c.LeftHandEquipment != null)
                    {
                        c.Inventory.UnequipItem(c.LeftHandEquipment);
                        itemsToFix.Add(c.LeftHandEquipment); // dont sync this (each player unequips locally for same character)
                    }
                }
                else
                {
                    weapon.TwoHand = Equipment.TwoHandedType.None;
                }

                // set Weapon.WeaponType
                weapon.Type = (Weapon.WeaponType)newWeaponType;

                // if we have it equipped, fix the left hand EquipmentSlot.m_lastEquippedItem
                if (c.Inventory.HasEquipped(weapon.ItemID))
                {
                    if (setTwoHanded)
                        At.SetField(c.Inventory.GetMatchingEquipmentSlot(EquipmentSlot.EquipmentSlotIDs.LeftHand), "m_lastEquippedItem", weapon);
                    else
                        At.SetField(c.Inventory.GetMatchingEquipmentSlot(EquipmentSlot.EquipmentSlotIDs.LeftHand), "m_lastEquippedItem", null);
                }
                
                c.Animator.SetInteger("WeaponType", newWeaponType); // fix the character animator
                if (shouldFixStats) { SetWeaponStats(weapon); }     // fix stats                
                StopItemSync(itemsToFix);                           // fix item sync
            } 
        }

        // fix and override Item Sync
        private void StopItemSync(List<Item> items)
        {
            if (items == null || items.Count() == 0) { return; }

            if (At.GetField(ItemManager.Instance, "m_itemToSyncToClient") is HashSet<string> masterToClientDict
                && At.GetField(ItemManager.Instance, "m_itemToSyncToMaster") is HashSet<string> clientToMasterDict)
            {
                foreach (Item item in items)
                {
                    if (item == null) { continue; }
                    At.SetField(item, "m_sendHierarchyRequired", false);
                    masterToClientDict.RemoveWhere(x => x == item.UID);
                    clientToMasterDict.RemoveWhere(x => x == item.UID);
                }

                //At.SetField(ItemManager.Instance, "m_itemToSyncToClient", masterToClientDict);
                //At.SetField(ItemManager.Instance, "m_itemToSyncToMaster", clientToMasterDict);
            }
        }

        

        // =================== STAT HELPERS, ETC ====================== //

        public Weapon.WeaponType GetSwappedType(Weapon.WeaponType type)
        {
            switch (type)
            {
                case Weapon.WeaponType.Sword_1H:
                    type = Weapon.WeaponType.Sword_2H;
                    break;
                case Weapon.WeaponType.Sword_2H:
                    type = Weapon.WeaponType.Sword_1H;
                    break;
                case Weapon.WeaponType.Axe_1H:
                    type = Weapon.WeaponType.Axe_2H;
                    break;
                case Weapon.WeaponType.Axe_2H:
                    type = Weapon.WeaponType.Axe_1H;
                    break;
                case Weapon.WeaponType.Mace_1H:
                    type = Weapon.WeaponType.Mace_2H;
                    break;
                case Weapon.WeaponType.Mace_2H:
                    type = Weapon.WeaponType.Mace_1H;
                    break;
                default:
                    break;
            }
            return type;
        }

        private void SetWeaponStats(Weapon newWeapon)
        {
            Weapon origWeapon = ResourcesPrefabManager.Instance.GetItemPrefab(newWeapon.ItemID) as Weapon;

            // if returning to the orig type, just use the orig item stats.
            if (origWeapon.TwoHand == newWeapon.TwoHand)
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(origWeapon.Stats), newWeapon.Stats);
                At.SetField(newWeapon, "m_baseDamage", newWeapon.Stats.BaseDamage);
                At.Invoke(newWeapon, "RefreshEnchantmentModifiers");
            }
            else
            {
                WeaponStats newStats = new WeaponStats();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(origWeapon.Stats), newStats);

                float adjustedSpeed = GetWeaponBalance(origWeapon, newWeapon);
                float damageMultiplier = 1.04f * adjustedSpeed;
                if (newWeapon.Type == Weapon.WeaponType.Spear_2H || newWeapon.Type == Weapon.WeaponType.Halberd_2H)
                {
                    damageMultiplier = 0.90f;
                }


                // set visible damage
                for (int i = 0; i < newStats.BaseDamage.List.Count; i++)
                {
                    if (newStats.BaseDamage.List[i].Damage > 0)
                    {
                        newStats.BaseDamage.List[i].Damage *= damageMultiplier;
                        newStats.BaseDamage.List[i].Damage = Mathf.Round(newStats.BaseDamage.List[i].Damage);
                    }
                }

                newStats.Impact *= damageMultiplier;
                newStats.Impact = Mathf.Round(newStats.Impact);

                // set attack speed
                adjustedSpeed += newStats.AttackSpeed;
                newStats.AttackSpeed = (float)Math.Round(100 * adjustedSpeed * 0.01f * 0.5f, 2); // average of "fixed" speed and current speed.

                for (int i = 0; i < newStats.Attacks.Count(); i++)
                {
                    newStats.Attacks[i].Knockback *= damageMultiplier;

                    for (int j = 0; j < newStats.Attacks[i].Damage.Count(); j++)
                    {
                        newStats.Attacks[i].Damage[j] *= damageMultiplier;
                    }
                }

                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(newStats), newWeapon.Stats);
                At.SetField(newWeapon, "m_baseDamage", newStats.BaseDamage);
                At.Invoke(newWeapon, "RefreshEnchantmentModifiers");
            }
        }

        public float GetWeaponBalance(Weapon originalWeapon, Weapon newWeapon)
        {
            if (originalWeapon.Type == Weapon.WeaponType.Spear_2H || originalWeapon.Type == Weapon.WeaponType.Halberd_2H)
            {
                return 0.9f * originalWeapon.Stats.AttackSpeed;
            }

            return 100f / (float)((decimal)weaponSpeeds[originalWeapon.Type] / (decimal)weaponSpeeds[newWeapon.Type]) / 100;
        }

        public Dictionary<Weapon.WeaponType, float> weaponSpeeds = new Dictionary<Weapon.WeaponType, float>
        {
            { Weapon.WeaponType.Sword_1H,   1.251f},
            { Weapon.WeaponType.Axe_1H,     1.399f},
            { Weapon.WeaponType.Mace_1H,    1.629f},
            { Weapon.WeaponType.Sword_2H,   1.710f},
            { Weapon.WeaponType.Axe_2H,     1.667f},
            { Weapon.WeaponType.Mace_2H,    2.036f},
            { Weapon.WeaponType.Spear_2H,   1.499f},
            { Weapon.WeaponType.Halberd_2H, 1.612f}
        };
    }

    [HarmonyPatch(typeof(CharacterEquipment), "EquipItem", new Type[] { typeof(Equipment), typeof(bool) })]
    public class CharacterEquipment_EquipItem
    {
        [HarmonyPrefix]
        public static bool Prefix(CharacterEquipment __instance, Equipment _itemToEquip, bool _playAnim = false)
        {
            var self = __instance;

            Character c = At.GetField(self, "m_character") as Character;

            if (!MixedGrip.AutoSwapOnEquipChange.Value
                || (_itemToEquip is Weapon weapon && (weapon.Type == Weapon.WeaponType.Bow || weapon.Type == Weapon.WeaponType.FistW_2H || weapon.IsSummonedEquipment))
                || (c.CurrentWeapon != null && (c.CurrentWeapon.Type == Weapon.WeaponType.Bow || c.CurrentWeapon.Type == Weapon.WeaponType.FistW_2H || c.CurrentWeapon.IsSummonedEquipment)))
            {
                return true;
            }

            bool anySwap = false;
            if (!c.IsAI && ((int)_itemToEquip.EquipSlot == 5 || (int)_itemToEquip.EquipSlot == 6))
            {
                if (_itemToEquip.TwoHanded && c.LeftHandEquipment != null)
                {
                    // we are equipping a 2H weapon but we currently have an off-hand item, swap the grip of the weapon first.
                    anySwap = true;

                    GripManager.Instance.SwapGrip(c, _itemToEquip as Weapon);
                    At.Invoke(c.Inventory.Equipment, "EquipWithoutAssociating", new object[] { _itemToEquip, false });
                }
                else if (_itemToEquip.EquipSlot == EquipmentSlot.EquipmentSlotIDs.LeftHand && c.CurrentWeapon != null && c.CurrentWeapon.TwoHanded)
                {
                    // we are equipping an off-hand item but our current weapon is 2H. swap our weapon to 1H first.
                    anySwap = true;

                    GripManager.Instance.SwapGrip(c, c.CurrentWeapon);

                    // set the offhand to our C.LeftHandEquipment now to avoid problems with autoswapping
                    At.SetField(c, "m_leftHandEquipment", _itemToEquip as Equipment);
                    At.Invoke(c.Inventory.Equipment, "EquipWithoutAssociating", new object[] { _itemToEquip, false });
                }
            }

            if (!anySwap)
            {
                return true;
            }

            return false;
        }
    }

}
