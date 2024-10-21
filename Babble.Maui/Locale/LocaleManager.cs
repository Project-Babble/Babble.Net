namespace Babble.Maui.Locale;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class LocaleManager
{
    private static readonly Lazy<LocaleManager> _instance =
        new Lazy<LocaleManager>(() => new LocaleManager());

    private List<string> _languages;
    private Dictionary<string, Dictionary<string, string>> _strings;
    private string _currentLanguage;

    // Private constructor to prevent direct instantiation
    private LocaleManager()
    {
        _languages = new List<string>();
        _strings = new Dictionary<string, Dictionary<string, string>>();
    }

    public static LocaleManager Instance => _instance.Value;

    public static void Initialize(string language)
    {
        Instance.LoadLanguages(Path.Combine(
            AppContext.BaseDirectory, 
            "Locale", 
            "Strings"
            ));
        Instance.LoadLanguage(language);
    }

    private void LoadLanguages(string localeDirectory)
    {
        // Load available languages (directories)
        foreach (var dir in Directory.GetDirectories(localeDirectory))
        {
            var lang = Path.GetFileName(dir);
            _languages.Add(lang);
            _strings[lang] = new Dictionary<string, string>();

            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                var fileContent = File.ReadAllText(file);
                var fileStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);
                var fileName = Path.GetFileNameWithoutExtension(file);

                foreach (var kvp in fileStrings)
                {
                    _strings[lang][$"{fileName}.{kvp.Key}"] = kvp.Value;
                }
            }
        }
    }

    private void LoadLanguage(string language)
    {
        _currentLanguage = language;

        if (!_languages.Contains(_currentLanguage))
        {
            throw new ArgumentException($"Language '{_currentLanguage}' is not supported");
        }
    }

    public static List<string> GetLanguages()
    {
        return Instance._languages;
    }

    public static string GetString(string pattern)
    {
        var key = $"locale.{pattern}";

        if (!Instance._strings[Instance._currentLanguage].ContainsKey(key))
        {
            throw new KeyNotFoundException($"String pattern '{key}' not found for language '{Instance._currentLanguage}'");
        }

        return Instance._strings[Instance._currentLanguage][key];
    }

    public static void UpdateLanguage(string language)
    {
        Instance.LoadLanguage(language);
    }
}

