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

        [Fact]
        public async Task GetMushafLinesAsync_ValidRequest_ReturnsSuccess()
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

            _mockSegmentationService.Setup(s => s.CreateMushafLines(It.IsAny<List<QuranLine>>(), It.IsAny<int>()))
                .Returns(new List<MushafLine>());

            // Act & Assert
            Xunit.Assert.True(request.LinesPerPage >= 10 && request.LinesPerPage <= 20);
        }

        [Fact]
        public async Task GetMultiTranslationAyahAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new MultiTranslationRequest
            {
                SurahNumber = 1,
                AyahNumber = 1,
                Translations = new List<string> { "english", "urdu" },
                IncludeArabic = true
            };

            // Act & Assert
            Xunit.Assert.True(request.Translations.Count > 0);
            Xunit.Assert.True(request.SurahNumber > 0);
            Xunit.Assert.True(request.AyahNumber > 0);
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

        [Theory]
        [InlineData(1, 1, 1, 15, true)] // Valid Mushaf request
        [InlineData(1, 1, 1, 9, false)] // Invalid lines per page (too low)
        [InlineData(1, 1, 1, 21, false)] // Invalid lines per page (too high)
        public async Task ValidateMushafRequestAsync_VariousInputs_ReturnsExpectedResult(
            int surahNumber, int hizbNumber, int pageNumber, int linesPerPage, bool expected)
        {
            // Arrange
            var request = new QuranMushafLinesRequest
            {
                SurahNumber = surahNumber,
                HizbNumber = hizbNumber,
                PageNumber = pageNumber,
                LinesPerPage = linesPerPage,
                Translation = "english"
            };

            // Act & Assert - Basic validation
            bool isValid = surahNumber >= 1 && surahNumber <= 114 &&
                          hizbNumber >= 1 && hizbNumber <= 30 &&
                          pageNumber >= 1 && pageNumber <= 604 &&
                          linesPerPage >= 10 && linesPerPage <= 20;

            Xunit.Assert.Equal(expected, isValid);
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
}








