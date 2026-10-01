using System;
using System.Collections.Generic;
using TacticalFPS.Core;

namespace TacticalFPS.Match
{
    public enum RoundPhase : byte { Warmup, Freeze, Live, BombPlanted, RoundEnd, MatchEnd }
    public enum RoundEndReason : byte { Elimination, Time, Defused, Exploded }
    public sealed class TeamEconomy
    {
        private static readonly int[] LossBonus = { 1400, 1900, 2400, 2900, 3400 };
        public int Money { get; private set; } = 800; public int Losses { get; private set; }
        public bool TryBuy(int cost) { if (cost < 0 || cost > Money) return false; Money -= cost; return true; }
        public void AwardWin() { Money += 3250; Losses = 0; }
        public void AwardLoss() { Money += LossBonus[Math.Min(Losses, LossBonus.Length - 1)]; Losses++; }
        public void Award(int value) => Money += Math.Max(0, value);
    }
    /// <summary>MR12: first to 13, halves after 12 regulation rounds; overtime is 3+3 blocks until a winner.</summary>
    public sealed class CompetitiveMatch
    {
        public RoundPhase Phase { get; private set; } = RoundPhase.Warmup;
        public int TerroristScore { get; private set; } public int CounterTerroristScore { get; private set; } public int PlayedRounds { get; private set; }
        public TeamEconomy Terrorists { get; } = new(); public TeamEconomy CounterTerrorists { get; } = new();
        public event Action<RoundPhase> PhaseChanged; public event Action<Team, RoundEndReason> RoundFinished;
        public void StartFreeze() => SetPhase(RoundPhase.Freeze);
        public void StartLive() { if (Phase != RoundPhase.Freeze) throw new InvalidOperationException("Round must start from freeze time."); SetPhase(RoundPhase.Live); }
        public void PlantBomb() { if (Phase != RoundPhase.Live) return; SetPhase(RoundPhase.BombPlanted); }
        public void EndRound(Team winner, RoundEndReason reason, bool bombPlantReward = false)
        {
            if (Phase != RoundPhase.Live && Phase != RoundPhase.BombPlanted) return;
            if (winner == Team.Terrorists) { TerroristScore++; Terrorists.AwardWin(); CounterTerrorists.AwardLoss(); if (bombPlantReward) Terrorists.Award(800); }
            else { CounterTerroristScore++; CounterTerrorists.AwardWin(); Terrorists.AwardLoss(); if (reason == RoundEndReason.Defused) CounterTerrorists.Award(300); }
            PlayedRounds++; RoundFinished?.Invoke(winner, reason);
            if (HasWinner()) SetPhase(RoundPhase.MatchEnd); else SetPhase(RoundPhase.RoundEnd);
        }
        private bool HasWinner() => TerroristScore >= 13 || CounterTerroristScore >= 13;
        private void SetPhase(RoundPhase phase) { Phase = phase; PhaseChanged?.Invoke(phase); }
    }
}
