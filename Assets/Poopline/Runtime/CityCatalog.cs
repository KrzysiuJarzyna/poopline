using UnityEngine;

namespace Poopline
{
    public sealed class SewerCity
    {
        public string Id { get; }
        public string Name { get; }
        public Vector2 Coordinates { get; }
        public Color WallColor { get; }
        public Color Accent { get; }
        public float ObstacleOffset { get; }
        public string BestKey => "Poopline.City." + Id + ".BestCoins.v2";

        public SewerCity(string id, string name, Vector2 coordinates, Color wall, Color accent, float offset)
        {
            Id = id; Name = name; Coordinates = coordinates;
            WallColor = wall; Accent = accent; ObstacleOffset = offset;
        }
    }

    public static class CityCatalog
    {
        // Approximate city centres, longitude then latitude. Offline map, no location permission.
        public static readonly SewerCity[] All = {
            new SewerCity("mielec", "Mielec", new Vector2(21.424f, 50.287f), new Color32(43, 60, 55, 255), new Color32(190, 226, 131, 255), 0),
            new SewerCity("poznan", "Poznań", new Vector2(16.925f, 52.407f), new Color32(37, 56, 68, 255), new Color32(145, 212, 234, 255), 0.8f),
            new SewerCity("warszawa", "Warszawa", new Vector2(21.012f, 52.230f), new Color32(62, 49, 53, 255), new Color32(236, 171, 151, 255), -0.8f),
            new SewerCity("wroclaw", "Wrocław", new Vector2(17.038f, 51.107f), new Color32(55, 47, 66, 255), new Color32(203, 181, 235, 255), 1.2f),
            new SewerCity("krakow", "Kraków", new Vector2(19.945f, 50.065f), new Color32(63, 53, 40, 255), new Color32(233, 200, 135, 255), -1.2f),
            new SewerCity("gdansk", "Gdańsk", new Vector2(18.646f, 54.352f), new Color32(31, 61, 66, 255), new Color32(142, 222, 207, 255), 0.4f)
        };

        public static int FindIndex(string id)
        {
            for (int i = 0; i < All.Length; i++) if (All[i].Id == id) return i;
            return 0;
        }
    }
}
