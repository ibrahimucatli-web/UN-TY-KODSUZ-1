using System;
using TacticalFPS.Core;
namespace TacticalFPS.Presentation
{
    public readonly struct KillFeedEvent { public readonly ulong Killer, Victim; public readonly string Weapon; public KillFeedEvent(ulong killer, ulong victim, string weapon) { Killer=killer; Victim=victim; Weapon=weapon; } }
    public interface IMatchEventSink { void OnHitConfirmed(float damage, bool headshot); void OnKill(KillFeedEvent kill); void OnRoundPhaseChanged(Match.RoundPhase phase); }
    public interface ISpatialAudio { void PlayAt(string soundId, UnityEngine.Vector3 position, SurfaceMaterial material); }
}
