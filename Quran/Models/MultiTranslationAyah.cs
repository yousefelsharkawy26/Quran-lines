// Models/MultiTranslationAyah.cs
namespace Quran.Models;

public class MultiTranslationAyah
{
    public string ArabicText { get; set; } = string.Empty;
    public Dictionary<string, string> Translations { get; set; } = new();
    public int SurahNumber { get; set; }
    public int AyahNumber { get; set; }
    public string SurahName { get; set; } = string.Empty;
    public string SurahNameArabic { get; set; } = string.Empty;
    public int Juz { get; set; }
    public int Hizb { get; set; }
    public int Page { get; set; }
    public Dictionary<string, List<string>> TranslationSegments { get; set; } = new();
    public List<string> ArabicSegments { get; set; } = new();
}


