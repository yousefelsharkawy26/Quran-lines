// Models/MultiTranslationRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Quran.Models;

public class MultiTranslationRequest
{
    [Required]
    [Range(1, 114, ErrorMessage = "رقم السورة يجب أن يكون بين 1 و 114")]
    public int SurahNumber { get; set; }

    [Required]
    [Range(1, 286, ErrorMessage = "رقم الآية غير صحيح")]
    public int AyahNumber { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "يجب تحديد ترجمة واحدة على الأقل")]
    public List<string> Translations { get; set; } = new() { "english" };

    public bool IncludeArabic { get; set; } = true;
}


