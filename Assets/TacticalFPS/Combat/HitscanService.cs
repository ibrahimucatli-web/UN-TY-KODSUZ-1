using UnityEngine;
using TacticalFPS.Core;

namespace TacticalFPS.Combat
{
    /// <summary>Called only on the authoritative server after its rollback adapter validates the requested fire time.</summary>
    public sealed class HitscanService
    {
        private readonly IHitscanWorld world;
        public HitscanService(IHitscanWorld world) => this.world = world;
        public bool Fire(ShotRequest request, WeaponDefinition weapon, out HitResult result)
        {
            result = default; Vector3 origin = request.Origin, direction = request.Direction.normalized; float remainingDamage = weapon.damage, remainingDistance = weapon.range;
            for (int pass = 0; pass < 5 && remainingDamage > 1f && remainingDistance > .01f; pass++)
            {
                if (!world.Raycast(origin, direction, remainingDistance, out var hit)) return false;
                float traveled = Vector3.Distance(origin, hit.point); remainingDistance -= traveled;
                if (world.TryGetTarget(hit.collider, out var target) && target.IsAlive)
                {
                    bool headshot = hit.collider.CompareTag("Head"); float damage = remainingDamage * (headshot ? 4f : 1f);
                    target.ApplyDamage(damage, request.ShooterId); target.ApplyTag(Mathf.Clamp(damage / 200f, .2f, .5f), .35f);
                    result = new HitResult { TargetId = target.NetworkId, Point = hit.point, Normal = hit.normal, Damage = damage, IsHeadshot = headshot }; return true;
                }
                float resistance = world.GetMaterial(hit.collider) switch { SurfaceMaterial.Glass => .15f, SurfaceMaterial.Wood => .55f, SurfaceMaterial.Metal => 1.3f, SurfaceMaterial.Concrete => 2.2f, _ => 1f };
                remainingDamage -= resistance * 25f / Mathf.Max(.1f, weapon.penetrationPower); origin = hit.point + direction * .02f;
            }
            return false;
        }
    }
}
