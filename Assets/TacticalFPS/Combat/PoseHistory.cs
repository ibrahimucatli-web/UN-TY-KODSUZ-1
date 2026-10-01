using System.Collections.Generic;
using UnityEngine;

namespace TacticalFPS.Combat
{
    public readonly struct PoseSample { public readonly double Time; public readonly Vector3 Position; public readonly Quaternion Rotation; public PoseSample(double time, Vector3 p, Quaternion r) { Time=time; Position=p; Rotation=r; } }
    /// <summary>Per-player bounded server history. The network adapter temporarily rewinds colliders to a sampled pose then restores them.</summary>
    public sealed class PoseHistory
    {
        private readonly Queue<PoseSample> samples = new(); private readonly double retention;
        public PoseHistory(double retentionSeconds = 1.0) => retention = retentionSeconds;
        public void Record(double time, Transform source) { samples.Enqueue(new PoseSample(time, source.position, source.rotation)); while (samples.Count > 0 && samples.Peek().Time < time - retention) samples.Dequeue(); }
        public bool TrySample(double time, out PoseSample result)
        {
            result = default; bool found = false;
            foreach (var sample in samples) { if (sample.Time > time) break; result = sample; found = true; }
            return found;
        }
    }
}
