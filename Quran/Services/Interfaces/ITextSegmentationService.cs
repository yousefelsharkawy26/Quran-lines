// Services/Interfaces/ITextSegmentationService.cs
// Services/Interfaces/ITextSegmentationService.cs
using Quran.Models;

namespace QuranLinesService.Services.Interfaces
{
    public interface ITextSegmentationService
    {
        List<string> SegmentArabicText(string arabicText);
        List<string> SegmentEnglishText(string englishText);
        List<string> SegmentOtherLanguageText(string text, string language);
        List<string> SmartSegment(string text, string language, int maxSegments = 5);
        List<string> CombineSegmentsToMushafLine(List<string> segments, int targetLineLength = 60);
        List<MushafLine> CreateMushafLines(List<QuranLine> quranLines, int linesPerPage = 15);
    }
}