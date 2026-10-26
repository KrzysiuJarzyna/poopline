using UnityEngine;
using UnityEngine.Rendering;

namespace Poopline
{
    // Keep the masonry and its shadows, but reveal the player behind a doorway or arch.
    public sealed class SewerCutaway : MonoBehaviour
    {
        private PooplineGame game;
        private Renderer[] surfaces;
        private ShadowCastingMode[] originalModes;
        private Bounds bounds;
        public bool Hidden { get; private set; }

        private void Awake()
        {
            game = GetComponentInParent<PooplineGame>();
            surfaces = GetComponentsInChildren<Renderer>();
            originalModes = new ShadowCastingMode[surfaces.Length];
            if (surfaces.Length == 0) return;
            bounds = surfaces[0].bounds;
            for (int i = 0; i < surfaces.Length; i++)
            { bounds.Encapsulate(surfaces[i].bounds); originalModes[i] = surfaces[i].shadowCastingMode; }
        }
        private void LateUpdate()
        {
            if (game == null || game.Player == null || surfaces.Length == 0) return;
            Vector3 from = game.GameCamera.transform.position;
            Vector3 feet = game.Player.transform.position;
            bool hide = Blocks(from, feet + Vector3.up * .15f) ||
                Blocks(from, feet + Vector3.up * .9f) || Blocks(from, feet + Vector3.up * 1.7f);
            if (hide == Hidden) return;
            Hidden = hide;
            for (int i = 0; i < surfaces.Length; i++)
                surfaces[i].shadowCastingMode = hide ? ShadowCastingMode.ShadowsOnly : originalModes[i];
        }
        private bool Blocks(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            return bounds.IntersectRay(new Ray(from, delta.normalized), out float distance) && distance < delta.magnitude;
        }
    }
}
