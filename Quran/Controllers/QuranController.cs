﻿// Controllers/QuranController.cs
using Microsoft.AspNetCore.Mvc;
using Quran.Models;
using QuranLinesService.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace QuranLinesService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuranController : ControllerBase
{
    private readonly IQuranService _quranService;
    private readonly ILogger<QuranController> _logger;

    public QuranController(IQuranService quranService, ILogger<QuranController> logger)
    {
        _quranService = quranService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على سطور القرآن الكريم مع الترجمة والتقسيم الذكي
    /// </summary>
    /// <param name="request">معلومات الطلب (رقم السورة، الحزب، الصفحة، الترجمة)</param>
    /// <returns>سطور القرآن مقسمة مع الترجمة</returns>
    [HttpPost("lines")]
    [ProducesResponseType(typeof(QuranResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<ActionResult<QuranResponse>> GetQuranLines([FromBody] QuranRequest request)
    {
        try
        {
            _logger.LogInformation($"Received request for Surah: {request.SurahNumber}, Hizb: {request.HizbNumber}, Page: {request.PageNumber}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiError
                {
                    Error = "بيانات الطلب غير صحيحة",
                    Details = string.Join(", ", errors),
                    StatusCode = 400
                });
            }

            var result = await _quranService.GetQuranLinesAsync(request);

            if (!result.Success)
            {
                return BadRequest(new ApiError
                {
                    Error = result.Message,
                    Details = "تحقق من صحة المعايير المدخلة",
                    StatusCode = 400
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Quran lines request");
            return StatusCode(500, new ApiError
            {
                Error = "خطأ في الخادم",
                Details = "حدث خطأ أثناء معالجة الطلب",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// الحصول على قائمة الترجمات المتاحة
    /// </summary>
    /// <returns>قائمة بالترجمات المتاحة</returns>
    [HttpGet("translations")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public ActionResult<List<string>> GetAvailableTranslations()
    {
        try
        {
            var translations = _quranService.GetAvailableTranslations();
            return Ok(translations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available translations");
            return StatusCode(500, new ApiError
            {
                Error = "خطأ في الخادم",
                Details = "حدث خطأ أثناء جلب قائمة الترجمات",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// التحقق من صحة طلب القرآن
    /// </summary>
    /// <param name="request">معلومات الطلب للتحقق منها</param>
    /// <returns>نتيجة التحقق</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<ActionResult<bool>> ValidateRequest([FromBody] QuranRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiError
                {
                    Error = "بيانات الطلب غير صحيحة",
                    Details = "تحقق من صحة الأرقام المدخلة",
                    StatusCode = 400
                });
            }

            var isValid = await _quranService.ValidateRequestAsync(request);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating request");
            return StatusCode(500, new ApiError
            {
                Error = "خطأ في الخادم",
                Details = "حدث خطأ أثناء التحقق من الطلب",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>حالة الخدمة</returns>
    [HttpGet("health")]
    [ProducesResponseType(200)]
    public ActionResult HealthCheck()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow, Service = "Quran Lines Service" });
    }
}
