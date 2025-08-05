// Models/QuranLine.cs
namespace Quran.Models;

public class QuranLine
{
    public string ArabicText { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public int SurahNumber { get; set; }
    public string SurahName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int HizbNumber { get; set; }
    public List<string> ArabicSegments { get; set; } = new();
    public List<string> TranslationSegments { get; set; } = new();
}


