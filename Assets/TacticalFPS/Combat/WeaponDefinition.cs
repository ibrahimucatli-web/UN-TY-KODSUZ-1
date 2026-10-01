using System;
using UnityEngine;
using TacticalFPS.Core;

namespace TacticalFPS.Combat
{
    [Serializable] public struct RecoilPoint { public float yaw; public float pitch; }
    [CreateAssetMenu(menuName = "Tactical FPS/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string weaponId;
        public float damage = 35f, fireInterval = .1f, range = 8192f, baseSpreadDegrees = .2f, firstShotInaccuracyDegrees = .08f;
        public float movementSpreadDegrees = 1.4f, airSpreadDegrees = 2.5f, ladderSpreadDegrees = 3f, penetrationPower = 1.5f;
        public int magazineSize = 30, killAward = 300;
        public RecoilPoint[] recoilPattern;
        public RecoilPoint GetRecoil(int shot) => recoilPattern == null || recoilPattern.Length == 0 ? default : recoilPattern[Mathf.Min(shot, recoilPattern.Length - 1)];
    }

    public struct ShotRequest { public ulong ShooterId; public double ClientTimestamp; public Vector3 Origin, Direction; public uint Seed; }
    public struct HitResult { public ulong TargetId; public Vector3 Point, Normal; public float Damage; public bool IsHeadshot; }
    public interface IHitscanWorld
    {
        bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit);
        bool TryGetTarget(Collider collider, out IDamageable target);
        SurfaceMaterial GetMaterial(Collider collider);
    }
    public interface IDamageable { ulong NetworkId { get; } bool IsAlive { get; } void ApplyDamage(float damage, ulong attackerId); void ApplyTag(float slowdownFraction, float duration); }
}
