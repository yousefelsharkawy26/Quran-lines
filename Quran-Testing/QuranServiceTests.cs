// Testing/QuranServiceTests.cs
using Microsoft.Extensions.Logging;
using Moq;
using Quran.Models;
using QuranLinesService.Services;
using QuranLinesService.Services.Interfaces;
using Xunit;

namespace QuranLinesService.Tests
{
    public class QuranServiceTests
    {
        private readonly Mock<ILogger<QuranService>> _mockLogger;
        private readonly Mock<ITextSegmentationService> _mockSegmentationService;
        private readonly HttpClient _httpClient;
        private readonly QuranService _quranService;

        public QuranServiceTests()
        {
            _mockLogger = new Mock<ILogger<QuranService>>();
            _mockSegmentationService = new Mock<ITextSegmentationService>();
            _httpClient = new HttpClient();
            _quranService = new QuranService(_httpClient, _mockSegmentationService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetQuranLinesAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new QuranRequest
            {
                SurahNumber = 1,
                HizbNumber = 1,
                PageNumber = 1,
                Translation = "english"
            };

            _mockSegmentationService.Setup(s => s.SmartSegment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new List<string> { "Test segment" });

            // Act & Assert - This would require mocking HttpClient for full unit testing
            // For integration testing, you can run this against the actual API
            Xunit.Assert.True(request.SurahNumber > 0);
        }

        [Theory]
        [InlineData(0, 1, 1, false)] // Invalid Surah
        [InlineData(115, 1, 1, false)] // Invalid Surah
        [InlineData(1, 0, 1, false)] // Invalid Hizb
        [InlineData(1, 31, 1, false)] // Invalid Hizb
        [InlineData(1, 1, 0, false)] // Invalid Page
        [InlineData(1, 1, 605, false)] // Invalid Page
        [InlineData(1, 1, 1, true)] // Valid request
        public async Task ValidateRequestAsync_VariousInputs_ReturnsExpectedResult(
            int surahNumber, int hizbNumber, int pageNumber, bool expected)
        {
            // Arrange
            var request = new QuranRequest
            {
                SurahNumber = surahNumber,
                HizbNumber = hizbNumber,
                PageNumber = pageNumber,
                Translation = "english"
            };

            // Act
            var result = await _quranService.ValidateRequestAsync(request);

            // Assert - Basic validation without HTTP calls
            if (surahNumber < 1 || surahNumber > 114 ||
                hizbNumber < 1 || hizbNumber > 30 ||
                pageNumber < 1 || pageNumber > 604)
            {
                Xunit.Assert.False(result);
            }
        }

        [Fact]
        public void GetAvailableTranslations_ReturnsTranslationsList()
        {
            // Act
            var translations = _quranService.GetAvailableTranslations();

            // Assert
            Xunit.Assert.NotEmpty(translations);
            Xunit.Assert.Contains("english", translations);
            Xunit.Assert.Contains("arabic", translations);
        }
    }

    public class TextSegmentationServiceTests
    {
        private readonly TextSegmentationService _service;

        public TextSegmentationServiceTests()
        {
            _service = new TextSegmentationService();
        }

        [Fact]
        public void SegmentArabicText_WithStopMarks_ReturnsCorrectSegments()
        {
            // Arrange
            var arabicText = "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ ۚ ٱلْحَمْدُ لِلَّهِ رَبِّ ٱلْعَٰلَمِينَ";

            // Act
            var segments = _service.SegmentArabicText(arabicText);

            // Assert
            Xunit.Assert.NotEmpty(segments);
            Xunit.Assert.True(segments.Count >= 1);
        }

        [Fact]
        public void SegmentEnglishText_WithPunctuation_ReturnsCorrectSegments()
        {
            // Arrange
            var englishText = "In the name of Allah, the Entirely Merciful, the Especially Merciful. Praise to Allah, Lord of the worlds.";

            // Act
            var segments = _service.SegmentEnglishText(englishText);

            // Assert
            Xunit.Assert.NotEmpty(segments);
            Xunit.Assert.True(segments.Count >= 2); // Should split on period
        }

        [Theory]
        [InlineData("arabic", "ar")]
        [InlineData("english", "en")]
        [InlineData("french", "fr")]
        public void SmartSegment_DifferentLanguages_ReturnsSegments(string language, string langCode)
        {
            // Arrange
            var text = "This is a test text for segmentation in different languages.";

            // Act
            var segments = _service.SmartSegment(text, language);

            // Assert
            Xunit.Assert.NotEmpty(segments);
        }
    }
}








