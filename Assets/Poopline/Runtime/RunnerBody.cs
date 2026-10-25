using UnityEngine;

namespace Poopline
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class RunnerBody : MonoBehaviour
    {
        private PooplineGame game;
        private Rigidbody2D body;
        private float jumpBuffer;
        private float coyoteTime;
        private Vector2 pausedVelocity;
        private Transform appearance;
        public Vector2 Position => body.position;
        public bool Grounded { get; private set; }

        public void Initialize(PooplineGame owner)
        {
            game = owner;
            appearance = transform.Find("Appearance");
            body = GetComponent<Rigidbody2D>();
            gameObject.layer = 2; // Exclude the player from the ground probe (Ignore Raycast).
            body.gravityScale = 2.65f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            GetComponent<CircleCollider2D>().radius = 0.36f;
            ResetPosition();
        }

        public void ResetPosition()
        {
            body.simulated = false;
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(2, 0.4f);
            transform.position = body.position;
            pausedVelocity = Vector2.zero;
            jumpBuffer = coyoteTime = 0;
            Grounded = false;
            if (appearance != null) appearance.localScale = Vector3.one;
        }

        public void SetRunning(bool running)
        {
            if (!running) pausedVelocity = body.linearVelocity;
            body.simulated = running;
            body.linearVelocity = running ? pausedVelocity : Vector2.zero;
            jumpBuffer = 0;
        }

        public void QueueJump() { if (game.State.Phase == RunPhase.Playing) jumpBuffer = 0.14f; }

        private void FixedUpdate()
        {
            if (game == null || game.State.Phase != RunPhase.Playing) return;
            Grounded = body.linearVelocity.y <= 0.01f &&
                Physics2D.CircleCast(body.position, 0.31f, Vector2.down, 0.12f, 1 << 0).collider != null;
            coyoteTime = Grounded ? 0.12f : Mathf.Max(0, coyoteTime - Time.fixedDeltaTime);
            float vertical = body.linearVelocity.y;
            if (jumpBuffer > 0 && coyoteTime > 0)
            {
                vertical = game.JumpSpeed;
                jumpBuffer = coyoteTime = 0;
                Grounded = false;
                game.PlayJumpSound();
            }
            jumpBuffer = Mathf.Max(0, jumpBuffer - Time.fixedDeltaTime);
            float horizontal = game.Input.Horizontal;
            body.linearVelocity = new Vector2(horizontal * game.MoveSpeed, vertical);
            if (appearance != null && Mathf.Abs(horizontal) > 0.05f)
                appearance.localScale = new Vector3(Mathf.Sign(horizontal), 1, 1);
            if (body.position.y < -4) game.Finish(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (game.State.Phase != RunPhase.Playing) return;
            TrackItem item = other.GetComponent<TrackItem>();
            if (item != null) item.Touch(game);
        }
    }
}
