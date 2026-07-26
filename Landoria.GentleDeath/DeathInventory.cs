using System.Collections.Generic;
using UnityEngine;

namespace Landoria.GentleDeath
{
    internal static class DeathInventory
    {
        internal static void CreateTombstone(Player player)
        {
            Inventory playerInventory = player.GetInventory();
            if (playerInventory.NrOfItems() == 0)
            {
                return;
            }

            GameObject tombstone = Object.Instantiate(
                player.m_tombstone,
                player.GetCenterPoint(),
                player.transform.rotation);
            Inventory graveInventory = tombstone.GetComponent<Container>().GetInventory();
            int movedItems = MoveNonEquippableItems(playerInventory, graveInventory);
            SetupTombstone(tombstone);
            GentleDeathPlugin.Log.LogInfo($"Death inventory split: kept={playerInventory.NrOfItems()}, " +
                           $"movedToTombstone={movedItems}.");
        }

        private static int MoveNonEquippableItems(Inventory playerInventory, Inventory graveInventory)
        {
            List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>(
                playerInventory.GetAllItems());
            int movedItems = 0;
            foreach (ItemDrop.ItemData item in items)
            {
                if (!item.IsEquipable() && MoveItem(playerInventory, graveInventory, item))
                {
                    movedItems++;
                }
            }

            return movedItems;
        }

        private static bool MoveItem(
            Inventory playerInventory,
            Inventory graveInventory,
            ItemDrop.ItemData item)
        {
            if (!graveInventory.AddItem(item))
            {
                GentleDeathPlugin.Log.LogWarning($"The tombstone had no room for {item.m_shared.m_name}; " +
                                  "the item was kept by the player.");
                return false;
            }

            return playerInventory.RemoveItem(item);
        }

        private static void SetupTombstone(GameObject tombstone)
        {
            PlayerProfile profile = Game.instance.GetPlayerProfile();
            tombstone.GetComponent<TombStone>().Setup(profile.GetName(), profile.GetPlayerID());
        }
    }
}
