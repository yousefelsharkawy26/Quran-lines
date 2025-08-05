// Services/Interfaces/IQuranService.cs
using Quran.Models;

namespace QuranLinesService.Services.Interfaces;

public interface IQuranService
{
    Task<QuranResponse> GetQuranLinesAsync(QuranRequest request);
    Task<bool> ValidateRequestAsync(QuranRequest request);
    List<string> GetAvailableTranslations();
}
