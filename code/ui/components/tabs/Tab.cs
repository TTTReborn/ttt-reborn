using System;

using Sandbox.UI;
using Sandbox.UI.Construct;

using TTTReborn.Globalization;

namespace TTTReborn.UI
{
    public partial class Tab : Panel
    {
        public readonly Label TitleLabel;

        public Action OnSelectTab { get; set; }
        public Action<PanelContent> CreateContent { get; set; }

        public object Value { get; set; }

        private readonly Tabs _parentTabs;

        public Tab(Sandbox.UI.Panel parent, Tabs parentTabs, TranslationData translationData, Action<PanelContent> createContent, object value = null, Action onSelectTab = null) : base(parent)
        {
            Parent = parent;
            _parentTabs = parentTabs;
            CreateContent = createContent;
            Value = value;
            OnSelectTab = onSelectTab;

            TitleLabel = Add.TranslationLabel(translationData, "title");
        }

        protected override void OnClick(MousePanelEvent e)
        {
            _parentTabs.OnSelectTab(this);
        }
    }
}
