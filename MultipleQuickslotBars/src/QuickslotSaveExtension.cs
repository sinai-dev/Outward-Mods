using SideLoader.SaveData;
using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultipleQuickslotBars
{
    public class QuickslotSaveExtension : PlayerSaveExtension
    {
        public int ActiveBarIndex = 0;
        public List<string> QuickslotData = new List<string>(MultipleQuickslotBars.cfg_NumberOfExtraBars.Value);

        public override void ApplyLoadedSave(Character character, bool isWorldHost)
        {
            var ext = MultipleQuickslotBars.GetOrCreateSave(character.UID);

            ext.ActiveBarIndex = this.ActiveBarIndex;
            ext.QuickslotData = this.QuickslotData;

            ApplySaveToCharacter(character);
        }

        public override void Save(Character character, bool isWorldHost)
        {
            var ext = MultipleQuickslotBars.GetOrCreateSave(character.UID);

            this.ActiveBarIndex = ext.ActiveBarIndex;
            this.QuickslotData = ext.QuickslotData;

            SetDataFromCharacter(character);
        }

        public void SetDataFromCharacter(Character character)
        {
            var qsMgr = character.QuickSlotMngr;

            while (ActiveBarIndex >= QuickslotData.Count)
                QuickslotData.Add(null);

            string data = "";
            for (int i = 0; i < qsMgr.QuickSlotCount; i++)
            {
                if (data != "")
                    data += ",";

                var qs = qsMgr.GetQuickSlot(i);

                int add = qs ? qs.ItemID : -1;

                // Debug.Log("Quickslot " + i + " is " + add);

                data += add.ToString();
            }

            // Debug.Log("Saving data as: " + data);

            QuickslotData[ActiveBarIndex] = data;
        }

        public static void ApplySaveToCharacter(Character character)
        {
            //Debug.Log("Applying QS save to character");

            var ext = MultipleQuickslotBars.GetOrCreateSave(character.UID);

            var qsMgr = character.QuickSlotMngr;

            while (ext.ActiveBarIndex >= ext.QuickslotData.Count)
                ext.QuickslotData.Add(null);

            string data = ext.QuickslotData[ext.ActiveBarIndex];
            int[] itemIDs = new int[qsMgr.QuickSlotCount];

            if (string.IsNullOrEmpty(data))
            {
                for (int i = 0; i < qsMgr.QuickSlotCount; i++)
                    itemIDs[i] = -1;
            }
            else
            {
                string[] idSplit = data.Split(',');

                for (int i = 0; i < qsMgr.QuickSlotCount; i++)
                {
                    if (i >= idSplit.Length)
                        itemIDs[i] = -1;
                    else if (int.TryParse(idSplit[i], out int id))
                        itemIDs[i] = id;
                    else
                        itemIDs[i] = -1;
                }
            }

            for (int i = 0; i < itemIDs.Length; i++)
            {
                try
                {
                    // Debug.Log("Setting quickslot " + i + " to character as item id " + itemIDs[i]);

                    qsMgr.ClearQuickSlot(i);

                    Item item = character.Inventory.SkillKnowledge.GetItemFromItemID(itemIDs[i]);
                    if (!item)
                        item = ResourcesPrefabManager.Instance.GetItemPrefab(itemIDs[i]);

                    if (item)
                        qsMgr.SetQuickSlot(i, item, true);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Exception setting player quickslot index " + i + ": " + ex);
                }
            }
        }
    }
}
