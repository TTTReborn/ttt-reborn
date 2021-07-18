using System;

using Sandbox;

using TTTReborn.Player;

namespace TTTReborn.Items
{
    public struct ShopItemData
    {
        public string Name;
        public string Description;
        public int Price;
        public SlotType SlotType;
        public Type Type;

        public bool IsBuyable(TTTPlayer player)
        {
            Inventory inventory = player.Inventory as Inventory;

            if (Type.IsSubclassOf(typeof(TTTPerk)))
            {
                return !inventory.Perks.Has(Name);
            }
            else if (Type.IsSubclassOf(typeof(TTTEquipment)) || Type.IsSubclassOf(typeof(TTTWeapon)))
            {
                return !inventory.IsCarryingType(Type) && inventory.HasEmptySlot(SlotType);
            }

            return false;
        }
    }

    public interface IBuyableItem : IItem
    {
        int Price { get; }

        ShopItemData CreateItemData()
        {
            ShopItemData itemData = new ShopItemData
            {
                Name = Name,
                Price = Price,
                Type = GetType()
            };

            if (this is ICarriableItem carriableItem)
            {
                itemData.Description = $"Slot: { carriableItem.SlotType }";
                itemData.SlotType = carriableItem.SlotType;
            }

            return itemData;
        }

        void OnPurchase(TTTPlayer player)
        {
            (player.Inventory as Inventory).TryAdd(this);
        }
    }
}
