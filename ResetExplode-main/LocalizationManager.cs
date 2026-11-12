using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSKGamingSuite;

public static class LocalizationManager
{
    private static readonly string SettingsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CSKGamingSuite", "language.json");

    public static string CurrentLanguage { get; private set; } = "pt";

    public static readonly Dictionary<string, string> SupportedLanguages = new()
    {
        { "pt", "Português (Brasil)" },
        { "en", "English (US)" },
        { "es", "Español" },
        { "ru", "Русский" },
        { "zh", "中文 (简体)" }
    };

    // Dicionários em memória: Idioma -> (TextoOriginal -> Tradução)
    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new(StringComparer.OrdinalIgnoreCase);

    public static event Action? LanguageChanged;

    public static void Initialize()
    {
        LoadAllTranslations();

        string saved = LoadSavedLanguage();
        if (string.IsNullOrEmpty(saved) || !SupportedLanguages.ContainsKey(saved))
        {
            string sysLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            saved = SupportedLanguages.ContainsKey(sysLang) ? sysLang : "pt";
        }
        SetLanguage(saved);
    }

    private static void LoadAllTranslations()
    {
        string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "I18n");
        // Também tenta o diretório do projeto se executando em debug/dev
        if (!Directory.Exists(dir))
        {
            dir = Path.Combine(AppContext.BaseDirectory, "I18n");
        }

        foreach (var lang in SupportedLanguages.Keys)
        {
            _translations[lang] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string jsonFile = Path.Combine(dir, $"{lang}.json");
            if (File.Exists(jsonFile))
            {
                try
                {
                    string content = File.ReadAllText(jsonFile);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                            _translations[lang][kvp.Key] = kvp.Value;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading {jsonFile}: {ex.Message}");
                }
            }
        }
    }

    public static void SetLanguage(string langCode)
    {
        if (!SupportedLanguages.ContainsKey(langCode))
            langCode = "pt";

        CurrentLanguage = langCode;

        // Atualiza ResourceDictionary XAML também
        string dictPath = $"I18n/{langCode}.xaml";
        try
        {
            var uri = new Uri(dictPath, UriKind.Relative);
            var newDict = new ResourceDictionary { Source = uri };

            ResourceDictionary? oldDict = null;
            foreach (var d in Application.Current.Resources.MergedDictionaries)
            {
                if (d.Source != null && d.Source.OriginalString.StartsWith("I18n/"))
                {
                    oldDict = d;
                    break;
                }
            }

            if (oldDict != null)
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);

            Application.Current.Resources.MergedDictionaries.Add(newDict);
        }
        catch { }

        SaveLanguage(langCode);
        LanguageChanged?.Invoke();

        if (Application.Current?.MainWindow != null)
        {
            TranslateTree(Application.Current.MainWindow);
        }
    }

    public static string Get(string originalText)
    {
        if (string.IsNullOrWhiteSpace(originalText)) return originalText;
        string key = originalText.Trim();

        if (_translations.TryGetValue(CurrentLanguage, out var dict))
        {
            if (dict.TryGetValue(key, out var trans))
                return trans;
        }
        return originalText;
    }

    public static string GetString(string key, string fallback = "")
    {
        if (Application.Current?.Resources.Contains(key) == true)
        {
            var val = Application.Current.Resources[key];
            if (val is string s) return s;
        }
        return string.IsNullOrEmpty(fallback) ? key : fallback;
    }

    private static readonly Dictionary<DependencyObject, string> _originalTexts = new();

    public static void TranslateTree(DependencyObject parent)
    {
        if (parent == null) return;

        if (parent is TextBlock tb)
        {
            if (!_originalTexts.TryGetValue(tb, out var orig))
            {
                orig = tb.Text;
                if (!string.IsNullOrWhiteSpace(orig))
                    _originalTexts[tb] = orig;
            }

            if (!string.IsNullOrWhiteSpace(orig))
            {
                string trans = Get(orig);
                if (trans != orig)
                    tb.Text = trans;
                else if (CurrentLanguage == "pt")
                    tb.Text = orig;
            }
        }
        else if (parent is Button btn && btn.Content is string btnStr)
        {
            if (!_originalTexts.TryGetValue(btn, out var orig))
            {
                orig = btnStr;
                if (!string.IsNullOrWhiteSpace(orig))
                    _originalTexts[btn] = orig;
            }

            if (!string.IsNullOrWhiteSpace(orig))
            {
                string trans = Get(orig);
                if (trans != orig)
                    btn.Content = trans;
                else if (CurrentLanguage == "pt")
                    btn.Content = orig;
            }
        }
        else if (parent is ContentControl cc && cc.Content is string ccStr)
        {
            if (!_originalTexts.TryGetValue(cc, out var orig))
            {
                orig = ccStr;
                if (!string.IsNullOrWhiteSpace(orig))
                    _originalTexts[cc] = orig;
            }

            if (!string.IsNullOrWhiteSpace(orig))
            {
                string trans = Get(orig);
                if (trans != orig)
                    cc.Content = trans;
                else if (CurrentLanguage == "pt")
                    cc.Content = orig;
            }
        }
        else if (parent is HeaderedContentControl hc && hc.Header is string hdrStr)
        {
            if (!_originalTexts.TryGetValue(hc, out var orig))
            {
                orig = hdrStr;
                if (!string.IsNullOrWhiteSpace(orig))
                    _originalTexts[hc] = orig;
            }

            if (!string.IsNullOrWhiteSpace(orig))
            {
                string trans = Get(orig);
                if (trans != orig)
                    hc.Header = trans;
                else if (CurrentLanguage == "pt")
                    hc.Header = orig;
            }
        }

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            TranslateTree(child);
        }
    }

    private static string LoadSavedLanguage()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Language", out var prop))
                    return prop.GetString() ?? "";
            }
        }
        catch { }
        return "";
    }

    private static void SaveLanguage(string langCode)
    {
        try
        {
            string? dir = Path.GetDirectoryName(SettingsFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(new { Language = langCode });
            File.WriteAllText(SettingsFile, json);
        }
        catch { }
    }
}
