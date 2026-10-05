using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

using R = TrayToolbar.Resources.Resources;

namespace TrayToolbar.Tests;

[TestClass]
public class ResourcesTests
{
    static IEnumerable<object[]> TranslatedLanguages => SettingsForm.SupportedLanguages
        .Where(culture => culture.TwoLetterISOLanguageName != "en")
        .Select(culture => new object[] { culture.Name });

    [TestMethod]
    [DynamicData(nameof(TranslatedLanguages))]
    public void Every_supported_language_translates_every_string(string language)
    {
        var neutral = StringResources(CultureInfo.InvariantCulture);
        var translated = StringResources(CultureInfo.GetCultureInfo(language));

        var missing = neutral.Keys.Except(translated.Keys).Order().ToArray();
        Assert.IsEmpty(missing, $"{language} is missing: {string.Join(", ", missing)}");
        foreach (var (key, value) in translated)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"{language} has an empty value for '{key}'");
        }
    }

    [TestMethod]
    [DynamicData(nameof(TranslatedLanguages))]
    public void Translations_keep_the_format_placeholders(string language)
    {
        var neutral = StringResources(CultureInfo.InvariantCulture);
        var translated = StringResources(CultureInfo.GetCultureInfo(language));

        foreach (var (key, value) in neutral)
        {
            if (!translated.TryGetValue(key, out var translation)) continue;
            CollectionAssert.AreEquivalent(Placeholders(value), Placeholders(translation), $"{language}: '{key}'");
        }
    }

    static Dictionary<string, string> StringResources(CultureInfo culture)
    {
        var resourceSet = R.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        Assert.IsNotNull(resourceSet, $"No resources for '{culture.Name}'");
        return resourceSet.Cast<DictionaryEntry>()
            .Where(entry => entry.Value is string)
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
    }

    static string[] Placeholders(string value) => Regex.Matches(value, @"\{\d+\}").Select(m => m.Value).ToArray();
}
