using System;

using Sandbox;

using TTTReborn.Globalization;
using TTTReborn.Settings;

namespace TTTReborn.UI.Menu
{
    using Sandbox.UI.Construct;

    using TTTReborn.Globals;

    public partial class Menu
    {
        public PanelContent ServerSettingsTabContent { get; private set; }

        public Tabs SettingsTabs { get; private set; }

        internal void OpenSettings(PanelContent menuContent)
        {
            menuContent.SetPanelContent((menuContent) =>
            {
                SettingsTabs = menuContent.Add.Tabs();
                SettingsTabs.AddTab(new TranslationData("REALM_CLIENT"), (panelContent) =>
                {
                    panelContent.SetPanelContent((content) => CreateSettings(content.Add.Tabs(), Settings.ClientSettings.Instance));
                }, Utils.Realm.Client);

                if (Local.Client.HasPermission("serversettings"))
                {
                    SettingsTabs.AddTab(new TranslationData("REALM_SERVER"), InitServerSettings, Utils.Realm.Server);
                }

                CreateSettingsButtons(menuContent);
            }, "MENU_SUBMENU_SETTINGS", "settings");
        }

        private void InitServerSettings(PanelContent tabContent)
        {
            ServerSettingsTabContent = tabContent;

            tabContent.Add.TranslationLabel(new TranslationData("MENU_SETTINGS_LOADING"));

            SettingFunctions.RequestServerSettings();
        }

        internal void ProceedServerSettings(ServerSettings serverSettings)
        {
            if (!Enabled || ServerSettingsTabContent == null)
            {
                return;
            }

            ServerSettingsTabContent.SetPanelContent((menuContent) => CreateSettings(menuContent.Add.Tabs(), serverSettings));
        }

        internal static Sandbox.UI.TextEntry CreateSettingsEntry<T>(Sandbox.UI.Panel parent, string title, T defaultValue, string description, Action<T> OnSubmit = null, Action<T> OnChange = null, params object[] translationData)
        {
            Sandbox.UI.Panel wrapper = parent.Add.Panel();
            TranslationLabel textLabel = wrapper.Add.TranslationLabel(new TranslationData(title));
            textLabel.AddTooltip(new TranslationData(description), null, null, null, null);

            Sandbox.UI.TextEntry textEntry = wrapper.Add.TextEntry(defaultValue.ToString());
            textEntry.AddClass("setting");
            textEntry.AddClass("rounded");
            textEntry.AddClass("box-shadow");
            textEntry.AddClass("background-color-secondary");

            textEntry.AddEventListener("onsubmit", (panelEvent) =>
            {
                try
                {
                    textEntry.Text.TryToType(typeof(T), out object value);

                    if (value.ToString().Equals(textEntry.Text))
                    {
                        T newValue = (T) value;

                        OnSubmit?.Invoke(newValue);

                        defaultValue = newValue;
                    }
                }
                catch (Exception) { }

                textEntry.Text = defaultValue.ToString();
            });

            textEntry.AddEventListener("onchange", (panelEvent) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(textEntry.Text))
                    {
                        return;
                    }

                    textEntry.Text.TryToType(typeof(T), out object value);

                    if (value.ToString().Equals(textEntry.Text))
                    {
                        T newValue = (T) value;

                        OnChange?.Invoke(newValue);

                        defaultValue = newValue;
                    }
                }
                catch (Exception) { }

                textEntry.Text = defaultValue.ToString();
            });

            return textEntry;
        }
    }
}
