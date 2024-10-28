namespace Babble.Maui.Locale;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class LocaleManager
{
    private static readonly Lazy<LocaleManager> _instance =
        new(() => new LocaleManager());

    private readonly HashSet<string> _languages;
    private readonly Dictionary<string, Dictionary<string, string>> _strings;
    private string _currentLanguage;

    // Private constructor to prevent direct instantiation
    private LocaleManager()
    {
        _languages = [];
        _strings = [];
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
            _strings[lang] = [];

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

    public static HashSet<string> GetLanguages()
    {
        return Instance._languages;
    }

    public static string GetString(string pattern)
    {
        var key = $"locale.{pattern}";

        if (!Instance._strings[Instance._currentLanguage].TryGetValue(key, out string? value))
        {
            throw new KeyNotFoundException($"String pattern '{key}' not found for language '{Instance._currentLanguage}'");
        }

        return value;
    }

    public static void UpdateLanguage(string language)
    {
        Instance.LoadLanguage(language);
    }
}

