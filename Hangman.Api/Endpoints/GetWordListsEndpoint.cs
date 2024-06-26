using Hangman.Application.Services;

namespace Hangman.Api.Endpoints
{
    public static class GetWordListsEndpoint
    {
        public const string name = "GetWordLists";
        public static IEndpointRouteBuilder MapGetWordlists(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.GetWordLists, async (
                IGameService gameService,
                CancellationToken token) =>
            {
                var data = await gameService.GetWordlists();
                return TypedResults.Ok(data);
            }).CacheOutput();
            return app;
        }
    }
}
