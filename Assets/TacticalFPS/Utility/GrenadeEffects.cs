using System.Collections.Generic;
using UnityEngine;
using TacticalFPS.Combat;

namespace TacticalFPS.Utility
{
    public interface ILineOfSight { bool IsVisible(Vector3 from, Vector3 to); }
    public sealed class FlashbangEffect
    {
        public float EvaluateSeconds(Vector3 detonation, Vector3 viewerPosition, Vector3 viewerForward, ILineOfSight sight)
        {
            Vector3 toFlash = detonation - viewerPosition; float distance = toFlash.magnitude;
            if (distance > 30f || !sight.IsVisible(viewerPosition, detonation)) return 0f;
            float facing = Mathf.Clamp01(Vector3.Dot(viewerForward.normalized, toFlash.normalized));
            return Mathf.Lerp(0f, 4.8f, facing) * Mathf.Clamp01(1f - distance / 30f);
        }
    }
    public sealed class SmokeVolume : MonoBehaviour
    {
        [SerializeField] float lifetime = 18f, maxRadius = 4.5f, expansionSeconds = .7f;
        private readonly List<(Vector3 center, float radius, float until)> holes = new(); private float spawnedAt;
        public float Radius => Mathf.SmoothStep(0f, maxRadius, Mathf.Clamp01((Time.time - spawnedAt) / expansionSeconds));
        private void Awake() => spawnedAt = Time.time;
        public void CarveHole(Vector3 position, float radius, float duration) => holes.Add((position, radius, Time.time + duration));
        public bool Obscures(Vector3 position) { holes.RemoveAll(h => h.until < Time.time); foreach (var h in holes) if (Vector3.Distance(position, h.center) <= h.radius) return false; return Vector3.Distance(position, transform.position) <= Radius; }
        private void Update() { if (Time.time - spawnedAt >= lifetime) Destroy(gameObject); }
    }
    public sealed class ExplosionDamage
    {
        public void Apply(Vector3 center, float radius, float maxDamage, IEnumerable<IDamageable> targets, System.Func<IDamageable, Vector3> position)
        { foreach (var target in targets) { float t = Vector3.Distance(center, position(target)) / radius; if (t < 1f) target.ApplyDamage(maxDamage * (1f - t), 0); } }
    }
}
