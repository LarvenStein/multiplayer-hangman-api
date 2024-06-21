namespace Hangman.Api.Endpoints
{
    public static class EndpointsExtensions
    {
        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapCreateGame();
            app.MapJoinGame();
            // POST /games - neues spiel
            // POST /games/:id/players - spiel beitreten
            return app;
        }
    }
}
