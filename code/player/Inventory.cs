using System;
using System.Collections.Generic;
using System.Linq;

using Sandbox;

using TTTReborn.Items;

namespace TTTReborn.Player
{
    public partial class Inventory : BaseInventory
    {
        public readonly PerksInventory Perks;
        public readonly AmmoInventory Ammo;
        public readonly int[] SlotCapacity = new int[] { 1, 1, 1, 3, 3, 1 };

        private const int DROPPOSITIONOFFSET = 50;
        private const int DROPVELOCITY = 500;

        public Inventory(TTTPlayer player) : base(player)
        {
            Ammo = new(player);
            Perks = new(player);
        }

        public static int GetSlotByCategory(CarriableCategories category)
        {
            return category switch
            {
                CarriableCategories.Melee => 1,
                CarriableCategories.Pistol => 2,
                CarriableCategories.SMG or CarriableCategories.Shotgun or CarriableCategories.Sniper => 3,
                CarriableCategories.OffensiveEquipment => 4,
                CarriableCategories.UtilityEquipment => 5,
                CarriableCategories.Grenade => 6,
                _ => 7,
            };
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

            RPCs.ClientClearInventory(To.Multiple(Utils.GetClients((pl) => pl.CurrentPlayer == Owner as TTTPlayer)));

            Perks.Clear();
            Ammo.Clear();

            base.DeleteContents();
        }

        public override bool Add(Entity entity, bool makeActive = false)
        {
            TTTPlayer player = Owner as TTTPlayer;

            if (entity is ICarriableItem carriable)
            {
                if (IsCarryingType(entity.GetType()) || !HasEmptySlot(carriable.Category))
                {
                    return false;
                }
            }

            bool add = base.Add(entity, makeActive);

            if (add && entity is ICarriableItem carriableItem)
            {
                RPCs.ClientOnPlayerCarriableItemPickup(To.Multiple(Utils.GetClients((pl) => pl.CurrentPlayer == player)), entity);
                Sound.FromWorld("dm.pickup_weapon", entity.Position);
            }

            return add;
        }

        public bool Add(Perk perk)
        {
            return Perks.Give(perk);
        }

        public bool Add(IItem item, bool makeActive = false)
        {
            if (item is Entity ent)
            {
                return Add(ent, makeActive);
            }
            else if (item is Perk perk)
            {
                return Add(perk);
            }

            return false;
        }

        /// <summary>
        /// Tries to add an `TTTReborn.Items.IItem` to the inventory.
        /// </summary>
        /// <param name="item">`TTTReborn.Items.IItem` that will be added to the inventory if conditions are met.</param>
        /// <param name="deleteIfFails">Delete `TTTReborn.Items.IItem` if it fails to add to inventory.</param>
        /// <param name="makeActive">Make `TTTReborn.Items.IItem` the active item in the inventory.</param>
        /// <returns></returns>
        public bool TryAdd(IItem item, bool deleteIfFails = false, bool makeActive = false)
        {
            if (Owner.LifeState != LifeState.Alive || !Add(item, makeActive))
            {
                if (deleteIfFails)
                {
                    item.Delete();
                }

                return false;
            }

            return true;
        }

        public bool Remove(Entity item)
        {
            if (Contains(item))
            {
                RPCs.ClientOnPlayerCarriableItemDrop(To.Single(Owner), item);
                item.Delete();
                List.Remove(item);

                return true;
            }

            return false;
        }

        public bool HasEmptySlot(CarriableCategories category)
        {
            int itemsInSlot = List.Count(x => ((ICarriableItem) x).Category == category);

            return SlotCapacity[GetSlotByCategory(category) - 1] - itemsInSlot > 0;
        }

        public bool IsCarryingType(Type t)
        {
            return List.Any(x => x.GetType() == t);
        }

        public IList<string> GetAmmoNames()
        {
            List<string> types = new();

            foreach (Entity entity in List)
            {
                if (entity is Weapon wep)
                {
                    if (!types.Contains(wep.AmmoName))
                    {
                        types.Add(wep.AmmoName);
                    }
                }
            }

            return types;
        }

        public void SelectHands()
        {
            foreach (Entity entity in List)
            {
                if (entity is Hands)
                {
                    Active = entity;

                    break;
                }
            }
        }

        public override bool Drop(Entity entity)
        {
            if (!Host.IsServer || !Contains(entity) || entity is ICarriableItem item && !item.CanDrop())
            {
                return false;
            }

            using (Prediction.Off())
            {
                RPCs.ClientOnPlayerCarriableItemDrop(To.Multiple(Utils.GetClients((pl) => pl.CurrentPlayer == Owner as TTTPlayer)), entity);
            }

            return base.Drop(entity);
        }

        public void DropAll()
        {
            List<Entity> cache = new(List);

            foreach (Entity entity in cache)
            {
                Drop(entity);
            }
        }

        public override Entity DropActive()
        {
            Entity entity;

            if (Active is Equipment equipment && equipment.CanDrop())
            {
                entity = DropEntity(equipment);
            }
            else
            {
                entity = base.DropActive();
            }

            if (Active == null)
            {
                SelectHands();
            }

            return entity;
        }

        public Entity DropEntity(Equipment equipment)
        {
            Type type = equipment.ObjectType;
            Vector3 position = Owner.EyePosition + Owner.EyeRotation.Forward * DROPPOSITIONOFFSET;
            Rotation rotation = Owner.EyeRotation;
            Vector3 velocity = Owner.EyeRotation.Forward * DROPVELOCITY;

            if (Remove(equipment))
            {
                Entity droppedEntity = Utils.GetObjectByType<Entity>(type);
                droppedEntity.Position = position;
                droppedEntity.Rotation = rotation;
                droppedEntity.Velocity = velocity;
                droppedEntity.Tags.Add(IItem.ITEM_TAG);

                SelectHands();

                return droppedEntity;
            }

            return null;
        }
    }
}
