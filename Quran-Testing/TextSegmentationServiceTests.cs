// Testing/TextSegmentationServiceTests.cs
using Quran.Models;
using QuranLinesService.Services;
using Xunit;

namespace QuranLinesService.Tests;

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
    public void CombineSegmentsToMushafLine_WithMultipleSegments_ReturnsProperLines()
    {
        // Arrange
        var segments = new List<string>
        {
            "بِسْمِ ٱللَّهِ",
            "ٱلرَّحْمَٰنِ ٱلرَّحِيمِ",
            "ٱلْحَمْدُ لِلَّهِ",
            "رَبِّ ٱلْعَٰلَمِينَ"
        };

        // Act
        var mushafLines = _service.CombineSegmentsToMushafLine(segments, 40);

        // Assert
        Xunit.Assert.NotEmpty(mushafLines);
        Xunit.Assert.All(mushafLines, line => Xunit.Assert.True(line.Length <= 50)); // Some buffer for Arabic text
    }

    [Fact]
    public void CreateMushafLines_ValidQuranLines_ReturnsStructuredMushafLines()
    {
        // Arrange
        var quranLines = new List<QuranLine>
        {
            new QuranLine
            {
                ArabicText = "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ",
                Translation = "In the name of Allah, the Entirely Merciful, the Especially Merciful.",
                LineNumber = 1,
                SurahNumber = 1,
                SurahName = "Al-Fatiha",
                PageNumber = 1,
                HizbNumber = 1
            }
        };

        // Act
        var mushafLines = _service.CreateMushafLines(quranLines, 15);

        // Assert
        Xunit.Assert.NotEmpty(mushafLines);
        Xunit.Assert.All(mushafLines, line =>
        {
            Xunit.Assert.Equal(1, line.SurahNumber);
            Xunit.Assert.Equal(1, line.PageNumber);
            Xunit.Assert.NotEmpty(line.AyahNumbers);
        });
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






