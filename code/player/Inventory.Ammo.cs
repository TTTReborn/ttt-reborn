using System;
using System.Collections.Generic;

using Sandbox;

using TTTReborn.Items;

namespace TTTReborn.Player
{
    public partial class AmmoInventory
    {
        private Dictionary<string, int> AmmoList { get; set; } = new();
        private Inventory Inventory;

        public AmmoInventory(Inventory inventory) : base()
        {
            Inventory = inventory;
        }

        public int Count(string ammoType)
        {
            string ammo = ammoType.ToLower();

            if (AmmoList == null || !AmmoList.ContainsKey(ammo))
            {
                return 0;
            }

            return AmmoList[ammo];
        }

        public bool Set(string ammoType, int amount)
        {
            string ammo = ammoType.ToLower();

            if (AmmoList == null || string.IsNullOrEmpty(ammo))
            {
                return false;
            }

            while (!AmmoList.ContainsKey(ammo))
            {
                AmmoList.Add(ammo, 0);
            }

            AmmoList[ammo] = amount;


            if (Host.IsServer)
            {
                TTTPlayer player = Inventory.Owner as TTTPlayer;

                player.ClientSetAmmo(To.Single(player), ammo, amount);
            }

            return true;
        }

        public bool Give(string ammoType, int amount)
        {
            string ammo = ammoType.ToLower();

            if (AmmoList == null)
            {
                return false;
            }

            Set(ammo, Count(ammo) + amount);

            return true;
        }

        public int Take(string ammoType, int amount)
        {
            string ammo = ammoType.ToLower();

            if (AmmoList == null)
            {
                return 0;
            }

            int available = Count(ammoType);
            amount = Math.Min(available, amount);

            Set(ammoType, available - amount);

            return amount;
        }

        public void Clear()
        {
            AmmoList.Clear();

            if (Host.IsServer)
            {
                TTTPlayer player = Inventory.Owner as TTTPlayer;

                player.ClientClearAmmo(To.Single(player));
            }
        }
    }
}
