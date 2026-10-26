using UnityEngine;
namespace Poopline
{
    public sealed class SewerCamera : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0, 12, -15);
        private PooplineGame game;
        private Vector3 velocity;
        private Camera view;
        public void Initialize(PooplineGame owner) { game = owner; view = GetComponent<Camera>(); }
        private Vector3 Target => new Vector3(game.Player.transform.position.x + 1.2f,
            .6f + game.Player.transform.position.y * .65f, game.Player.transform.position.z + 1.1f);
        public void Snap() { velocity = Vector3.zero; transform.position = Target + offset; transform.rotation = Quaternion.LookRotation(-offset); }
        private void LateUpdate()
        {
            if (game == null || game.Paused) return;
            transform.position = Vector3.SmoothDamp(transform.position, Target + offset, ref velocity, 0.18f);
            transform.rotation = Quaternion.LookRotation(-offset);
            view.orthographicSize = Mathf.Max(5.8f, 9.2f / Mathf.Max(0.5f, view.aspect));
        }
    }
}
