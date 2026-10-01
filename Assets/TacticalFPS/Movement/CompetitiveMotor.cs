using UnityEngine;
using TacticalFPS.Core;

namespace TacticalFPS.Movement
{
    [System.Serializable]
    public struct MovementSettings
    {
        public float MaxGroundSpeed, MaxAirSpeed, GroundAcceleration, AirAcceleration, Friction, StopSpeed, JumpSpeed, Gravity, CrouchSpeedMultiplier;
        public static MovementSettings Competitive => new MovementSettings { MaxGroundSpeed = 6.2f, MaxAirSpeed = 6.2f, GroundAcceleration = 55f, AirAcceleration = 10f, Friction = 6f, StopSpeed = 1.2f, JumpSpeed = 5.25f, Gravity = 18f, CrouchSpeedMultiplier = .52f };
    }

    /// <summary>Authoritative, fixed-step motor. Feed timestamps through an input buffer; never use frame delta for simulation.</summary>
    public sealed class CompetitiveMotor : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private MovementSettings settings = default;
        [SerializeField] private float bhopGraceSeconds = .055f;
        public Vector3 Velocity { get; private set; }
        public bool IsGrounded => controller != null && controller.isGrounded;
        public bool IsCrouching { get; private set; }
        public float TagMultiplier { get; private set; } = 1f;
        private double lastGroundedAt = double.NegativeInfinity;
        private InputCommand input;

        private void Reset() { controller = GetComponent<CharacterController>(); settings = MovementSettings.Competitive; }
        private void Awake() { if (settings.MaxGroundSpeed <= 0f) settings = MovementSettings.Competitive; }
        public void SetInput(InputCommand command) => input = command;
        public void ApplyTagSlow(float slowdownFraction, float duration) { TagMultiplier = Mathf.Clamp01(1f - slowdownFraction); CancelInvoke(nameof(ClearTagSlow)); Invoke(nameof(ClearTagSlow), duration); }
        private void ClearTagSlow() => TagMultiplier = 1f;

        public void Simulate(float fixedDeltaTime, double simulationTime, Transform view)
        {
            if (controller == null) return;
            bool grounded = controller.isGrounded;
            if (grounded) lastGroundedAt = simulationTime;
            IsCrouching = input.IsPressed(InputButtons.Crouch);
            Vector3 wish = Vector3.ProjectOnPlane(view.forward * input.Move.y + view.right * input.Move.x, Vector3.up);
            float speed = settings.MaxGroundSpeed * TagMultiplier * (IsCrouching ? settings.CrouchSpeedMultiplier : 1f);
            wish = Vector3.ClampMagnitude(wish, 1f) * speed;
            if (grounded) ApplyFriction(fixedDeltaTime);
            Accelerate(new Vector3(wish.x, 0, wish.z), grounded ? settings.GroundAcceleration : settings.AirAcceleration, grounded ? speed : settings.MaxAirSpeed * TagMultiplier, fixedDeltaTime);
            if (input.IsPressed(InputButtons.Jump) && (grounded || simulationTime - lastGroundedAt <= bhopGraceSeconds)) { Velocity = new Vector3(Velocity.x, settings.JumpSpeed, Velocity.z); lastGroundedAt = double.NegativeInfinity; }
            else Velocity = new Vector3(Velocity.x, grounded && Velocity.y < 0 ? -1f : Velocity.y - settings.Gravity * fixedDeltaTime, Velocity.z);
            controller.Move(Velocity * fixedDeltaTime);
        }
        private void ApplyFriction(float dt)
        {
            Vector3 lateral = new Vector3(Velocity.x, 0, Velocity.z); float magnitude = lateral.magnitude;
            if (magnitude < .001f) return;
            float drop = Mathf.Max(magnitude, settings.StopSpeed) * settings.Friction * dt;
            lateral *= Mathf.Max(0f, magnitude - drop) / magnitude; Velocity = new Vector3(lateral.x, Velocity.y, lateral.z);
        }
        private void Accelerate(Vector3 wishVelocity, float acceleration, float maxSpeed, float dt)
        {
            Vector3 lateral = new Vector3(Velocity.x, 0, Velocity.z); float current = Vector3.Dot(lateral, wishVelocity.normalized); float add = maxSpeed - current;
            if (add <= 0f || wishVelocity.sqrMagnitude < .0001f) return;
            float push = Mathf.Min(acceleration * maxSpeed * dt, add); lateral += wishVelocity.normalized * push; Velocity = new Vector3(lateral.x, Velocity.y, lateral.z);
        }
    }
}
