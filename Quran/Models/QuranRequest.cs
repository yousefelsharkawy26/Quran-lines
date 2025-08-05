// Models/QuranRequest.cs
using System.ComponentModel.DataAnnotations;

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


