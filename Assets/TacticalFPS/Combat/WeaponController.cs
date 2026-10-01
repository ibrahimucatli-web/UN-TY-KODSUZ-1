using UnityEngine;
using TacticalFPS.Movement;

namespace TacticalFPS.Combat
{
    /// <summary>Local presentation and server request production. Damage must never be decided here.</summary>
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition definition;
        [SerializeField] private CompetitiveMotor motor;
        public int ShotsFired { get; private set; }
        public float CurrentInaccuracy(bool onLadder)
        {
            float speedRatio = motor == null ? 0f : new Vector3(motor.Velocity.x, 0, motor.Velocity.z).magnitude / 6.2f;
            float inaccuracy = definition.baseSpreadDegrees + speedRatio * definition.movementSpreadDegrees;
            if (motor != null && !motor.IsGrounded) inaccuracy += definition.airSpreadDegrees;
            if (onLadder) inaccuracy += definition.ladderSpreadDegrees;
            return inaccuracy + (ShotsFired == 0 ? definition.firstShotInaccuracyDegrees : 0f);
        }
        public Vector3 ApplySpreadAndRecoil(Vector3 aimDirection, uint seed)
        {
            var random = new System.Random((int)seed); float angle = (float)random.NextDouble() * Mathf.PI * 2f; float radius = Mathf.Sqrt((float)random.NextDouble()) * CurrentInaccuracy(false);
            var recoil = definition.GetRecoil(ShotsFired++); Quaternion rotation = Quaternion.Euler(-recoil.pitch + Mathf.Sin(angle) * radius, recoil.yaw + Mathf.Cos(angle) * radius, 0); return rotation * aimDirection;
        }
        public void ResetSpray() => ShotsFired = 0;
    }
}
