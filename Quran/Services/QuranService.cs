// Services/QuranService.cs
using Quran.Models;
using QuranLinesService.Services.Interfaces;
using System.Text.Json;

namespace QuranLinesService.Services;

public class QuranService : IQuranService
{
    private readonly HttpClient _httpClient;
    private readonly ITextSegmentationService _segmentationService;
    private readonly ILogger<QuranService> _logger;

    // خريطة الترجمات المتاحة
    private readonly Dictionary<string, string> _translations = new()
    {
        { "arabic", "ar" },
        { "english", "en.sahih" },
        { "urdu", "ur.jalandhry" },
        { "french", "fr.hamidullah" },
        { "german", "de.aburida" },
        { "spanish", "es.cortes" },
        { "turkish", "tr.diyanet" },
        { "indonesian", "id.muntakhab" },
        { "russian", "ru.kuliev" },
        { "chinese", "zh.jian" },
        { "bengali", "bn.bengali" }
    };

    public QuranService(HttpClient httpClient, ITextSegmentationService segmentationService, ILogger<QuranService> logger)
    {
        _httpClient = httpClient;
        _segmentationService = segmentationService;
        _logger = logger;

        // إعداد HttpClient
        _httpClient.BaseAddress = new Uri("https://api.alquran.cloud/v1/");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<QuranResponse> GetQuranLinesAsync(QuranRequest request)
    {
        try
        {
            _logger.LogInformation($"Processing request: Surah {request.SurahNumber}, Hizb {request.HizbNumber}, Page {request.PageNumber}");

            // التحقق من صحة الطلب
            if (!await ValidateRequestAsync(request))
            {
                return new QuranResponse
                {
                    Success = false,
                    Message = "طلب غير صحيح - تحقق من الأرقام المدخلة"
                };
            }

            // الحصول على النص العربي
            var arabicLines = await GetArabicLinesAsync(request);
            if (!arabicLines.Any())
            {
                return new QuranResponse
                {
                    Success = false,
                    Message = "لم يتم العثور على نصوص للمعايير المحددة"
                };
            }

            // الحصول على الترجمة
            var translationKey = GetTranslationKey(request.Translation ?? "english");
            var translatedLines = await GetTranslatedLinesAsync(request, translationKey);

            // دمج النصوص مع الترجمات وتقسيمها
            var processedLines = ProcessLines(arabicLines, translatedLines, request);

            return new QuranResponse
            {
                Success = true,
                Message = "تم استرداد البيانات بنجاح",
                Lines = processedLines,
                RequestInfo = request
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching Quran data");
            return new QuranResponse
            {
                Success = false,
                Message = "خطأ في الاتصال بالخدمة - يرجى المحاولة لاحقاً"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error occurred");
            return new QuranResponse
            {
                Success = false,
                Message = "خطأ في معالجة البيانات المستلمة"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            return new QuranResponse
            {
                Success = false,
                Message = "حدث خطأ غير متوقع - يرجى المحاولة لاحقاً"
            };
        }
    }

    public async Task<bool> ValidateRequestAsync(QuranRequest request)
    {
        try
        {
            // التحقق من الأرقام الأساسية
            if (request.SurahNumber < 1 || request.SurahNumber > 114) return false;
            if (request.HizbNumber < 1 || request.HizbNumber > 30) return false;
            if (request.PageNumber < 1 || request.PageNumber > 604) return false;

            // التحقق من توفر الترجمة
            var translationKey = GetTranslationKey(request.Translation ?? "english");
            if (string.IsNullOrEmpty(translationKey)) return false;

            // التحقق من وجود السورة (اختياري - يمكن إزالته لتوفير استدعاءات API)
            var response = await _httpClient.GetAsync($"surah/{request.SurahNumber}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<MushafLinesResponse> GetMushafLinesAsync(QuranMushafLinesRequest request)
    {
        try
        {
            _logger.LogInformation($"Processing Mushaf request: Surah {request.SurahNumber}, Page {request.PageNumber}");

            // التحقق من صحة الطلب
            if (!await ValidateMushafRequestAsync(request))
            {
                return new MushafLinesResponse
                {
                    Success = false,
                    Message = "طلب غير صحيح - تحقق من الأرقام المدخلة"
                };
            }

            // الحصول على البيانات الأساسية
            var basicRequest = new QuranRequest
            {
                SurahNumber = request.SurahNumber,
                HizbNumber = request.HizbNumber,
                PageNumber = request.PageNumber,
                Translation = request.Translation
            };

            var basicResponse = await GetQuranLinesAsync(basicRequest);
            if (!basicResponse.Success || !basicResponse.Lines.Any())
            {
                return new MushafLinesResponse
                {
                    Success = false,
                    Message = "لم يتم العثور على نصوص للمعايير المحددة"
                };
            }

            // تحويل للسطور حسب المصحف
            var mushafLines = _segmentationService.CreateMushafLines(basicResponse.Lines, request.LinesPerPage);

            return new MushafLinesResponse
            {
                Success = true,
                Message = "تم استرداد سطور المصحف بنجاح",
                MushafLines = mushafLines,
                RequestInfo = request,
                TotalLines = mushafLines.Count,
                TotalAyahs = basicResponse.Lines.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Mushaf lines request");
            return new MushafLinesResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء معالجة طلب سطور المصحف"
            };
        }
    }

    public async Task<MultiTranslationResponse> GetMultiTranslationAyahAsync(MultiTranslationRequest request)
    {
        try
        {
            _logger.LogInformation($"Processing multi-translation request: Surah {request.SurahNumber}, Ayah {request.AyahNumber}");

            // التحقق من صحة الطلب
            if (!await ValidateMultiTranslationRequestAsync(request))
            {
                return new MultiTranslationResponse
                {
                    Success = false,
                    Message = "طلب غير صحيح - تحقق من الأرقام والترجمات المدخلة"
                };
            }

            var ayah = new MultiTranslationAyah
            {
                SurahNumber = request.SurahNumber,
                AyahNumber = request.AyahNumber,
                Translations = new Dictionary<string, string>(),
                TranslationSegments = new Dictionary<string, List<string>>()
            };

            var failedTranslations = new List<string>();

            // الحصول على النص العربي إذا كان مطلوباً
            if (request.IncludeArabic)
            {
                var arabicData = await GetSingleAyahAsync(request.SurahNumber, request.AyahNumber, "quran-uthmani");
                if (arabicData != null)
                {
                    ayah.ArabicText = GetTextFromAyahData(arabicData);
                    ayah.SurahName = GetSurahNameFromAyahData(arabicData);
                    ayah.SurahNameArabic = GetSurahNameArabicFromAyahData(arabicData);
                    ayah.Page = GetPageFromAyahData(arabicData);
                    ayah.Juz = GetJuzFromAyahData(arabicData);
                    ayah.Hizb = GetHizbFromAyahData(arabicData);

                    // تقسيم النص العربي
                    ayah.ArabicSegments = _segmentationService.SmartSegment(ayah.ArabicText, "arabic");
                }
            }

            // الحصول على الترجمات المطلوبة
            foreach (var translation in request.Translations)
            {
                var translationKey = GetTranslationKey(translation);
                if (string.IsNullOrEmpty(translationKey))
                {
                    failedTranslations.Add(translation);
                    continue;
                }

                try
                {
                    var translationData = await GetSingleAyahAsync(request.SurahNumber, request.AyahNumber, translationKey);
                    if (translationData != null)
                    {
                        var translationText = GetTextFromAyahData(translationData);
                        ayah.Translations[translation] = translationText;

                        // تقسيم الترجمة
                        ayah.TranslationSegments[translation] = _segmentationService.SmartSegment(translationText, translation);
                    }
                    else
                    {
                        failedTranslations.Add(translation);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to fetch translation {translation} for Surah {request.SurahNumber}, Ayah {request.AyahNumber}");
                    failedTranslations.Add(translation);
                }
            }

            return new MultiTranslationResponse
            {
                Success = true,
                Message = $"تم استرداد {ayah.Translations.Count} ترجمة بنجاح",
                Ayah = ayah,
                RequestInfo = request,
                AvailableTranslations = GetAvailableTranslations(),
                FailedTranslations = failedTranslations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing multi-translation request");
            return new MultiTranslationResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء معالجة طلب الترجمات المتعددة"
            };
        }
    }

    public async Task<bool> ValidateMushafRequestAsync(QuranMushafLinesRequest request)
    {
        try
        {
            // التحقق من الأرقام الأساسية
            if (request.SurahNumber < 1 || request.SurahNumber > 114) return false;
            if (request.HizbNumber < 1 || request.HizbNumber > 30) return false;
            if (request.PageNumber < 1 || request.PageNumber > 604) return false;
            if (request.LinesPerPage < 10 || request.LinesPerPage > 20) return false;

            // التحقق من توفر الترجمة
            var translationKey = GetTranslationKey(request.Translation ?? "english");
            return !string.IsNullOrEmpty(translationKey);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateMultiTranslationRequestAsync(MultiTranslationRequest request)
    {
        try
        {
            // التحقق من الأرقام الأساسية
            if (request.SurahNumber < 1 || request.SurahNumber > 114) return false;
            if (request.AyahNumber < 1) return false;
            if (!request.Translations.Any()) return false;

            // التحقق من وجود السورة والآية
            var response = await _httpClient.GetAsync($"ayah/{request.SurahNumber}:{request.AyahNumber}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<JsonElement?> GetSingleAyahAsync(int surahNumber, int ayahNumber, string edition)
    {
        try
        {
            var url = $"ayah/{surahNumber}:{ayahNumber}/{edition}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch ayah {surahNumber}:{ayahNumber} with edition {edition}");
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(jsonContent);

            if (data.TryGetProperty("data", out var dataElement))
            {
                return dataElement;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching single ayah {surahNumber}:{ayahNumber}");
            return null;
        }
    }

    private string GetTextFromAyahData(JsonElement? ayahData)
    {
        return ayahData.Value.TryGetProperty("text", out var textElement)
            ? textElement.GetString() ?? "" : "";
    }

    private string GetSurahNameFromAyahData(JsonElement? ayahData)
    {
        return ayahData.Value.TryGetProperty("surah", out var surahElement) &&
               surahElement.TryGetProperty("englishName", out var nameElement)
            ? nameElement.GetString() ?? "" : "";
    }

    private string GetSurahNameArabicFromAyahData(JsonElement? ayahData)
    {
        return ayahData.Value.TryGetProperty("surah", out var surahElement) &&
               surahElement.TryGetProperty("name", out var nameElement)
            ? nameElement.GetString() ?? "" : "";
    }

    private int GetPageFromAyahData(JsonElement? ayahData)
    {
        return ayahData.Value.TryGetProperty("page", out var pageElement)
            ? pageElement.GetInt32() : 0;
    }

    private int GetJuzFromAyahData(JsonElement? ayahData)
    {
        return ayahData.Value.TryGetProperty("juz", out var juzElement)
            ? juzElement.GetInt32() : 0;
    }

    private int GetHizbFromAyahData(JsonElement? ayahData)
    {
        return ayahData.Value.TryGetProperty("hizbQuarter", out var hizbElement)
            ? (hizbElement.GetInt32() + 3) / 4 : 0; // تحويل ربع الحزب إلى رقم الحزب
    }

    public List<string> GetAvailableTranslations()
    {
        return _translations.Keys.ToList();
    }

    private async Task<List<dynamic>> GetArabicLinesAsync(QuranRequest request)
    {
        var url = $"page/{request.PageNumber}/quran-uthmani";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Failed to fetch Arabic text for page {request.PageNumber}");
            return new List<dynamic>();
        }

        var jsonContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(jsonContent);

        if (data.TryGetProperty("data", out var dataElement) &&
            dataElement.TryGetProperty("ayahs", out var ayahsElement))
        {
            return ayahsElement.EnumerateArray()
                .Where(ayah => ayah.TryGetProperty("surah", out var surahElement) &&
                              surahElement.TryGetProperty("number", out var numberElement) &&
                              numberElement.GetInt32() == request.SurahNumber)
                .Select(ayah => (dynamic)ayah)
                .ToList();
        }

        return new List<dynamic>();
    }

    private async Task<List<dynamic>> GetTranslatedLinesAsync(QuranRequest request, string translationKey)
    {
        var url = $"page/{request.PageNumber}/{translationKey}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Failed to fetch translation for page {request.PageNumber} with key {translationKey}");
            return new List<dynamic>();
        }

        var jsonContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(jsonContent);

        if (data.TryGetProperty("data", out var dataElement) &&
            dataElement.TryGetProperty("ayahs", out var ayahsElement))
        {
            return ayahsElement.EnumerateArray()
                .Where(ayah => ayah.TryGetProperty("surah", out var surahElement) &&
                              surahElement.TryGetProperty("number", out var numberElement) &&
                              numberElement.GetInt32() == request.SurahNumber)
                .Select(ayah => (dynamic)ayah)
                .ToList();
        }

        return new List<dynamic>();
    }

    private List<QuranLine> ProcessLines(List<dynamic> arabicLines, List<dynamic> translatedLines, QuranRequest request)
    {
        var processedLines = new List<QuranLine>();

        for (int i = 0; i < arabicLines.Count; i++)
        {
            var arabicAyah = (JsonElement)arabicLines[i];
            var translatedAyah = i < translatedLines.Count ? (JsonElement)translatedLines[i] : new JsonElement();

            var arabicText = arabicAyah.TryGetProperty("text", out var arabicTextElement)
                ? arabicTextElement.GetString() ?? "" : "";

            var translationText = translatedAyah.TryGetProperty("text", out var translationTextElement)
                ? translationTextElement.GetString() ?? "" : "";

            var surahName = arabicAyah.TryGetProperty("surah", out var surahElement) &&
                           surahElement.TryGetProperty("englishName", out var nameElement)
                ? nameElement.GetString() ?? "" : "";

            // تقسيم النصوص
            var arabicSegments = _segmentationService.SmartSegment(arabicText, "arabic");
            var translationSegments = _segmentationService.SmartSegment(translationText, request.Translation ?? "english");

            // موازنة عدد القطع
            BalanceSegments(ref arabicSegments, ref translationSegments);

            var quranLine = new QuranLine
            {
                ArabicText = arabicText,
                Translation = translationText,
                LineNumber = i + 1,
                SurahNumber = request.SurahNumber,
                SurahName = surahName,
                PageNumber = request.PageNumber,
                HizbNumber = request.HizbNumber,
                ArabicSegments = arabicSegments,
                TranslationSegments = translationSegments
            };

            processedLines.Add(quranLine);
        }

        return processedLines;
    }

    private void BalanceSegments(ref List<string> arabicSegments, ref List<string> translationSegments)
    {
        int maxCount = Math.Max(arabicSegments.Count, translationSegments.Count);

        // إذا كان عدد القطع العربية أقل، قم بدمج بعض قطع الترجمة
        if (arabicSegments.Count < translationSegments.Count)
        {
            translationSegments = CombineSegmentsToCount(translationSegments, arabicSegments.Count);
        }
        // إذا كان عدد قطع الترجمة أقل، قم بدمج بعض القطع العربية
        else if (translationSegments.Count < arabicSegments.Count)
        {
            arabicSegments = CombineSegmentsToCount(arabicSegments, translationSegments.Count);
        }
    }

    private List<string> CombineSegmentsToCount(List<string> segments, int targetCount)
    {
        if (segments.Count <= targetCount) return segments;

        var combined = new List<string>();
        int segmentsPerGroup = (int)Math.Ceiling((double)segments.Count / targetCount);

        for (int i = 0; i < segments.Count; i += segmentsPerGroup)
        {
            var group = segments.Skip(i).Take(segmentsPerGroup);
            combined.Add(string.Join(" ", group));
        }

        return combined;
    }

    private string GetTranslationKey(string translation)
    {
        return _translations.TryGetValue(translation.ToLower(), out var key) ? key : _translations["english"];
    }
}


