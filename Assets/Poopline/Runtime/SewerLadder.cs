using UnityEngine;
namespace Poopline
{
    public sealed class SewerLadder : MonoBehaviour
    {
        public float height = 2.6f;
        public Vector3 topExit = new Vector3(0, 0.08f, 0.85f), bottomExit = new Vector3(0, 0.05f, -0.65f);
        public Vector3 Bottom => transform.position;
        public Vector3 Top => transform.position + Vector3.up * height;
        public Vector3 TopLanding => Top + transform.TransformDirection(topExit);
        public Vector3 BottomLanding => Bottom + transform.TransformDirection(bottomExit);
        public Vector3 FaceDirection => transform.forward;
        public int EntryDirection(Vector3 point)
        {
            if (Near(point, Bottom, 1.05f)) return 1;
            if (Near(point, TopLanding, .65f)) return -1;
            return 0;
        }
        public bool CanEnter(Vector3 point) { return EntryDirection(point) != 0; }
        public float ClimbInput(Vector3 movement) { return Vector3.Dot(movement, FaceDirection); }
        public bool WantsToEnter(Vector3 point, Vector3 movement)
        {
            float along = ClimbInput(movement) * EntryDirection(point);
            float across = Mathf.Abs(Vector3.Dot(movement, transform.right));
            // Walking diagonally away from a landing must not snap back onto the ladder.
            return along > .5f && along > across + .15f;
        }
        public string DirectionHint(Camera camera, bool ascending)
        {
            Vector3 right = camera.transform.right; right.y = 0; right.Normalize();
            Vector3 direction = FaceDirection * (ascending ? 1 : -1);
            float x = Vector3.Dot(direction, right), y = Vector3.Dot(direction, Vector3.Cross(right, Vector3.up));
            if (Mathf.Abs(x) > Mathf.Abs(y)) return x > 0 ? "D / →" : "A / ←";
            return y > 0 ? "W / ↑" : "S / ↓";
        }
        private static bool Near(Vector3 a, Vector3 b, float radius)
        { return Mathf.Abs(a.y - b.y) < 0.6f && new Vector2(a.x - b.x, a.z - b.z).magnitude < radius; }
    }
}
