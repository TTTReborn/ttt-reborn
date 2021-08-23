using System;
using System.Collections.Generic;
using System.IO;

using Sandbox;

namespace TTTReborn.Settings
{
    public partial class Settings
    {
        public Categories.General General { get; set; } = new Categories.General();
    }

    namespace Categories
    {
        public partial class General
        {
            public string Language { get; set; } = Globalization.TTTLanguage.FALLBACK_LANGUAGE;
        }
    }
}

namespace TTTReborn.Globalization
{
    public static class TTTLanguage
    {
        public static readonly Dictionary<string, Language> Languages = new();

        public const string FALLBACK_LANGUAGE = "en-US";

        public static Language ActiveLanguage
        {
            get => _activeLanguage;
            set
            {
                _activeLanguage = value ?? GetLanguageByCode(FALLBACK_LANGUAGE);
            }
        }
        private static Language _activeLanguage;

        public static void LoadLanguages()
        {
            foreach (string file in FileSystem.Mounted.FindFile("/lang/packs/", "*.json", false))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string json = FileSystem.Mounted.ReadAllText($"/lang/packs/{file}");

                Language language = new Language(name, json);

                Languages.Add(language.Data.Code ?? name, language);

                Log.Info($"Added language pack: '{name}'.");
            }

            ActiveLanguage = GetLanguageByCode(FALLBACK_LANGUAGE);
        }

        public static Language GetLanguageByCode(string name)
        {
            Language lang = null;

            if (Languages != null && !String.IsNullOrEmpty(name))
            {
                if (!Languages.TryGetValue(name, out lang))
                {
                    Log.Warning($"Tried to get a language that does not exist: '{name}'.");
                }
            }

            return lang ?? GetLanguageByCode(FALLBACK_LANGUAGE);
        }

        public static void UpdateLanguage(Language language)
        {
            if (language.Data.Code == ActiveLanguage.Data.Code)
            {
                return;
            }

            TTTLanguage.ActiveLanguage = language;

            if (Host.IsClient)
            {
                UI.TranslationLabel.UpdateLanguage(language);
            }
        }

        [Event("tttreborn.settings.instance.change")]
        public static void OnChangeLanguageSettings()
        {
            TTTLanguage.UpdateLanguage(TTTLanguage.GetLanguageByCode(Settings.SettingsManager.Instance.General.Language));
        }
    }
}

namespace TTTReborn.Player
{
    using Globalization;

    public partial class TTTPlayer
    {
        [ClientCmd("ttt_language")]
        public static void ChangeLanguage(string name = null)
        {
            if (name is null)
            {
                Log.Info($"Your current language is set to '{TTTLanguage.ActiveLanguage.Data.Name}' ('{TTTLanguage.ActiveLanguage.Data.Code}').");

                return;
            }

            Language language = TTTLanguage.GetLanguageByCode(name);

            if (language is null)
            {
                Log.Warning($"Language '{name}' does not exist. Please enter an ISO (tag) code (http://www.lingoes.net/en/translator/langcode.htm).");

                return;
            }

            Log.Warning($"You set your language to '{language.Data.Name}'.");

            Settings.SettingsManager.Instance.General.Language = language.Data.Code;

            TTTLanguage.OnChangeLanguageSettings();
        }
    }
}
