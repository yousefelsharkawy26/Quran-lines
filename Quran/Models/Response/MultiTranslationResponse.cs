// Models/QuranModels.cs

// Models/QuranModels.cs
using Quran.Models;

namespace Quran.Models;

public class MultiTranslationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MultiTranslationAyah? Ayah { get; set; }
    public MultiTranslationRequest? RequestInfo { get; set; }
    public List<string> AvailableTranslations { get; set; } = new();
    public List<string> FailedTranslations { get; set; } = new(); // الترجمات التي فشل تحميلها
}


