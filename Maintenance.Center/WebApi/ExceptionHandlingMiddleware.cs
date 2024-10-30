using Application;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebApi
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";

            switch (exception)
            {
                case ValidationException ve:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                    await httpContext.Response.WriteAsync(JsonSerializer.Serialize(
                        new ProblemDetails()
                        {
                            Type = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/errors/application-validation-error",
                            Title = "The application threw a validation error",
                            Detail = ve.Message,
                            Instance = httpContext.Request.Path,
                            Status = httpContext.Response.StatusCode
                        }));
                    break;
                case ApplicationLogicException ale:

                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                    await httpContext.Response.WriteAsync(JsonSerializer.Serialize(
                        new ProblemDetails()
                        {
                            Type = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/errors/application-logic-error",
                            Title = "The application threw a logic error",
                            Detail = ale.Message,
                            Instance = httpContext.Request.Path,
                            Status = httpContext.Response.StatusCode
                        }));
                    break;
                default:
                    if (exception is not TaskCanceledException)
                    {
                        _logger.LogError(exception, "Fatal Error Occurred");
                    }

                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    await httpContext.Response.WriteAsync(JsonSerializer.Serialize(
                        new ProblemDetails()
                        {
                            Type = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/errors/internal-server-error",
                            Title = "An internal server error occurred.",
                            Detail = "Something happened in the server and we couldn't process your request.",
                            Instance = httpContext.Request.Path,
                            Status = httpContext.Response.StatusCode
                        }));
                    break;
            }
        }
    }
}
