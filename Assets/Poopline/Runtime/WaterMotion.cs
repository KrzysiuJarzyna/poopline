using UnityEngine;
namespace Poopline
{
    public sealed class WaterMotion : MonoBehaviour
    {
        public float speed = 0.9f;
        public Vector3 direction = Vector3.right;
        private Vector3 origin;
        private float time;
        private PooplineGame game;
        private void Awake() { origin = transform.localPosition; game = GetComponentInParent<PooplineGame>(); }
        private void Update()
        {
            if(game==null || (game.InMainMenu?!game.BackdropAnimating:game.Frozen)) return;
            time += game.InMainMenu?Time.unscaledDeltaTime:Time.deltaTime;
            transform.localPosition = origin + direction * (Mathf.Repeat(time * speed, 2.4f) - 1.2f);
        }
    }
}
