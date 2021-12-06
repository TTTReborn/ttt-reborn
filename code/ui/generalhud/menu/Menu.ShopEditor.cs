using System;
using System.Collections.Generic;
using System.Text.Json;

using Sandbox;
using Sandbox.UI.Construct;

using TTTReborn.Globalization;
using TTTReborn.Items;
using TTTReborn.Player;
using TTTReborn.Roles;

namespace TTTReborn.UI.Menu
{
    public partial class Menu
    {
        private Panel _shopEditorWrapper;
        private TranslationCheckbox _shopToggle;
        private List<QuickShopItem> _shopItems = new();
        private TTTRole _selectedRole;

        private void OpenShopEditor(PanelContent menuContent)
        {
            menuContent.SetPanelContent((panelContent) =>
            {
                ServerRequestShopEditorAccess();
            }, "MENU_SUBMENU_SHOPEDITOR", "shopeditor");
        }

        [ServerCmd]
        public static void ServerRequestShopEditorAccess()
        {
            if (ConsoleSystem.Caller == null)
            {
                return;
            }

            To to = To.Single(ConsoleSystem.Caller);

            if (!ConsoleSystem.Caller.HasPermission("shopeditor"))
            {
                ClientReceiveShopEditorAccess(to, false);

                return;
            }

            foreach (Type roleType in Utils.GetTypes<TTTRole>())
            {
                TTTRole role = Utils.GetObjectByType<TTTRole>(roleType);

                ClientUpdateRoleShop(to, role.Name, JsonSerializer.Serialize(role.Shop));
            }

            ClientReceiveShopEditorAccess(to, true);
        }

        [ClientRpc]
        public static void ClientUpdateRoleShop(string roleName, string shopJson)
        {
            Type roleType = Utils.GetTypeByLibraryName<TTTRole>(roleName);

            if (roleType == null)
            {
                return;
            }

            TTTRole role = Utils.GetObjectByType<TTTRole>(roleType);

            if (role == null)
            {
                return;
            }

            role.Shop = Shop.InitializeFromJSON(shopJson);
        }

        [ClientRpc]
        public static void ClientReceiveShopEditorAccess(bool access)
        {
            Menu menu = Menu.Instance;

            if (menu == null || !menu.Enabled)
            {
                return;
            }

            PanelContent menuContent = menu.Content;

            if (menuContent == null || !menuContent.ClassName.Equals("shopeditor"))
            {
                return;
            }

            menu.CreateShopEditorContent(access);
        }

        private void CreateShopEditorContent(bool access)
        {
            if (access)
            {
                Content.SetPanelContent((panelContent) =>
                {
                    TranslationDropdown dropdown = panelContent.Add.TranslationDropdown();
                    dropdown.AddTooltip(new TranslationData("MENU_SHOPEDITOR_SELECTROLE"), "roleselection");

                    dropdown.AddEventListener("onchange", (e) =>
                    {
                        if (dropdown?.Selected?.Value is TTTRole role)
                        {
                            CreateShopContent(role);
                        }
                    });

                    _shopToggle = panelContent.Add.TranslationCheckbox(new TranslationData("MENU_SHOPEDITOR_ENABLEROLE"));
                    _shopToggle.Enabled = false;
                    _shopToggle.AddTooltip(new TranslationData("MENU_SHOPEDITOR_TOGGLEROLE"), "togglehint");
                    _shopToggle.AddEventListener("onchange", (e) =>
                    {
                        if (dropdown?.Selected?.Value is TTTRole role)
                        {
                            ServerToggleShop(role.Name, _shopToggle.Checked);
                        }
                    });

                    foreach (Type roleType in Utils.GetTypes<TTTRole>())
                    {
                        TTTRole role = Utils.GetObjectByType<TTTRole>(roleType);

                        if (role == null)
                        {
                            continue;
                        }

                        dropdown.Options.Add(new TranslationOption(new TranslationData(role.GetRoleTranslationKey("NAME")), role));
                    }

                    _shopEditorWrapper = new(panelContent);
                    _shopEditorWrapper.AddClass("wrapper");

                    _shopEditorWrapper.Add.TranslationLabel(new TranslationData("MENU_SHOPEDITOR_SELECTROLE"));
                }, "MENU_SUBMENU_SHOPEDITOR", "shopeditor");
            }
            else
            {
                Content.SetPanelContent((panelContent) =>
                {
                    _shopEditorWrapper = new(panelContent);
                    _shopEditorWrapper.AddClass("wrapper");

                    _shopEditorWrapper.Add.Label("MENU_SHOPEDITOR_NOPERMISSION");
                }, "MENU_SUBMENU_SHOPEDITOR", "shopeditor");

                return;
            }
        }

        private void CreateShopContent(TTTRole role)
        {
            _shopEditorWrapper.DeleteChildren(true);
            _shopItems.Clear();

            _selectedRole = role;

            _shopToggle.Enabled = true;
            _shopToggle.Checked = role.Shop.Enabled;

            foreach (Type itemType in Utils.GetTypesWithAttribute<IItem, BuyableAttribute>())
            {
                ShopItemData shopItemData = ShopItemData.CreateItemData(itemType);

                if (shopItemData == null)
                {
                    continue;
                }

                QuickShopItem item = new(_shopEditorWrapper);
                item.SetItem(shopItemData);

                item.AddEventListener("onclick", (e) =>
                {
                    ToggleItem(item, role);
                });

                item.AddEventListener("onrightclick", (e) =>
                {
                    EditItem(item, role);
                });

                foreach (ShopItemData loopItemData in role.Shop.Items)
                {
                    if (loopItemData.Name.Equals(shopItemData.Name))
                    {
                        shopItemData.CopyFrom(loopItemData);

                        item.SetItem(shopItemData);
                        item.SetClass("selected", true);
                    }
                }

                item.AddTooltip(new TranslationData(), "buttons", null, (tooltip) =>
                {
                    item.SetClass("tooltip-right", false);
                    item.SetClass("tooltip-left", false);
                }, (tooltip) =>
                {
                    if (item.HasClass("selected"))
                    {
                        if (item.HasClass("tooltip-left"))
                        {
                            return;
                        }

                        item.SetClass("tooltip-right", false);
                        item.SetClass("tooltip-left", true);

                        tooltip.DeleteChildren(true);

                        Sandbox.UI.Panel panel = tooltip.Add.Panel("span");
                        panel.Add.Label("keyboard_arrow_left", "icon");
                        panel.Add.TranslationLabel(new TranslationData("MENU_SHOPEDITOR_ITEM_DEACTIVATE", role.GetRoleTranslationKey("NAME")));

                        panel = tooltip.Add.Panel("span");
                        panel.Add.Icon("keyboard_arrow_right", "icon");
                        panel.Add.TranslationLabel(new TranslationData("MENU_SHOPEDITOR_ITEM_EDIT"));
                    }
                    else if (!item.HasClass("tooltip-right"))
                    {
                        item.SetClass("tooltip-right", true);
                        item.SetClass("tooltip-left", false);

                        tooltip.DeleteChildren(true);

                        Sandbox.UI.Panel panel = tooltip.Add.Panel("span");
                        panel.Add.Label("keyboard_arrow_left", "icon");
                        panel.Add.TranslationLabel(new TranslationData("MENU_SHOPEDITOR_ITEM_ACTIVATE", role.GetRoleTranslationKey("NAME")));
                    }
                });

                _shopItems.Add(item);
            }

            // link shops together
            // edit items
        }
    }
}
