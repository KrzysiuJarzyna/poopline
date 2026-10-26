using UnityEngine;

namespace Poopline
{
    // Darkness reveals the current stretch of canal; it is not a persistent exploration map.
    public sealed class SewerLighting : MonoBehaviour
    {
        public float clearDistance = 4.5f, darkDistance = 9f;
        private PooplineGame game;
        private Transform human;
        private Light phone;
        private Transform phoneGlow;
        private Light[] lamps;
        private float[] lampIntensities;
        private Renderer[] fixtures;
        private Color[] fixtureEmission;
        private MaterialPropertyBlock properties;

        public void Initialize(PooplineGame owner)
        {
            game = owner;
            human = game.Player.transform.Find("Human");
            phone = human.Find("Left arm/Phone light").GetComponent<Light>();
            phoneGlow = human.Find("Left arm/Phone reflected glow");
            var lights = new System.Collections.Generic.List<Light>();
            foreach (var light in GetComponentsInChildren<Light>(true))
                if (!light.transform.IsChildOf(game.Player.transform)) lights.Add(light);
            lamps = lights.ToArray();
            lampIntensities = new float[lamps.Length];
            for (int i = 0; i < lamps.Length; i++) lampIntensities[i] = lamps[i].intensity;

            var glowing = new System.Collections.Generic.List<Renderer>();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                if (!renderer.transform.IsChildOf(game.Player.transform) && renderer.sharedMaterial.IsKeywordEnabled("_EMISSION"))
                    glowing.Add(renderer);
            fixtures = glowing.ToArray();
            fixtureEmission = new Color[fixtures.Length];
            for (int i = 0; i < fixtures.Length; i++) fixtureEmission[i] = fixtures[i].sharedMaterial.GetColor("_EmissionColor");
            properties = new MaterialPropertyBlock();
        }

        private float Visibility(Vector3 position)
        {
            Vector3 delta = position - (game.InMainMenu?PooplineGame.MenuFocus:game.Player.transform.position);
            float distance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.z) * .65f);
            return 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(clearDistance, darkDistance, distance));
        }

        private void LateUpdate()
        {
            if (game == null) return;
            // Keep the beam on the path instead of letting the walking arm swing it above the floor.
            phone.transform.rotation = Quaternion.LookRotation(human.forward + Vector3.down * .5f);
            Vector3 towardCamera = game.GameCamera.transform.position - human.position;
            towardCamera.y = 0;
            phoneGlow.position = phone.transform.position + towardCamera.normalized * .8f + Vector3.up * .3f;
            for (int i = 0; i < lamps.Length; i++)
            {
                float visibility = Visibility(lamps[i].transform.position);
                lamps[i].enabled = visibility > .001f;
                lamps[i].intensity = lampIntensities[i] * visibility;
            }
            for (int i = 0; i < fixtures.Length; i++)
            {
                fixtures[i].GetPropertyBlock(properties);
                properties.SetColor("_EmissionColor", fixtureEmission[i] * Visibility(fixtures[i].transform.position));
                fixtures[i].SetPropertyBlock(properties);
            }
        }
    }
}
