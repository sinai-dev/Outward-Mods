using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SideLoader;
using UnityEngine;

namespace BlacksmithsToolbox
{
    // declare a custom Effect class.
    // To use this, we attach this class as a Component to the "Effects" gameobject on our desired item. (this is done in ToolboxGlobal.cs)

    public class ToolboxEffect : Effect
    {
        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            Character c = _affectedCharacter;

            if (!c.IsLocalPlayer) { return; }

            if (BlacksmithsToolbox.settings.Iron_Scrap_Cost > 0 && !c.Inventory.OwnsItem(6400140, BlacksmithsToolbox.settings.Iron_Scrap_Cost))
            {
                SendUIMessage(c, string.Format("You need {0} Iron Scrap to do that!", BlacksmithsToolbox.settings.Iron_Scrap_Cost));
            }
            else
            {
                // find the toolbox with the lowest durability (and make sure we have one with any durability)
                bool brokenToolbox = true;
                Item lowestDurBox = null;
                float currentDur = float.MaxValue;

                foreach (Item item in c.Inventory.GetOwnedItems(BlacksmithsToolbox.TOOLBOX_ID))
                {
                    if (item.CurrentDurability > 0 && item.CurrentDurability < currentDur)
                    {
                        // if we reach this, we must have at least one toolbox with some durability
                        brokenToolbox = false;

                        // this is also the new lowest durability we have found (or the first one)
                        lowestDurBox = item;
                        currentDur = item.CurrentDurability;
                    }
                }

                if (brokenToolbox)
                {
                    SendUIMessage(c, string.Format("Your Toolbox is broken!"));
                    return;
                }

                // remove scrap(s)                
                c.Inventory.RemoveItem(6400140, BlacksmithsToolbox.settings.Iron_Scrap_Cost);

                // repair everything. this will also repair our toolbox.
                c.Inventory.RepairEverything();

                // fix the toolbox durability. effectively this forces there to only ever be 1 toolbox that is used at a time, until it reaches 0 durability.
                At.SetField(lowestDurBox, "m_currentDurability", currentDur - BlacksmithsToolbox.settings.Durability_Cost_Per_Use);

                if (lowestDurBox.CurrentDurability <= 0)
                {
                    ItemManager.Instance.DestroyItem(lowestDurBox.UID);
                    //ItemManager.Instance.ConsumeCraftingItems(new string[] { lowestDurBox.UID }, null);
                    SendUIMessage(c, "All items repaired, but your Toolbox was destroyed!");
                }
                else
                {
                    SendUIMessage(c, "All items repaired!");
                }

            }
            // throw new NotImplementedException();
        }
        // little helper to send a char UI notification

        private void SendUIMessage(Character c, string s)
        {
            c.CharacterUI.ShowInfoNotification(s);
        }
    }
}
