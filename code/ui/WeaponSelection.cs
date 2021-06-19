using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

using TTTReborn.Player;
using TTTReborn.Items;

// TODO Fix animation on dropping a higher slot weapon (instead of deleting and recreating, move and delete WeaponSlots in DOM)

namespace TTTReborn.UI
{
    public class WeaponSelection : Panel
    {
        private Dictionary<string, WeaponSlot> weaponSlots = new();

        private TTTWeapon oldActiveWeapon;

        public WeaponSelection()
        {
            StyleSheet.Load("/ui/WeaponSelection.scss");
        }

        // TODO: This method could be made to be event based. Whenever a user pickups a weapon, and whenever a user fires.
        // Consider checking out DM98 or Hidden inventory HUD.
        public override void Tick()
        {
            base.Tick();

            var player = Local.Pawn as TTTPlayer;

            if (player == null)
            {
                return;
            }

            // update weapon slots, check for removed weapons etc.
            Inventory inventory = player.Inventory as Inventory;
            int inventoryCount = inventory.Count();
            List<TTTWeapon> newWeapons = new();
            List<WeaponSlot> tmpSlots = new();

            for (int i = 0; i < inventoryCount; i++)
            {
                TTTWeapon weapon = inventory.GetSlot(i) as TTTWeapon;

                if (weaponSlots.TryGetValue(weapon.GetName(), out WeaponSlot weaponSlot))
                {
                    tmpSlots.Add(weaponSlot);
                }
                else
                {
                    newWeapons.Add(weapon);
                }

                // Do not update if we already rebuilding the WeaponSlots
                if (newWeapons.Count == 0 && weapon.WeaponType != WeaponType.Melee)
                {
                    weaponSlot.UpdateAmmo($"{weapon.AmmoClip} + {player.AmmoCount(weapon.AmmoType)}");
                }
            }

            // remove WeaponSlots and rebuild (to keep the right order)
            if (newWeapons.Count != 0 || tmpSlots.Count != weaponSlots.Count)
            {
                foreach (WeaponSlot weaponSlot in weaponSlots.Values)
                {
                    weaponSlot.Delete();
                }

                weaponSlots.Clear();

                inventory.List.Sort((Entity wep1, Entity wep2) => (wep1 as TTTWeapon).WeaponType.CompareTo((wep2 as TTTWeapon).WeaponType));

                // rebuild weaponslots in the right order
                foreach (TTTWeapon weapon in inventory.List)
                {
                    // add in order
                    weaponSlots.Add(weapon.GetName(), new WeaponSlot(this, weapon));
                }

                // update for dropped active weapon maybe
                oldActiveWeapon = null;
            }

            // update current selection
            TTTWeapon activeWeapon = player.ActiveChild as TTTWeapon;

            if (oldActiveWeapon != activeWeapon && activeWeapon != null)
            {
                foreach (WeaponSlot weaponSlot in weaponSlots.Values)
                {
                    weaponSlot.SetClass("active", weaponSlot.WeaponName == activeWeapon.GetName());
                }

                oldActiveWeapon = activeWeapon;
            }

            SetClass("hide", weaponSlots.Count == 0);
        }

        /// <summary>
        /// IClientInput implementation, calls during the client input build.
        /// You can both read and write to input, to affect what happens down the line.
        /// </summary>
        [Event.BuildInput]
        private void ProcessClientWeaponSelectionInput(InputBuilder input)
        {
            Inventory inventory = Local.Pawn.Inventory as Inventory;

            if (inventory.Count() == 0)
            {
                return;
            }

            int selectedWeaponIndex = SlotPressInput(input);

            // no button pressed, but maybe scrolled
            if (selectedWeaponIndex == 0)
            {
                if (input.MouseWheel != 0)
                {
                    int nextSlot = inventory.List.IndexOf(Local.Pawn.ActiveChild) - input.MouseWheel;
                    nextSlot = inventory.NormalizeSlotIndex(nextSlot, inventory.Count() - 1);

                    input.ActiveChild = inventory.GetSlot(nextSlot);

                    input.MouseWheel = 0;
                }

                return;
            }

            // corresponding key pressed
            int nextWeaponSlot = inventory.GetNextWeaponSlot((WeaponType) selectedWeaponIndex);

            if (nextWeaponSlot != -1)
            {
                input.ActiveChild = inventory.GetSlot(nextWeaponSlot);
            }
        }

        // TODO: Handle mouse wheel, and additional number keys.
        private int SlotPressInput(InputBuilder input)
        {
            if (input.Pressed(InputButton.Slot1)) return 1;
            if (input.Pressed(InputButton.Slot2)) return 2;
            if (input.Pressed(InputButton.Slot3)) return 3;
            if (input.Pressed(InputButton.Slot4)) return 4;
            if (input.Pressed(InputButton.Slot5)) return 5;

            return 0;
        }

        public class WeaponSlot : Panel
        {
            public string WeaponName { get; private set; }
            public Label SlotLabel { set; get; }
            public Label WeaponLabel { set; get; }
            public Label AmmoLabel { set; get; }

            public WeaponSlot(Panel parent, TTTWeapon weapon)
            {
                Parent = parent;
                WeaponName = weapon.GetName();

                SlotLabel = Add.Label(((int) weapon.WeaponType).ToString(), "slotlabel");
                WeaponLabel = Add.Label(weapon.GetName(), "weaponlabel");

                if (weapon.WeaponType != WeaponType.Melee)
                {
                    AmmoLabel = Add.Label($"{weapon.AmmoClip}/{weapon.ClipSize}", "ammolabel");
                }
            }

            public void UpdateAmmo(string ammoText)
            {
                AmmoLabel.Text = ammoText;
            }
        }
    }
}
