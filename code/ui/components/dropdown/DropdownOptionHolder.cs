using Sandbox.UI;

namespace TTTReborn.UI
{
    public partial class DropdownOptionHolder : TTTPanel
    {
        public Dropdown RelatedPanel { get; private set; }

        public DropdownOptionHolder(Dropdown relatedPanel) : base()
        {
            RelatedPanel = relatedPanel;

            Hud.Current.RootPanel.AddChild(this);

            StyleSheet.Load("/ui/components/dropdown/DropdownOptionHolder.scss");
        }

        public override void Tick()
        {
            base.Tick();

            if (RelatedPanel.IsDeleted)
            {
                Delete(true);

                return;
            }

            if (!RelatedPanel.IsVisible)
            {
                IsShowing = false;
            }

            if (!IsShowing)
            {
                return;
            }

            Rect rect = RelatedPanel.Box.Rect;

            Style.Left = Length.Pixels(rect.left);
            Style.Top = Length.Pixels(rect.bottom);
            Style.Width = Length.Pixels(rect.width);
            Style.Dirty();
        }
    }
}
