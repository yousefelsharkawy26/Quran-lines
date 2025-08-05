// Models/MushafLine.cs
namespace Quran.Models;

public class MushafLine
{
    public string ArabicLine { get; set; } = string.Empty; // السطر كما يظهر في المصحف
    public string TranslationLine { get; set; } = string.Empty; // ترجمة السطر مجمعة
    public int LineNumber { get; set; }
    public int SurahNumber { get; set; }
    public string SurahName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int HizbNumber { get; set; }
    public List<int> AyahNumbers { get; set; } = new(); // أرقام الآيات في هذا السطر
    public List<string> OriginalSegments { get; set; } = new(); // الجزئيات الأصلية قبل التجميع
    public List<string> TranslationSegments { get; set; } = new(); // جزئيات الترجمة
    public bool IsCompleteLine { get; set; } = true; // هل السطر مكتمل أم مقطوع
}


