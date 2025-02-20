﻿using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Services
{
    public interface IGameStateService
    {
        Task<int> NextRoundAsync(String roomCode, Guid userId, bool start = false, CancellationToken token = default);
        Task<GameStatus> GetGameStatus(string roomCode, Guid userId, CancellationToken token = default);
        Task<Guess> HandleGuess(Guess guess, CancellationToken token = default);
        Task<RoundStatus> GetRoundStatus(RoundStatus status, CancellationToken token = default);
        Task<IEnumerable<RoundStatus>> GetRoundsStatus(String roomCode, Guid userId, CancellationToken token = default);
    }
}
