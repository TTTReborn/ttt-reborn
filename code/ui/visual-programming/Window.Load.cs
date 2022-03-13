using System.Collections.Generic;
using System.Text.Json;

using Sandbox;
using Sandbox.UI.Construct;

namespace TTTReborn.UI.VisualProgramming
{
    public partial class Window
    {
        public void Load()
        {
            _currentFileSelection?.Close();

            FileSelection fileSelection = FindRootPanel().Add.FileSelection();
            fileSelection.DefaultSelectionFileType = $"*{VISUALPROGRAMMING_FILE_EXTENSION}";
            fileSelection.OnAgree = () => OnAgreeLoadVisualProgrammingFrom(fileSelection);
            fileSelection.DefaultSelectionPath = GetSettingsPathByData(Utils.Realm.Client);
            fileSelection.Display();

            _currentFileSelection = fileSelection;
        }

        private void OnAgreeLoadVisualProgrammingFrom(FileSelection fileSelection)
        {
            if (fileSelection.SelectedEntry == null)
            {
                return;
            }

            string fileName = fileSelection.SelectedEntry.FileNameLabel.Text;

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            fileName = fileName.Split('/')[^1].Split('.')[0];

            if (Instance == null)
            {
                fileSelection.Close();

                return;
            }

            Dictionary<string, object> jsonData = Player.LoadVisualProgramming(fileSelection.CurrentFolderPath, fileName + VISUALPROGRAMMING_FILE_EXTENSION, Utils.Realm.Client);

            if (jsonData == null)
            {
                Log.Error($"VisualProgramming file '{fileSelection.CurrentFolderPath}{fileName}{VISUALPROGRAMMING_FILE_EXTENSION}' can't be loaded.");

                return;
            }

            LoadWorkspace(jsonData);

            fileSelection.Close();
        }

        private void LoadWorkspace(Dictionary<string, object> jsonData)
        {
            // TODO load workspace settings from jsonData as well

            jsonData.TryGetValue("Nodes", out object saveListJson);

            if (saveListJson == null)
            {
                return;
            }

            foreach (Node node in Nodes)
            {
                node.Delete(true);
            }

            Nodes.Clear();
            MainNode = null;

            LoadNodesFromStackJson(((JsonElement) saveListJson).GetRawText());
        }
    }
}

namespace TTTReborn
{
    public partial class Player
    {
        internal static Dictionary<string, object> LoadVisualProgramming(string path, string fileName, Utils.Realm realm)
        {
            path = Utils.GetSettingsFolderPath(realm, path);

            if (FileSystem.Data.FileExists(path + fileName))
            {
                string jsonData = FileSystem.Data.ReadAllText(path + fileName);

                if (jsonData == null || string.IsNullOrEmpty(jsonData))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData);
            }

            return null;
        }
    }
}
