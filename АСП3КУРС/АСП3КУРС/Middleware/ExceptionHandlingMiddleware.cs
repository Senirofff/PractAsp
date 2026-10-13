using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using АСП3курс.Services.Exceptions;

namespace АСП3курс.Middleware;

/// <summary>
/// Преобразует исключения слоя сервисов в понятные HTTP-ответы (ProblemDetails),
/// чтобы нарушение бизнес-правила не превращалось в 500.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Ресурс не найден", ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Нарушено бизнес-правило", ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Некорректный запрос", ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Ошибка сохранения в базе данных");
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "Конфликт данных",
                "Не удалось сохранить изменения: нарушено ограничение целостности базы данных.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанное исключение");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Внутренняя ошибка сервера",
                "Произошла непредвиденная ошибка.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
