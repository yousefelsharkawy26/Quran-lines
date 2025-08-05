// Services/Interfaces/IQuranService.cs
using Quran.Models;

namespace QuranLinesService.Services.Interfaces;

public interface IQuranService
{
    Task<QuranResponse> GetQuranLinesAsync(QuranRequest request);
    Task<MushafLinesResponse> GetMushafLinesAsync(QuranMushafLinesRequest request);
    Task<MultiTranslationResponse> GetMultiTranslationAyahAsync(MultiTranslationRequest request);
    Task<bool> ValidateRequestAsync(QuranRequest request);
    Task<bool> ValidateMushafRequestAsync(QuranMushafLinesRequest request);
    Task<bool> ValidateMultiTranslationRequestAsync(MultiTranslationRequest request);
    List<string> GetAvailableTranslations();
}
