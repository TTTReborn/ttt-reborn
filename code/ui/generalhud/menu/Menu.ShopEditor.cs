using System;

using Sandbox.UI.Construct;

using TTTReborn.Globals;
using TTTReborn.Items;
using TTTReborn.Roles;

namespace TTTReborn.UI.Menu
{
    public partial class Menu
    {
        private Panel _shopEditorWrapper;

        private void OpenShopEditor(PanelContent menuContent)
        {
            menuContent.SetPanelContent((panelContent) =>
            {
                panelContent.Add.Label("ShopEditor:");

                Dropdown dropdown = panelContent.Add.Dropdown();
                dropdown.TextLabel.Text = "Choose role...";

                _shopEditorWrapper = new(panelContent);
                _shopEditorWrapper.AddClass("wrapper");

                foreach (Type roleType in Utils.GetTypes<TTTRole>())
                {
                    dropdown.AddOption(Utils.GetTypeName(roleType), roleType, (panel) =>
                    {
                        CreateShopContent(roleType);
                    });
                }

                // on select, populate
            }, "ShopEditor", "shopeditor");
        }

        private void CreateShopContent(Type roleType)
        {
            _shopEditorWrapper.DeleteChildren(true);

            TTTRole role = Utils.GetObjectByType<TTTRole>(roleType);

            if (role == null || !role.IsSelectable)
            {
                return;
            }

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
                    ToggleItem(item);
                });

                item.AddEventListener("onrightclick", (e) =>
                {
                    Sandbox.Log.Error("Right clicked");
                });

                foreach (ShopItemData loopItemData in role.Shop.Items)
                {
                    if (loopItemData.Name.Equals(shopItemData.Name))
                    {
                        item.SetClass("selected", true);
                    }
                }
            }

            // send update on change to server
            // save on server if send
            // add a toggle to activate shop
            // just show shop if items are greater than 0
            // auto select first role
        }

        private static void ToggleItem(QuickShopItem item)
        {
            bool toggle = !item.HasClass("selected");

            item.SetClass("selected", toggle);
        }
    }
}
