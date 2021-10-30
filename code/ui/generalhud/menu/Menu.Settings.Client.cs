using Sandbox;
using Sandbox.UI.Construct;

using TTTReborn.Globalization;
using TTTReborn.Player;
using TTTReborn.Settings;

namespace TTTReborn.UI.Menu
{
    public partial class Menu
    {
        private void CreateClientSettings(PanelContent tabContent)
        {
            Tabs tabs = tabContent.Add.Tabs();

            tabs.AddTab("MENU_SETTINGS_CATEGORY_GENERAL", (panelContent) =>
            {
                AddGeneralSettings(panelContent);
            }, "general");

            tabs.AddTab("MENU_SETTINGS_CATEGORY_LANGUAGE", (panelContent) =>
            {
                AddLanguageSettings(panelContent);
            }, "language");
        }

        private void AddGeneralSettings(PanelContent panelContent)
        {
            Sandbox.UI.Panel sprintPanel = panelContent.Add.Panel("sprint");
            sprintPanel.Add.TranslationLabel("MENU_SETTINGS_FORCESPECTATOR");

            if (Local.Client.Pawn is TTTPlayer player)
            {
                Switch sw = sprintPanel.Add.Switch("forcespectator", player.IsForcedSpectator);
                sw.AddEventListener("onchange", (panelEvent) =>
                {
                    TTTPlayer.ToggleForceSpectator();

                    player.IsForcedSpectator = !player.IsForcedSpectator;
                });
            }
        }

        private void AddLanguageSettings(PanelContent panelContent)
        {
            Sandbox.UI.Panel languagePanel = panelContent.Add.Panel("language");

            languagePanel.Add.TranslationLabel("MENU_SETTINGS_LANGUAGE_SELECTION");

            Dropdown languageSelection = languagePanel.Add.Dropdown();

            foreach (Language language in TTTLanguage.Languages.Values)
            {
                languageSelection.AddOption(language.Data.Name, language.Data.Code);
            }

            languageSelection.OnSelectOption = (option) =>
            {
                TTTPlayer.ChangeLanguage((string) option.Data);
            };

            languageSelection.SelectByData(SettingsManager.Instance.General.Language);
        }
    }
}
