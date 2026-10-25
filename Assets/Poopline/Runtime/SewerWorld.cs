using System.Collections.Generic;
using UnityEngine;

namespace Poopline
{
    // Small, deterministic level. All artwork is generated locally; no downloaded assets.
    public sealed class SewerWorld : System.IDisposable
    {
        public const float FinishX = 158;
        private static readonly float[] BaseHazards = { 14, 25, 49, 60, 86, 98, 126, 141 };
        public float[] HazardPositions { get; }
        public static readonly Vector2[] GroundSegments = {
            new Vector2(-12, 34), new Vector2(37, 70),
            new Vector2(73, 110), new Vector2(113, 174)
        };
        private readonly Transform root;
        private readonly SewerCity city;
        private readonly Sprite square;
        private readonly Sprite circle;
        private readonly List<Object> generated = new List<Object>();
        private readonly List<GameObject> coins = new List<GameObject>();
        public int CoinCount => coins.Count;

        public SewerWorld(Transform parent, SewerCity selectedCity)
        {
            city = selectedCity;
            HazardPositions = new float[BaseHazards.Length];
            for (int i = 0; i < BaseHazards.Length; i++)
                HazardPositions[i] = BaseHazards[i] + city.ObstacleOffset * (i % 2 == 0 ? 1 : -1);
            root = new GameObject("Sewer level").transform;
            root.SetParent(parent, false);
            square = MakeSprite(false);
            circle = MakeSprite(true);
            Build();
        }

        private Sprite MakeSprite(bool round)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = round ? "Procedural circle" : "Procedural square";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32, 32));
                    pixels[y * size + x] = new Color(1, 1, 1, round ? Mathf.Clamp01(32 - distance) : 1);
                }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
            generated.Add(sprite);
            generated.Add(texture);
            return sprite;
        }

        private GameObject Shape(string name, Vector2 position, Vector2 size, Color color, int order,
            bool round = false, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent == null ? root : parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = round ? circle : square;
            renderer.color = color;
            renderer.sortingOrder = order;
            return go;
        }

        private void Build()
        {
            Shape("Tunnel darkness", new Vector2(78, 2), new Vector2(210, 48), new Color32(15, 30, 37, 255), -20);
            Shape("Tunnel wall", new Vector2(78, 4), new Vector2(210, 8), city.WallColor, -18);
            CreateBrickwork();
            Shape("City foundations above the tunnel", new Vector2(78, 6.3f), new Vector2(210, 1.3f), new Color32(13, 24, 28, 255), -6);
            Shape("Tunnel ceiling edge", new Vector2(78, 5.68f), new Vector2(210, 0.14f), new Color32(92, 108, 101, 255), -5);
            var entranceWall = Shape("Closed end of collector", new Vector2(-0.7f, 2.5f), new Vector2(0.6f, 6), new Color32(70, 87, 83, 255), 0);
            entranceWall.AddComponent<BoxCollider2D>();
            Shape("Wastewater", new Vector2(78, -2), new Vector2(210, 3), new Color32(49, 93, 73, 255), -8);
            Shape("Waterline", new Vector2(78, -0.8f), new Vector2(210, 0.12f), new Color32(139, 190, 110, 255), -7);
            for (int x = -8; x < 181; x += 8)
            {
                Shape("Pipe support", new Vector2(x, 4), new Vector2(0.25f, 8), new Color32(39, 65, 70, 255), -16);
                Shape("Overhead pipe", new Vector2(x, 5.5f), new Vector2(7.8f, 0.35f), new Color32(56, 84, 85, 255), -15);
                Shape("Lamp glow", new Vector2(x + 2, 4.7f), new Vector2(1.3f, 0.65f), new Color32(153, 211, 147, 30), -14, true);
                Shape("Lamp", new Vector2(x + 2, 4.7f), new Vector2(0.5f, 0.12f), city.Accent, -13);
            }
            Sign(city.Name.ToUpperInvariant() + "\nKANAŁY POD MIASTEM", new Vector2(3.5f, 3.4f));
            Sign("OCZYSZCZALNIA  >", new Vector2(152, 3.4f));

            foreach (Vector2 segment in GroundSegments)
            {
                float width = segment.y - segment.x;
                float center = (segment.x + segment.y) / 2;
                var floor = Shape("Walkway", new Vector2(center, -0.4f), new Vector2(width, 0.8f), new Color32(83, 106, 99, 255), 0);
                floor.AddComponent<BoxCollider2D>();
                Shape("Walkway edge", new Vector2(center, -0.04f), new Vector2(width, 0.08f), new Color32(176, 194, 145, 255), 1);
                for (float x = segment.x + 0.6f; x < segment.y; x += 1.6f)
                    Shape("Walkway joint", new Vector2(x, -0.42f), new Vector2(0.035f, 0.6f), new Color32(43, 63, 63, 255), 1);
            }

            foreach (float x in HazardPositions)
            {
                var hazard = Shape("Blockage — jump over", new Vector2(x, 0.4f), new Vector2(0.95f, 0.8f), new Color32(214, 103, 74, 255), 3, true);
                AddItem(hazard, TrackItemKind.Hazard);
                Shape("Blockage highlight", new Vector2(-0.15f, 0.15f), new Vector2(0.45f, 0.3f), new Color32(244, 158, 94, 255), 4, true, hazard.transform);
                Coin(new Vector2(x - 1.2f, 1.5f));
                Coin(new Vector2(x, 2));
                Coin(new Vector2(x + 1.2f, 1.5f));
            }
            foreach (float x in new[] { 35.5f, 71.5f, 111.5f })
            {
                Coin(new Vector2(x - 1, 1.5f));
                Coin(new Vector2(x, 2));
                Coin(new Vector2(x + 1, 1.5f));
                Shape("Gap warning", new Vector2(x - 2.6f, 0.04f), new Vector2(0.8f, 0.08f), new Color32(255, 210, 110, 255), 2);
            }
            Coin(new Vector2(7, 0.7f));
            Coin(new Vector2(9, 0.7f));

            Shape("Treatment plant", new Vector2(164, 2.1f), new Vector2(8, 4.2f), new Color32(54, 108, 105, 255), -2);
            for (int i = 0; i < 3; i++)
                Shape("Clean tank", new Vector2(162 + i * 2, 2), new Vector2(1.3f, 3), new Color32(118, 194, 176, 255), -1, true);
            var goal = Shape("Treatment plant entrance", new Vector2(FinishX, 2), new Vector2(0.35f, 4), new Color32(182, 234, 145, 255), 2);
            AddItem(goal, TrackItemKind.Finish);
        }

        private void CreateBrickwork()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Repeat;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    pixels[y * size + x] = new Color32(12, 25, 28,
                        (byte)(y % 64 < 2 || (x + (y / 64) * 64) % 128 < 2 ? 90 : 0));
            texture.SetPixels(pixels); texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f,
                size / 1.5f, 0, SpriteMeshType.FullRect);
            generated.Add(sprite); generated.Add(texture);
            var wall = new GameObject("Tiled brickwork");
            wall.transform.SetParent(root, false);
            wall.transform.localPosition = new Vector2(78, 3);
            var renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(210, 6); renderer.sortingOrder = -17;
        }

        private void Sign(string text, Vector2 position)
        {
            Shape("Collector sign", position, new Vector2(4.5f, 1.1f), new Color32(16, 32, 36, 255), 1);
            var go = new GameObject("City sign");
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;
            var label = go.AddComponent<TextMesh>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 48;
            label.characterSize = 0.07f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = city.Accent;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = label.font.material;
            renderer.sortingOrder = 2;
        }

        private void Coin(Vector2 position)
        {
            var coin = Shape("Bubble", position, Vector2.one * 0.42f, new Color32(246, 209, 111, 255), 5, true);
            AddItem(coin, TrackItemKind.Coin);
            Shape("Bubble shine", new Vector2(-0.15f, 0.15f), Vector2.one * 0.24f, Color.white, 6, true, coin.transform);
            coins.Add(coin);
        }

        private static void AddItem(GameObject go, TrackItemKind kind)
        {
            go.layer = 2; // Ground probes must ignore triggers.
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<TrackItem>().Kind = kind;
        }

        public RunnerBody CreatePlayer(PooplineGame game)
        {
            var player = new GameObject("Poop — temporary character");
            player.transform.SetParent(root, false);
            var appearance = new GameObject("Appearance").transform;
            appearance.SetParent(player.transform, false);
            Color brown = new Color32(151, 94, 65, 255);
            Shape("Base", new Vector2(0, -0.03f), new Vector2(0.9f, 0.55f), brown, 10, true, appearance);
            Shape("Middle", new Vector2(-0.03f, 0.2f), new Vector2(0.65f, 0.45f), brown, 11, true, appearance);
            Shape("Top", new Vector2(0.03f, 0.43f), new Vector2(0.34f, 0.35f), brown, 12, true, appearance);
            foreach (float x in new[] { -0.16f, 0.16f })
            {
                Shape("Eye", new Vector2(x, 0.12f), new Vector2(0.19f, 0.22f), Color.white, 14, true, appearance);
                Shape("Pupil", new Vector2(x + 0.035f, 0.12f), new Vector2(0.08f, 0.11f), new Color32(30, 32, 33, 255), 15, true, appearance);
            }
            Shape("Smile", new Vector2(0.04f, -0.07f), new Vector2(0.23f, 0.08f), Color.white, 14, true, appearance);
            var runner = player.AddComponent<RunnerBody>();
            runner.Initialize(game);
            return runner;
        }

        public void ResetCoins() { foreach (var coin in coins) coin.SetActive(true); }
        public void Dispose()
        {
            if (root != null) { root.gameObject.SetActive(false); Object.Destroy(root.gameObject); }
            foreach (var item in generated) Object.Destroy(item);
        }
    }
}
