// Models/ApiError.cs
namespace Quran.Models;

public class ApiError
{
    public string Error { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}


