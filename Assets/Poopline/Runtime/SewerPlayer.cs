using UnityEngine;
namespace Poopline
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SewerPlayer : MonoBehaviour
    {
        public float walkSpeed = 3.2f, wadeSpeed = 2.3f;
        public bool InWater { get; private set; }
        public bool Climbing => ladder != null;
        public SewerLadder ActiveLadder => ladder;
        public SewerLadder NearbyLadder { get; private set; }
        public Vector3 Velocity { get; private set; }
        private PooplineGame game;
        private CharacterController controller;
        private Transform appearance, leftLeg, rightLeg, leftArm, rightArm;
        private SewerLadder ladder;
        private bool waitForNeutral;
        private float verticalSpeed, climbProgress, stepPhase, ladderCooldown;
        public void Initialize(PooplineGame owner)
        {
            game = owner; controller = GetComponent<CharacterController>();
            appearance = transform.Find("Human");
            leftLeg = appearance.Find("Left leg"); rightLeg = appearance.Find("Right leg");
            leftArm = appearance.Find("Left arm"); rightArm = appearance.Find("Right arm");
        }
        public void ResetAt(Vector3 position)
        {
            controller.enabled = false; transform.position = position; controller.enabled = true;
            ladder = null; NearbyLadder = null; InWater = false; waitForNeutral = false; verticalSpeed = 0; Velocity = Vector3.zero;
            ladderCooldown = 0.3f; appearance.rotation = Quaternion.Euler(0, 90, 0);
        }
        public void StopMotion() { Velocity = Vector3.zero; verticalSpeed = 0; }
        private void Update()
        {
            if (game == null || game.Frozen) return;
            float dt = Time.deltaTime;
            ladderCooldown = Mathf.Max(0, ladderCooldown - dt);
            Vector2 input = game.Input.Move;
            // Releasing the climbing direction prevents an accidental walk off the landing.
            if (waitForNeutral)
            {
                if (input.sqrMagnitude < .01f) waitForNeutral = false;
                input = Vector2.zero;
            }
            Vector3 right = game.GameCamera.transform.right; right.y = 0; right.Normalize();
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            Vector3 desired = right * input.x + forward * input.y;
            NearbyLadder = null;
            if (!Climbing)
                foreach (var candidate in game.Ladders)
                    if (candidate.CanEnter(transform.position)) { NearbyLadder = candidate; break; }
            if (NearbyLadder != null && ladderCooldown <= 0 && NearbyLadder.WantsToEnter(transform.position, desired))
                BeginClimb(NearbyLadder);
            Vector3 before = transform.position;
            if (Climbing) UpdateClimb(ladder.ClimbInput(desired), dt);
            else
            {
                Vector3 current = Vector3.zero; InWater = false;
                foreach (var flow in game.Flows)
                    if (flow.ContainsFeet(transform.position)) { current = flow.Current; InWater = true; break; }
                Vector3 motion = desired * (InWater ? wadeSpeed : walkSpeed) + current;
                verticalSpeed = controller.isGrounded && verticalSpeed < 0 ? -2 : verticalSpeed - 22 * dt;
                motion.y = verticalSpeed;
                Vector3 step = KeepOnDeck(motion * dt);
                controller.Move(step);
                if (desired.sqrMagnitude > 0.01f)
                    appearance.rotation = Quaternion.RotateTowards(appearance.rotation, Quaternion.LookRotation(desired), 650 * dt);
            }
            Velocity = dt > 0 ? (transform.position - before) / dt : Vector3.zero;
            Animate(dt);
        }
        // Rails block visible edges; this also protects the small openings around ladders.
        // Climbing uses its own movement so only an intentional descent can leave a deck.
        private Vector3 KeepOnDeck(Vector3 step)
        {
            Vector3 feet = transform.position;
            if (!Supported(feet, -.05f)) return step;
            if (Supported(feet + new Vector3(step.x, 0, step.z))) return step;
            if (!Supported(feet + new Vector3(step.x, 0, 0))) step.x = 0;
            if (!Supported(feet + new Vector3(step.x, 0, step.z))) step.z = 0;
            return step;
        }
        private bool Supported(Vector3 feet, float inset = .14f)
        {
            foreach (var deck in game.Decks) if (deck.Supports(feet, inset)) return true;
            return false;
        }
        private void BeginClimb(SewerLadder target)
        {
            ladder = target; climbProgress = Mathf.Abs(transform.position.y - target.Top.y) < 0.65f ? 1 : 0;
            InWater = false; verticalSpeed = 0; appearance.rotation = Quaternion.LookRotation(target.FaceDirection);
            // Move clear of the deck lip at landing height before starting the descent.
            // This lets a held approach direction catch the side ladder without a free fall.
            if (climbProgress == 1)
                controller.Move(new Vector3(target.Top.x, Mathf.Max(transform.position.y, target.TopLanding.y), target.Top.z) - transform.position);
        }
        private void UpdateClimb(float input, float dt)
        {
            if (Mathf.Abs(input) < .01f) return;
            float length = Vector3.Distance(ladder.Bottom, ladder.Top);
            climbProgress = Mathf.Clamp01(climbProgress + input * 1.65f * dt / length);
            Vector3 desired = Vector3.Lerp(ladder.Bottom, ladder.Top, climbProgress);
            controller.Move(desired - transform.position);
            if ((climbProgress >= 1 && input > 0) || (climbProgress <= 0 && input < 0))
            {
                Vector3 landing = climbProgress >= 1 ? ladder.TopLanding : ladder.BottomLanding;
                controller.Move(landing - transform.position);
                ladder = null; ladderCooldown = 0.45f; waitForNeutral = true;
            }
        }
        private void Animate(float dt)
        {
            float speed = Velocity.magnitude; stepPhase += speed * dt * 4.2f;
            float angle = speed > 0.15f ? Mathf.Sin(stepPhase) * (Climbing ? 28 : 22) : 0;
            leftLeg.localRotation = Quaternion.Euler(angle, 0, 0); rightLeg.localRotation = Quaternion.Euler(-angle, 0, 0);
            leftArm.localRotation = Quaternion.Euler(-angle * 0.7f - 12, 0, -8); rightArm.localRotation = Quaternion.Euler(angle * 0.7f, 0, 8);
        }
    }
}
