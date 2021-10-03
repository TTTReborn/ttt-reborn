using System;

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

using TTTReborn.Globals;
using TTTReborn.Settings;

namespace TTTReborn.UI.Menu
{
    public partial class Menu
    {
        public FileSelection ServerSettingsFileSelection { get; private set; }

        private FileSelection _currentFileSelection;

        private string GetSettingsPathByData(object data)
        {
            if (data is Utils.Realm realm)
            {
                if (realm == Utils.Realm.Client)
                {
                    return $"/settings/{Utils.GetTypeNameByType(typeof(ClientSettings)).ToLower()}/";
                }
                else
                {
                    return $"/settings/{Utils.GetTypeNameByType(typeof(ServerSettings)).ToLower()}/";
                }
            }

            return $"/settings/{Utils.GetTypeNameByType(SettingsManager.Instance.GetType()).ToLower()}/";
        }

        private void CreateSettingsButtons(PanelContent menuContent)
        {
            Sandbox.UI.Panel buttonsWrapperPanel = menuContent.Add.Panel("wrapper");

            buttonsWrapperPanel.Add.Button("Save as", "fileselectionbutton", () =>
            {
                _currentFileSelection?.Close();

                FileSelection fileSelection = FindRootPanel().Add.FileSelection();
                fileSelection.DefaultSelectionFileType = $"*{SettingFunctions.SETTINGS_FILE_EXTENSION}";
                fileSelection.OnAgree = () => OnAgreeSaveAs(fileSelection);
                fileSelection.DefaultSelectionPath = GetSettingsPathByData(SettingsTabs.SelectedTab.Value);
                fileSelection.EnableFileNameEntry();
                fileSelection.Display();

                _currentFileSelection = fileSelection;
            });

            buttonsWrapperPanel.Add.Button("Load from", "fileselectionbutton", () =>
            {
                _currentFileSelection?.Close();

                FileSelection fileSelection = FindRootPanel().Add.FileSelection();
                fileSelection.DefaultSelectionFileType = $"*{SettingFunctions.SETTINGS_FILE_EXTENSION}";
                fileSelection.OnAgree = () => OnAgreeLoadFrom(fileSelection, menuContent);
                fileSelection.DefaultSelectionPath = GetSettingsPathByData(SettingsTabs.SelectedTab.Value);

                if (SettingsTabs.SelectedTab.Value is Utils.Realm realm && realm == Utils.Realm.Server)
                {
                    fileSelection.OnClose = (modal) =>
                    {
                        if (ServerSettingsFileSelection != fileSelection)
                        {
                            return;
                        }

                        ServerSettingsFileSelection = null;
                    };

                    ServerSettingsFileSelection = fileSelection;
                }
                else
                {
                    ServerSettingsFileSelection = null;
                }

                fileSelection.Display();

                _currentFileSelection = fileSelection;
            });
        }

        private void OnAgreeSaveAs(FileSelection fileSelection)
        {
            string fileName = fileSelection.FileNameEntry.Text;

            if (string.IsNullOrEmpty(fileName) || SettingsTabs == null)
            {
                return;
            }

            fileSelection.Close();

            fileName = fileName.Split('/')[^1].Split('.')[0];

            if (SettingsTabs.SelectedTab.Value is not Utils.Realm realm)
            {
                return;
            }

            if (realm == Utils.Realm.Client)
            {
                if (!FileSystem.Data.FileExists(fileSelection.CurrentFolderPath + fileName + SettingFunctions.SETTINGS_FILE_EXTENSION))
                {
                    SettingFunctions.SaveSettings<ClientSettings>(ClientSettings.Instance, fileSelection.CurrentFolderPath, fileName);
                }
                else
                {
                    AskOverwriteSelectedSettings(fileSelection.CurrentFolderPath, fileName, () => SettingFunctions.SaveSettings<ClientSettings>(ClientSettings.Instance, fileSelection.CurrentFolderPath, fileName));
                }
            }
            else if (realm == Utils.Realm.Server)
            {
                ConsoleSystem.Run("ttt_serversettings_saveas_request", fileSelection.CurrentFolderPath, fileName);
            }
        }

        internal static void AskOverwriteSelectedSettings(string folderPath, string fileName, Action onConfirm)
        {
            string fullFilePath = folderPath + fileName + SettingFunctions.SETTINGS_FILE_EXTENSION;

            DialogBox dialogBox = new DialogBox();
            dialogBox.TitleLabel.Text = $"Overwrite '{fullFilePath}'";
            dialogBox.AddText($"Do you want to overwrite '{fullFilePath}' with the current settings? (If you agree, the settings defined in this file will be lost!)");
            dialogBox.OnAgree = () =>
            {
                onConfirm();

                dialogBox.Close();
            };
            dialogBox.OnDecline = () =>
            {
                dialogBox.Close();
            };

            Hud.Current.RootPanel.AddChild(dialogBox);

            dialogBox.Display();
        }

        private void OnAgreeLoadFrom(FileSelection fileSelection, PanelContent menuContent)
        {
            string fileName = fileSelection.SelectedEntry.FileNameLabel.Text;

            if (string.IsNullOrEmpty(fileName) || SettingsTabs == null)
            {
                return;
            }

            fileName = fileName.Split('/')[^1].Split('.')[0];

            if (SettingsTabs.SelectedTab.Value is not Utils.Realm realm)
            {
                return;
            }

            if (realm == Utils.Realm.Client)
            {
                SettingsManager.Instance = SettingFunctions.LoadSettings<ClientSettings>(fileSelection.CurrentFolderPath, fileName);

                if (SettingsManager.Instance.LoadingError != SettingsLoadingError.None)
                {
                    Log.Error($"Settings file '{fileSelection.CurrentFolderPath}{fileName}{SettingFunctions.SETTINGS_FILE_EXTENSION}' can't be loaded. Reason: '{SettingsManager.Instance.LoadingError.ToString()}'");

                    return;
                }

                SettingFunctions.SaveSettings<ClientSettings>(ClientSettings.Instance);

                fileSelection.Close();

                // refresh settings
                menuContent.SetPanelContent(OpenSettings);
            }
            else if (realm == Utils.Realm.Server)
            {
                ConsoleSystem.Run("ttt_serversettings_loadfrom_request", fileSelection.CurrentFolderPath, fileName);
            }
        }
    }
}

namespace TTTReborn.Player
{
    using TTTReborn.UI.Menu;

    public partial class TTTPlayer
    {
        [ServerCmd(Name = "ttt_serversettings_saveas_request")]
        public static void RequestSaveAs(string filePath, string fileName, bool overwrite = false)
        {
            if (!ConsoleSystem.Caller.HasPermission("serversettings"))
            {
                return;
            }

            if (overwrite || !FileSystem.Data.FileExists(filePath + fileName + SettingFunctions.SETTINGS_FILE_EXTENSION))
            {
                SettingFunctions.SaveSettings<ServerSettings>(ServerSettings.Instance, filePath, fileName);
            }
            else
            {
                ClientAskOverwriteSelectedSettings(To.Single(ConsoleSystem.Caller), filePath, fileName);
            }
        }

        [ClientRpc]
        public static void ClientAskOverwriteSelectedSettings(string filePath, string fileName)
        {
            Menu.AskOverwriteSelectedSettings(filePath, fileName, () =>
            {
                ConsoleSystem.Run("ttt_serversettings_saveas_request", filePath, fileName, true);
            });
        }

        [ServerCmd(Name = "ttt_serversettings_loadfrom_request")]
        public static void RequestLoadFrom(string filePath, string fileName)
        {
            if (!ConsoleSystem.Caller.HasPermission("serversettings"))
            {
                return;
            }

            SettingsManager.Instance = SettingFunctions.LoadSettings<ServerSettings>(filePath, fileName);

            if (SettingsManager.Instance.LoadingError != SettingsLoadingError.None)
            {
                Log.Error($"Settings file '{filePath}{fileName}{SettingFunctions.SETTINGS_FILE_EXTENSION}' can't be loaded. Reason: '{SettingsManager.Instance.LoadingError.ToString()}'");

                return;
            }

            SettingFunctions.SaveSettings<ServerSettings>(ServerSettings.Instance);

            ClientFinishServerSettingsLoading(To.Single(ConsoleSystem.Caller), filePath, fileName);
        }

        [ClientRpc]
        public static void ClientFinishServerSettingsLoading(string filePath, string fileName)
        {
            Menu menu = Menu.Instance;

            if (menu != null && menu.Enabled && menu.ServerSettingsTabContent != null)
            {
                // refresh settings
                menu.MenuContent.SetPanelContent(menu.OpenSettings);
                menu.SettingsTabs?.SelectByValue(Utils.Realm.Server);

                menu.ServerSettingsFileSelection?.Close();
            }
        }
    }
}
