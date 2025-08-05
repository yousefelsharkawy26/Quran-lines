// Services/TextSegmentationService.cs
using Quran.Models;
using QuranLinesService.Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace QuranLinesService.Services;

public class TextSegmentationService : ITextSegmentationService
{
    // علامات الوقف القرآنية
    private readonly string[] _quranicStopMarks = { "ۚ", "ۛ", "ۖ", "ۗ", "ۘ", "ۙ", "۞" };

    // أدوات الربط العربية
    private readonly string[] _arabicConnectors = { "و", "أو", "إذا", "إذ", "إن", "أن", "كان", "قال", "قل" };

    // أدوات الربط الإنجليزية
    private readonly string[] _englishConnectors = { "and", "but", "because", "however", "therefore", "moreover", "furthermore", "nevertheless" };

    public List<string> SegmentArabicText(string arabicText)
    {
        if (string.IsNullOrWhiteSpace(arabicText))
            return new List<string>();

        var segments = new List<string>();

        // أولاً: التقسيم حسب علامات الوقف القرآنية
        var stopMarkSegments = SplitByQuranicStopMarks(arabicText);

        foreach (var segment in stopMarkSegments)
        {
            if (segment.Length <= 50) // إذا كان القطعة قصيرة، اتركها كما هي
            {
                segments.Add(segment.Trim());
            }
            else
            {
                // تقسيم إضافي للقطع الطويلة
                var subSegments = SplitByArabicConnectors(segment);
                segments.AddRange(subSegments.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
            }
        }

        return segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    public List<string> SegmentEnglishText(string englishText)
    {
        if (string.IsNullOrWhiteSpace(englishText))
            return new List<string>();

        var segments = new List<string>();

        // التقسيم حسب النقاط والفواصل
        var sentences = englishText.Split(new char[] { '.', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var sentence in sentences)
        {
            if (sentence.Length <= 80) // طول مناسب للجملة الإنجليزية
            {
                segments.Add(sentence.Trim());
            }
            else
            {
                // تقسيم الجمل الطويلة حسب الفواصل وأدوات الربط
                var subSegments = SplitByEnglishConnectors(sentence);
                segments.AddRange(subSegments.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
            }
        }

        return segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    public List<string> SegmentOtherLanguageText(string text, string language)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        // تقسيم متوازن للغات الأخرى
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var segments = new List<string>();
        var currentSegment = new List<string>();
        int maxWordsPerSegment = GetMaxWordsPerSegment(language);

        foreach (var word in words)
        {
            currentSegment.Add(word);

            if (currentSegment.Count >= maxWordsPerSegment)
            {
                segments.Add(string.Join(" ", currentSegment));
                currentSegment.Clear();
            }
        }

        // إضافة الجزء المتبقي إذا وُجد
        if (currentSegment.Any())
        {
            segments.Add(string.Join(" ", currentSegment));
        }

        return segments;
    }

    public List<string> SmartSegment(string text, string language, int maxSegments = 5)
    {
        List<string> segments;

        switch (language.ToLower())
        {
            case "arabic":
            case "ar":
                segments = SegmentArabicText(text);
                break;
            case "english":
            case "en":
                segments = SegmentEnglishText(text);
                break;
            default:
                segments = SegmentOtherLanguageText(text, language);
                break;
        }

        // التأكد من عدم تجاوز الحد الأقصى للقطع
        if (segments.Count > maxSegments)
        {
            segments = CombineSegments(segments, maxSegments);
        }

        return segments;
    }

    private List<string> SplitByQuranicStopMarks(string text)
    {
        var segments = new List<string> { text };

        foreach (var stopMark in _quranicStopMarks)
        {
            var newSegments = new List<string>();
            foreach (var segment in segments)
            {
                var parts = segment.Split(new string[] { stopMark }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i < parts.Length - 1)
                        newSegments.Add(parts[i] + stopMark);
                    else
                        newSegments.Add(parts[i]);
                }
            }
            segments = newSegments;
        }

        return segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    private List<string> SplitByArabicConnectors(string text)
    {
        var segments = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentSegment = new List<string>();

        foreach (var word in words)
        {
            currentSegment.Add(word);

            // إذا كان الكلمة من أدوات الربط وليست في بداية الجملة
            if (_arabicConnectors.Any(conn => word.Contains(conn)) && currentSegment.Count > 3)
            {
                segments.Add(string.Join(" ", currentSegment));
                currentSegment.Clear();
            }
            else if (currentSegment.Count >= 8) // حد أقصى للكلمات في القطعة
            {
                segments.Add(string.Join(" ", currentSegment));
                currentSegment.Clear();
            }
        }

        if (currentSegment.Any())
        {
            segments.Add(string.Join(" ", currentSegment));
        }

        return segments;
    }

    private List<string> SplitByEnglishConnectors(string text)
    {
        var segments = new List<string>();

        // استخدام regex للتقسيم حسب أدوات الربط
        string pattern = @"\b(" + string.Join("|", _englishConnectors) + @")\b";
        var parts = Regex.Split(text, pattern, RegexOptions.IgnoreCase);

        string currentSegment = "";

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part)) continue;

            currentSegment += part;

            if (_englishConnectors.Contains(part.Trim().ToLower()) || currentSegment.Length > 60)
            {
                if (!string.IsNullOrWhiteSpace(currentSegment))
                {
                    segments.Add(currentSegment.Trim());
                    currentSegment = "";
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(currentSegment))
        {
            segments.Add(currentSegment.Trim());
        }

        return segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    private int GetMaxWordsPerSegment(string language)
    {
        return language.ToLower() switch
        {
            "french" or "fr" => 10,
            "german" or "de" => 8,
            "spanish" or "es" => 12,
            "urdu" or "ur" => 8,
            "turkish" or "tr" => 10,
            _ => 10
        };
    }

    private List<string> CombineSegments(List<string> segments, int maxSegments)
    {
        if (segments.Count <= maxSegments) return segments;

        var combined = new List<string>();
        int segmentsPerGroup = (int)Math.Ceiling((double)segments.Count / maxSegments);

        for (int i = 0; i < segments.Count; i += segmentsPerGroup)
        {
            var group = segments.Skip(i).Take(segmentsPerGroup);
            combined.Add(string.Join(" ", group));
        }

        return combined;
    }

    public List<string> CombineSegmentsToMushafLine(List<string> segments, int targetLineLength = 60)
    {
        if (!segments.Any()) return new List<string>();

        var mushafLines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var segment in segments)
        {
            // إذا كان إضافة هذا الجزء سيتجاوز الطول المطلوب
            if (currentLine.Length + segment.Length + 1 > targetLineLength && currentLine.Length > 0)
            {
                mushafLines.Add(currentLine.ToString().Trim());
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
                currentLine.Append(" ");

            currentLine.Append(segment);
        }

        // إضافة السطر الأخير إذا كان هناك محتوى
        if (currentLine.Length > 0)
        {
            mushafLines.Add(currentLine.ToString().Trim());
        }

        return mushafLines;
    }

    public List<MushafLine> CreateMushafLines(List<QuranLine> quranLines, int linesPerPage = 15)
    {
        var mushafLines = new List<MushafLine>();
        var allArabicText = new List<string>();
        var allTranslationText = new List<string>();
        var ayahMapping = new List<(int ayahNumber, int startIndex, int endIndex)>();

        // جمع كل النصوص مع تتبع الآيات
        int currentIndex = 0;
        for (int i = 0; i < quranLines.Count; i++)
        {
            var line = quranLines[i];
            var arabicSegments = SmartSegment(line.ArabicText, "arabic", 10);
            var translationSegments = SmartSegment(line.Translation, "english", 10);

            int startIndex = currentIndex;
            allArabicText.AddRange(arabicSegments);
            allTranslationText.AddRange(translationSegments);
            currentIndex += arabicSegments.Count;

            ayahMapping.Add((i + 1, startIndex, currentIndex - 1));
        }

        // تجميع الجزئيات في سطور مصحف
        var arabicMushafLines = CombineSegmentsToMushafLine(allArabicText, 70);
        var translationMushafLines = CombineSegmentsToMushafLine(allTranslationText, 80);

        // إنشاء سطور المصحف مع ربطها بالآيات
        int maxLines = Math.Max(arabicMushafLines.Count, translationMushafLines.Count);
        maxLines = Math.Min(maxLines, linesPerPage); // تحديد عدد السطور حسب المصحف

        for (int lineIndex = 0; lineIndex < maxLines; lineIndex++)
        {
            var mushafLine = new MushafLine
            {
                LineNumber = lineIndex + 1,
                ArabicLine = lineIndex < arabicMushafLines.Count ? arabicMushafLines[lineIndex] : "",
                TranslationLine = lineIndex < translationMushafLines.Count ? translationMushafLines[lineIndex] : "",
                SurahNumber = quranLines.FirstOrDefault()?.SurahNumber ?? 0,
                SurahName = quranLines.FirstOrDefault()?.SurahName ?? "",
                PageNumber = quranLines.FirstOrDefault()?.PageNumber ?? 0,
                HizbNumber = quranLines.FirstOrDefault()?.HizbNumber ?? 0,
                OriginalSegments = new List<string>(),
                TranslationSegments = new List<string>(),
                AyahNumbers = new List<int>(),
                IsCompleteLine = true
            };

            // تحديد الآيات المرتبطة بهذا السطر (تقدير تقريبي)
            int estimatedAyahsPerLine = Math.Max(1, quranLines.Count / maxLines);
            int startAyah = lineIndex * estimatedAyahsPerLine + 1;
            int endAyah = Math.Min(startAyah + estimatedAyahsPerLine - 1, quranLines.Count);

            for (int ayah = startAyah; ayah <= endAyah; ayah++)
            {
                mushafLine.AyahNumbers.Add(ayah);
            }

            mushafLines.Add(mushafLine);
        }

        return mushafLines;
    }


}