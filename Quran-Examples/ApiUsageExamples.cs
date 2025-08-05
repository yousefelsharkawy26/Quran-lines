// Examples/ApiUsageExamples.cs
using Quran.Models;
using System.Text;
using System.Text.Json;

namespace QuranLinesService.Examples;

/// <summary>
/// أمثلة على كيفية استخدام API الخدمة
/// </summary>
public static class ApiUsageExamples
{
    private static readonly HttpClient _httpClient = new();
    private static readonly string _baseUrl = "https://localhost:7234/api/quran"; // قم بتغيير العنوان حسب إعدادك

    /// <summary>
    /// مثال 1: الحصول على سطور من سورة الفاتحة
    /// </summary>
    public static async Task Example1_GetFatihaLines()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== Test 1: Get The Lines of The-Openning Surah ===");
        Console.ResetColor();

        var request = new QuranRequest
        {
            SurahNumber = 1,    // سورة الفاتحة
            HizbNumber = 1,     // الحزب الأول
            PageNumber = 1,     // الصفحة الأولى
            Translation = "english"
        };

        try
        {
            var response = await PostJsonAsync<QuranResponse>($"{_baseUrl}/lines", request);

            if (response?.Success == true)
            {
                Console.WriteLine($"They are found {response.Lines.Count} line");

                foreach (var line in response.Lines)
                {
                    Console.WriteLine($"\nLine {line.LineNumber}:");
                    Console.WriteLine($"Arabic: {line.ArabicText}");
                    Console.WriteLine($"Translation: {line.Translation}");

                    Console.WriteLine("The Current Segmants:");
                    for (int i = 0; i < line.ArabicSegments.Count; i++)
                    {
                        Console.WriteLine($"  {i + 1}. {line.ArabicSegments[i]}");
                    }

                    Console.WriteLine("The Translation Segmants:");
                    for (int i = 0; i < line.TranslationSegments.Count; i++)
                    {
                        Console.WriteLine($"  {i + 1}. {line.TranslationSegments[i]}");
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Wrong: {response?.Message}");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"something is wrong: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// مثال 2: الحصول على سطور بترجمة أردية
    /// </summary>
    public static async Task Example2_GetUrduTranslation()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n=== Test 2: Get the Urdu Language ===");
        Console.ResetColor();

        var request = new QuranRequest
        {
            SurahNumber = 2,    // سورة البقرة
            HizbNumber = 1,
            PageNumber = 2,
            Translation = "urdu"
        };

        try
        {
            var response = await PostJsonAsync<QuranResponse>($"{_baseUrl}/lines", request);

            if (response?.Success == true && response.Lines.Any())
            {
                var firstLine = response.Lines.First();
                Console.WriteLine($"the first line of page {request.PageNumber}:");
                Console.WriteLine($"arabic: {firstLine.ArabicText}");
                Console.WriteLine($"urdy: {firstLine.Translation}");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"something is wrong: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// مثال 3: الحصول على قائمة الترجمات المتاحة
    /// </summary>
    public static async Task Example3_GetAvailableTranslations()
    {
        Console.WriteLine("\n=== Test 3: The available translations ===");

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/translations");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var translations = JsonSerializer.Deserialize<List<string>>(content);

                Console.WriteLine("The Available Translations:");
                foreach (var translation in translations ?? new List<string>())
                {
                    Console.WriteLine($"- {translation}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"something is wrong: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// مثال 4: التحقق من صحة الطلب
    /// </summary>
    public static async Task Example4_ValidateRequest()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n=== Test 4: Check The Validate of Request ===");
        Console.ResetColor();
        var validRequest = new QuranRequest
        {
            SurahNumber = 1,
            HizbNumber = 1,
            PageNumber = 1,
            Translation = "english"
        };

        var invalidRequest = new QuranRequest
        {
            SurahNumber = 200,  // رقم غير صحيح
            HizbNumber = 1,
            PageNumber = 1,
            Translation = "english"
        };

        try
        {
            // اختبار طلب صحيح
            var validResult = await PostJsonAsync<bool>($"{_baseUrl}/validate", validRequest);
            Console.WriteLine($"The request is right: {validResult}");

            // اختبار طلب غير صحيح
            var invalidResult = await PostJsonAsync<bool>($"{_baseUrl}/validate", invalidRequest);
            Console.WriteLine($"The request is wrong: {invalidResult}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"something is wrong: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// مثال 5: اختبار Health Check
    /// </summary>
    public static async Task Example5_HealthCheck()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n=== Test 5: the state of service ===");
        Console.ResetColor();
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"حالة الخدمة: {content}");
            }
            else
            {
                Console.WriteLine($"الخدمة غير متاحة: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"something is wrong in connection: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// تشغيل جميع الأمثلة
    /// </summary>
    public static async Task RunAllExamples()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Quran api service is started");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(new string('=', 50));
        Console.ResetColor();

        await Example1_GetFatihaLines();
        await Example2_GetUrduTranslation();
        await Example3_GetAvailableTranslations();
        await Example4_ValidateRequest();
        await Example5_HealthCheck();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nThe All tests Are Done");
        Console.ResetColor();
    }

    // Helper method for POST requests
    private static async Task<T?> PostJsonAsync<T>(string url, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        throw new HttpRequestException($"Request failed with status {response.StatusCode}");
    }
}