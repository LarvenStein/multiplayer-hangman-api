using FluentValidation;
using Hangman.Contracts.Responses;

namespace Hangman.Api.Mapping
{
    public class ValidationMappingMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMappingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = 400;
                var validationFailureResponse = new ValidationFailureResponse
                {
                    Errors = ex.Errors.Select(x => new ValidationResponse
                    {
                        PropertyName = x.PropertyName,
                        Message = x.ErrorMessage,
                    })
                };

                await context.Response.WriteAsJsonAsync(validationFailureResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                string[] exContents = ex.Message.Split(";");
                int exContentsMessageIndex = 1;
                if(Int32.TryParse(exContents[0], out int statusCode))
                {
                    context.Response.StatusCode = statusCode;
                } else
                {
                    context.Response.StatusCode = 500;
                    exContentsMessageIndex = 0;

                }

                var failureResponse = new OtherFailureResponse
                {
                    message = exContents[exContentsMessageIndex]
                };
                await context.Response.WriteAsJsonAsync(failureResponse);
            }
        }


    }
}
