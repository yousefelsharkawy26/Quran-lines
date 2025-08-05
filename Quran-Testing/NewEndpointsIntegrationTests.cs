// Testing/NewEndpointsIntegrationTests.cs
using Quran.Models;
using System.Text;
using System.Text.Json;
using Xunit;

namespace QuranLinesService.Tests;

public class NewEndpointsIntegrationTests
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7234/api/quran";

    public NewEndpointsIntegrationTests()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }
    [Fact]
    public async Task MushafLinesEndpoint_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new QuranMushafLinesRequest
        {
            SurahNumber = 1,
            HizbNumber = 1,
            PageNumber = 1,
            Translation = "english",
            LinesPerPage = 15,
            CombineSegments = true
        };

        // Act
        var response = await PostJsonAsync<MushafLinesResponse>($"{_baseUrl}/mushaf-lines", request);

        // Assert
        Xunit.Assert.NotNull(response);
        // Additional assertions would require actual API to be running
    }

    public async Task MultiTranslationEndpoint_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new MultiTranslationRequest
        {
            SurahNumber = 1,
            AyahNumber = 1,
            Translations = new List<string> { "english", "urdu" },
            IncludeArabic = true
        };

        // Act
        var response = await PostJsonAsync<MultiTranslationResponse>($"{_baseUrl}/multi-translation", request);

        // Assert
        Xunit.Assert.NotNull(response);
        // Additional assertions would require actual API to be running
    }

    private async Task<T?> PostJsonAsync<T>(string url, object data)
    {
        try
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

            return default(T);
        }
        catch
        {
            return default(T);
        }
    }
}






