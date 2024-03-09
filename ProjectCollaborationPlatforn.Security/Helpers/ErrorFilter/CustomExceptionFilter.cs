using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;

namespace ProjectCollaborationPlatforn.Security.Helpers.ErrorFilter
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _exeptionLogger;

        public CustomExceptionFilter(ILogger<CustomExceptionFilter> exeptionLogger)
        {
            _exeptionLogger = exeptionLogger;
        }

        public void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                CustomApiException apiException => apiException.StatusCode,
                ArgumentNullException _ => StatusCodes.Status422UnprocessableEntity,
                ValidationException _ => StatusCodes.Status422UnprocessableEntity,
                AuthenticationException _ => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            var title = context.Exception switch
            {
                CustomApiException apiException => apiException.Title,
                ArgumentNullException _ => "Ignored or malformed argument",
                ValidationException _ => "Validation error",
                AuthenticationException _ => "Authentication issue",
                _ => "Internal error"
            };

            var message = context.Exception switch
            {
                CustomApiException apiException => apiException.Detail,
                ArgumentNullException _ => context.Exception.Message,
                ValidationException _ => context.Exception.Message,
                AuthenticationException _ => "Action is permitted because of authentication reasons",
                _ => "Internal error occured. Please try again later"
            };



            var problemDetails = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = message
            };

            if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
            {
                _exeptionLogger.LogError(context.Exception, "Critical error handled");
            }
            else if (problemDetails.Status >= StatusCodes.Status400BadRequest)
            {
                _exeptionLogger.LogError(context.Exception, "Request error handled");
            }

            var response = BuildResponse(problemDetails);

            context.HttpContext.Response.StatusCode = response.StatusCode ?? StatusCodes.Status500InternalServerError;
            context.Result = response;
            context.ExceptionHandled = true;
        }

        private static ObjectResult BuildResponse(ProblemDetails problem) =>
            new ObjectResult(problem)
            {
                StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError,
                ContentTypes = new MediaTypeCollection { "application/problem+json" }
            };
    }
}

