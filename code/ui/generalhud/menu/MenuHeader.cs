using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TTTReborn.UI.Menu
{
    public partial class MenuHeader : Panel
    {
        public readonly MenuNavigationHeader NavigationHeader;

        private MenuDragHeader _dragHeaderWrapper;

        public MenuHeader(Menu parent) : base(parent)
        {
            Parent = parent;

            StyleSheet.Load("/ui/generalhud/menu/MenuHeader.scss");

            _dragHeaderWrapper = new MenuDragHeader(this, Parent as Menu);
            NavigationHeader = new MenuNavigationHeader(_dragHeaderWrapper, Parent as Menu);
        }
    }

    public partial class MenuNavigationHeader : PanelHeader
    {
        public Button HomeButton { get; private set; }
        public Button PreviousButton { get; private set; }
        public Button NextButton { get; private set; }

        public readonly Menu Menu;

        public MenuNavigationHeader(Sandbox.UI.Panel parent, Menu menu) : base(parent)
        {
            Parent = parent;
            Menu = menu;

            OnClose = (panelHeader) =>
            {
                Menu.Enabled = false;
            };
        }

        public override void OnCreateHeader()
        {
            HomeButton = new Button("home", "", () => Menu.OpenHomepage());
            HomeButton.AddClass("home");

            AddChild(HomeButton);

            PreviousButton = Add.Button("<", "previous", () => Menu.MenuContent.Previous());
            NextButton = Add.Button(">", "next", () => Menu.MenuContent.Next());

            HomeButton.SetClass("disable", true);
            PreviousButton.SetClass("disable", true);
            NextButton.SetClass("disable", true);
        }
    }

    public partial class MenuDragHeader : Drag
    {
        public MenuDragHeader(Sandbox.UI.Panel parent, Menu menu) : base(parent)
        {
            Parent = parent;
            DragBasePanel = menu;

            IsFreeDraggable = true;
        }
    }
}
