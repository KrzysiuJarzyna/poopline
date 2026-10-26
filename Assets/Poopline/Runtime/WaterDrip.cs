using UnityEngine;

namespace Poopline
{
    public sealed class WaterDrip : MonoBehaviour
    {
        public float fall = 2, speed = 2.8f, phase;
        private Vector3 origin;
        private float elapsed;
        private PooplineGame game;
        private void Awake() { origin = transform.localPosition; game = GetComponentInParent<PooplineGame>(); }
        private void Update()
        {
            if (game == null || game.Frozen) return;
            elapsed += Time.deltaTime;
            transform.localPosition = origin + Vector3.down * Mathf.Repeat(elapsed * speed + phase, fall);
        }
    }
}
