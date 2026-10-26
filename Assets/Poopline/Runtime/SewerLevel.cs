using UnityEngine;

namespace Poopline
{
    // Both stages are authored scene objects. Only the current stage is active in Play.
    public sealed class SewerLevel : MonoBehaviour
    {
        public string title;
        [TextArea] public string introduction;
        public Vector3 start;
        public Bounds exit;
        public Bounds playableBounds;
        public Vector3[] checkpoints;
        public string[] objectives;
        public Vector3[] suggestedRoute;

        public bool ContainsExit(Vector3 feet) => exit.Contains(feet);
        public bool ContainsPlayer(Vector3 feet) => playableBounds.Contains(feet);
    }
}
