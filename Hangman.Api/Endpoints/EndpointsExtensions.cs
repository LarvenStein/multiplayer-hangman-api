namespace Hangman.Api.Endpoints
{
    public static class EndpointsExtensions
    {
        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapCreateGame();
            app.MapJoinGame();
            app.MapEditGame();
            app.MapStartGame();
            // POST /games - neues spiel
            // POST /games/:id/players - spiel beitreten
            // PUT /games/:id - change room settings
            return app;
        }
    }
}
