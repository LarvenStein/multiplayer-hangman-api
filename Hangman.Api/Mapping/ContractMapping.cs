using Hangman.Application.Models;
using Hangman.Contracts.Requests;
using Hangman.Contracts.Responses;
using System.Runtime.CompilerServices;

namespace Hangman.Api.Mapping
{
    public static class ContractMapping
    {
        public static Player MapToPlayer(this JoinGameRequest request,  String roomCode)
        {
            return new Player
            {
                Id = Guid.NewGuid(),
                Nickname = request.nickname,
                roomCode = roomCode,
            };
        }

        public static JoinGameResponse MapToResponse(this Player player)
        {
            return new JoinGameResponse
            {
                Id = player.Id,
                nickname = player.Nickname,
                roomCode = player.roomCode,
            };
        }
    }
}
