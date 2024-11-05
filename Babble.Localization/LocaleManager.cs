namespace Babble.Locale;

using Babble.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class LocaleManager
{
    public static LocaleManager Instance { get; private set; }

    public static event Action OnLocaleChanged;

    static LocaleManager()
    {
        Instance = new LocaleManager();
    }

    private LocaleManager()
    {
        if (Instance is not null) return;
        Instance = this;
        
        _currentLanguage = BabbleCore.Instance.Settings.GetSetting<string>("gui_language");
        LoadLanguages(Path.Combine(
            AppContext.BaseDirectory,
            "Locale"
            ));
        ChangeLanguage(_currentLanguage);
    }

    private readonly HashSet<string> _languages = [];
    private readonly Dictionary<string, Dictionary<string, string>> _strings = [];
    private string _currentLanguage;

    public string this[string key] =>
        Instance._strings[Instance._currentLanguage].ContainsKey($"locale.{key}") ?
        Instance._strings[Instance._currentLanguage][$"locale.{key}"] :
        key;

    public string GetLanguage() => Instance._currentLanguage;
    public HashSet<string> GetLanguages() => Instance._languages;

    public void ChangeLanguage(string language)
    {
        Instance._currentLanguage = language;
        OnLocaleChanged?.Invoke();
    }

    public void ChangeLanguage(int index)
    {
        Instance._currentLanguage = GetLanguages().ElementAt(index);
        OnLocaleChanged?.Invoke();
    }

    private void LoadLanguages(string localeDirectory)
    {
        // Load available languages (directories)
        foreach (var dir in Directory.GetDirectories(localeDirectory))
        {
            var lang = Path.GetFileName(dir);
            Instance._languages.Add(lang);
            Instance._strings[lang] = [];

            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                var fileContent = File.ReadAllText(file);
                var fileStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent)!;
                var fileName = Path.GetFileNameWithoutExtension(file);

                foreach (var kvp in fileStrings)
                {
                    Instance._strings[lang][$"{fileName}.{kvp.Key}"] = kvp.Value;
                }
            }
        }
    }
}

