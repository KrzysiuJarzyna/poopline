using UnityEngine;
namespace Poopline
{
    public sealed class RatPatrol : MonoBehaviour
    {
        public Vector3 start = new Vector3(18, 0.12f, -3.5f), end = new Vector3(23.5f, 0.12f, -3.5f);
        public float speed = 1.1f;
        private float distance;
        private PooplineGame game;
        public void Initialize(PooplineGame owner) { game = owner; }
        public void ResetPatrol() { distance = 0; transform.position = start; transform.rotation = Quaternion.Euler(0, 90, 0); }
        private void Update()
        {
            if (game == null || game.Frozen) return;
            distance += Time.deltaTime * speed;
            float length = Vector3.Distance(start, end);
            float t = Mathf.PingPong(distance, length) / length;
            transform.position = Vector3.Lerp(start, end, t);
            Vector3 direction = (Mathf.Repeat(distance, length * 2) < length ? end - start : start - end).normalized;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 240);
            Vector3 delta = game.Player.transform.position - transform.position;
            if (Mathf.Abs(delta.y) < 0.8f && new Vector2(delta.x, delta.z).magnitude < 0.72f)
            {
                int before=game.Recoveries;
                game.Recover("Powrót do punktu kontrolnego.");
                if(game.Recoveries>before) game.Thoughts.Say(ThoughtCue.RatEscape);
            }
        }
    }
}
