using System;
using System.Linq;

using Sandbox;

using TTTReborn.Globals;
using TTTReborn.Items;

namespace TTTReborn.Player
{
    public partial class Inventory : BaseInventory
    {
        public readonly PerksInventory Perks;
        public readonly AmmoInventory Ammo;
        public readonly int[] SlotCapacity = new int[] { 1, 1, 1, 3, 3 };

        public Inventory(TTTPlayer player) : base(player)
        {
            Ammo = new AmmoInventory(this);
            Perks = new PerksInventory(this);
        }

        public override void DeleteContents()
        {
            foreach (Entity entity in List)
            {
                if (entity is IItem item)
                {
                    item.Remove();
                }
            }

            RPCs.ClientClearInventory(To.Multiple(Utils.GetClientsSpectatingPlayer(Owner as TTTPlayer)));

            Perks.Clear();
            Ammo.Clear();

            base.DeleteContents();
        }

        public override bool Add(Entity entity, bool makeActive = false)
        {
            TTTPlayer player = Owner as TTTPlayer;

            if (entity is ICarriableItem carriable)
            {
                if (IsCarryingType(entity.GetType()) || !HasEmptySlot(carriable.SlotType))
                {
                    return false;
                }

                RPCs.ClientOnPlayerCarriableItemPickup(To.Multiple(Utils.GetClientsSpectatingPlayer(player)), entity);
                Sound.FromWorld("dm.pickup_weapon", entity.Position);
            }

            bool added = base.Add(entity, makeActive);

            return added;
        }

        public bool Add(TTTPerk perk)
        {
            return Perks.Give(perk);
        }

        public bool Add(IItem item, bool makeActive = false)
        {
            if (item is Entity ent)
            {
                return Add(ent, makeActive);
            }
            else if (item is TTTPerk perk)
            {
                return Add(perk);
            }

            return false;
        }

        /// <summary>
        /// Tries to add an `TTTReborn.Items.IItem` to the inventory. If it fails, the given item is deleted
        /// </summary>
        /// <param name="item">`TTTReborn.Items.IItem` that will be added to the inventory or get removed on fail</param>
        /// <param name="makeActive"></param>
        /// <returns></returns>
        public bool TryAdd(IItem item, bool makeActive = false)
        {
            if (!Add(item, makeActive))
            {
                item.Delete();

                return false;
            }

            return true;
        }

        public bool HasEmptySlot(SlotType slotType)
        {
            int itemsInSlot = List.Count(x => ((ICarriableItem) x).SlotType == slotType);

            return SlotCapacity[(int) slotType] - itemsInSlot > 0;
        }

        public bool IsCarryingType(Type t)
        {
            return List.Any(x => x.GetType() == t);
        }

        public override bool Drop(Entity entity)
        {
            if (!Host.IsServer || !Contains(entity) || entity is ICarriableItem item && !item.CanDrop())
            {
                return false;
            }

            using (Prediction.Off())
            {
                RPCs.ClientOnPlayerCarriableItemDrop(To.Multiple(Utils.GetClientsSpectatingPlayer(Owner as TTTPlayer)), entity);
            }

            return base.Drop(entity);
        }
    }
}
