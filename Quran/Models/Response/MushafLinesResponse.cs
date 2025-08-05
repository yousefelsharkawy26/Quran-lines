// Models/MushafLinesResponse.cs
using Quran.Models;

namespace Quran.Models;

public class MushafLinesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<MushafLine> MushafLines { get; set; } = new();
    public QuranMushafLinesRequest? RequestInfo { get; set; }
    public int TotalLines { get; set; }
    public int TotalAyahs { get; set; }
}


