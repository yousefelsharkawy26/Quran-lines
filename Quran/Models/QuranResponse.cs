// Models/QuranResponse.cs
namespace Quran.Models;

public class QuranResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<QuranLine> Lines { get; set; } = new();
    public QuranRequest? RequestInfo { get; set; }
}


