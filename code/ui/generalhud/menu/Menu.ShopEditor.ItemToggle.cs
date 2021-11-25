// TTT Reborn https://github.com/TTTReborn/tttreborn/
// Copyright (C) Neoxult

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see https://github.com/TTTReborn/tttreborn/blob/master/LICENSE.

using System;
using System.Text.Json;

using Sandbox;

using TTTReborn.Globals;
using TTTReborn.Items;
using TTTReborn.Player;
using TTTReborn.Roles;

namespace TTTReborn.UI.Menu
{
    public partial class Menu
    {
        private static void ToggleItem(QuickShopItem item, TTTRole role)
        {
            bool toggle = !item.HasClass("selected");

            ServerUpdateItem(item.ItemData.Name, toggle, toggle ? JsonSerializer.Serialize(item.ItemData) : "", role.Name);
        }

        private static bool ProcessItemUpdate(string itemName, bool toggle, string shopItemDataJson, string roleName, out ShopItemData shopItemData)
        {
            shopItemData = null;

            Type roleType = Utils.GetTypeByLibraryName<TTTRole>(roleName);

            if (roleType == null)
            {
                return false;
            }

            TTTRole role = Utils.GetObjectByType<TTTRole>(roleType);

            if (role == null)
            {
                return false;
            }

            if (toggle)
            {
                ShopItemData itemData = JsonSerializer.Deserialize<ShopItemData>(shopItemDataJson);

                if (itemData == null)
                {
                    return false;
                }

                Type itemType = Utils.GetTypeByLibraryName<IItem>(itemName);

                if (itemType == null)
                {
                    return false;
                }

                shopItemData = ShopItemData.CreateItemData(itemType);

                if (shopItemData == null)
                {
                    return false;
                }

                shopItemData.CopyFrom(itemData);
            }

            UpdateShop(role.Shop, toggle, itemName, shopItemData);

            if (Host.IsServer)
            {
                foreach (Client client in Client.All)
                {
                    if (client.Pawn is TTTPlayer player && player.Role.Equals(roleName))
                    {
                        UpdateShop(player.Shop, toggle, itemName, shopItemData);
                    }
                }
            }
            else if (Local.Client?.Pawn is TTTPlayer player && player.Role.Name.Equals(roleName))
            {
                UpdateShop(player.Shop, toggle, itemName, shopItemData);

                QuickShop.Instance?.Reload();
            }

            return true;
        }

        private static void UpdateShop(Shop shop, bool toggle, string itemName, ShopItemData shopItemData)
        {
            ShopItemData storedItem = null;

            foreach (ShopItemData loopItem in shop.Items)
            {
                if (loopItem.Name.Equals(itemName))
                {
                    storedItem = loopItem;

                    break;
                }
            }

            if (toggle)
            {
                if (storedItem == null)
                {
                    shop.Items.Add(shopItemData);
                }
                else
                {
                    shop.Items.Remove(storedItem);
                    shop.Items.Add(shopItemData);
                }
            }
            else if (storedItem != null)
            {
                shop.Items.Remove(storedItem);
            }
        }

        [ServerCmd]
        public static void ServerUpdateItem(string itemName, bool toggle, string shopItemDataJson, string roleName)
        {
            if (!(ConsoleSystem.Caller?.HasPermission("shopeditor") ?? false))
            {
                return;
            }

            if (ProcessItemUpdate(itemName, toggle, shopItemDataJson, roleName, out _))
            {
                Shop.Save(Utils.GetObjectByType<TTTRole>(Utils.GetTypeByLibraryName<TTTRole>(roleName)));

                ClientUpdateItem(itemName, toggle, shopItemDataJson, roleName);
            }
        }

        [ClientRpc]
        public static void ClientUpdateItem(string itemName, bool toggle, string shopItemDataJson, string roleName)
        {
            if (!ProcessItemUpdate(itemName, toggle, shopItemDataJson, roleName, out ShopItemData shopItemData))
            {
                return;
            }

            Menu menu = Menu.Instance;

            if (menu == null || !menu.Enabled)
            {
                return;
            }

            PanelContent menuContent = menu.Content;

            if (menuContent == null || !menuContent.ClassName.Equals("shopeditor") || !roleName.Equals(menu._selectedRole?.Name) || menu._shopItems.Count < 1)
            {
                return;
            }

            foreach (QuickShopItem shopItem in menu._shopItems)
            {
                if (shopItem.ItemData.Name.Equals(itemName))
                {
                    if (shopItemData != null)
                    {
                        shopItem.SetItem(shopItemData);
                    }

                    shopItem.SetClass("selected", toggle);

                    break;
                }
            }
        }
    }
}
