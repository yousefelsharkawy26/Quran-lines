// Models/QuranModels.cs
using Microsoft.AspNetCore.Mvc;
using Quran.Models;
using QuranLinesService.Services;
using QuranLinesService.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Quran.Models;

public class QuranRequest
{
    [Required]
    [Range(1, 114, ErrorMessage = "رقم السورة يجب أن يكون بين 1 و 114")]
    public int SurahNumber { get; set; }

    [Required]
    [Range(1, 30, ErrorMessage = "رقم الحزب يجب أن يكون بين 1 و 30")]
    public int HizbNumber { get; set; }

    [Required]
    [Range(1, 604, ErrorMessage = "رقم الصفحة يجب أن يكون بين 1 و 604")]
    public int PageNumber { get; set; }

    public string? Translation { get; set; } = "english"; // Default translation
}

public class QuranLine
{
    public string ArabicText { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public int SurahNumber { get; set; }
    public string SurahName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int HizbNumber { get; set; }
    public List<string> ArabicSegments { get; set; } = new();
    public List<string> TranslationSegments { get; set; } = new();
}

public class QuranResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<QuranLine> Lines { get; set; } = new();
    public QuranRequest? RequestInfo { get; set; }
}

public class ApiError
{
    public string Error { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}



