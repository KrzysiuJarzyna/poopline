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
        public void Snap() { if(game.InMainMenu) {MenuPose();return;} velocity = Vector3.zero; transform.position = Target + offset; transform.rotation = Quaternion.LookRotation(-offset); }
        private void MenuPose()
        {
            float sway=game.Settings.MenuMotion?Mathf.Sin(Time.unscaledTime*.16f)*.35f:0;
            transform.position=new Vector3(-39+sway,4.3f,-9);
            transform.rotation=Quaternion.LookRotation(new Vector3(-38.2f,1.3f,2.5f)-transform.position);
            view.orthographicSize=Mathf.Max(4.6f,7.6f/Mathf.Max(.5f,view.aspect));
        }
        private void LateUpdate()
        {
            if(game==null) return;
            if(game.InMainMenu) {MenuPose();return;}
            if(game.Paused) return;
            transform.position = Vector3.SmoothDamp(transform.position, Target + offset, ref velocity, 0.18f);
            transform.rotation = Quaternion.LookRotation(-offset);
            view.orthographicSize = Mathf.Max(5.8f, 9.2f / Mathf.Max(0.5f, view.aspect));
        }
    }
}
