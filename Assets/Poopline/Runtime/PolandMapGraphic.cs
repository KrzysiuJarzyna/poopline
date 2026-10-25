using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Poopline
{
    // Natural Earth 1:110m admin-0, Poland. Public domain; see MAP_SOURCES.md.
    public sealed class PolandMapGraphic : Graphic
    {
        private static readonly Vector2[] Border = {
            new Vector2(23.484128f, 53.912498f),
            new Vector2(23.527536f, 53.470122f),
            new Vector2(23.804935f, 53.089731f),
            new Vector2(23.799199f, 52.691099f),
            new Vector2(23.199494f, 52.486977f),
            new Vector2(23.508002f, 52.023647f),
            new Vector2(23.527071f, 51.578454f),
            new Vector2(24.029986f, 50.705407f),
            new Vector2(23.922757f, 50.424881f),
            new Vector2(23.426508f, 50.308506f),
            new Vector2(22.518450f, 49.476774f),
            new Vector2(22.776419f, 49.027395f),
            new Vector2(22.558138f, 49.085738f),
            new Vector2(21.607808f, 49.470107f),
            new Vector2(20.887955f, 49.328772f),
            new Vector2(20.415839f, 49.431453f),
            new Vector2(19.825023f, 49.217125f),
            new Vector2(19.320713f, 49.571574f),
            new Vector2(18.909575f, 49.435846f),
            new Vector2(18.853144f, 49.496230f),
            new Vector2(18.392914f, 49.988629f),
            new Vector2(17.649445f, 50.049038f),
            new Vector2(17.554567f, 50.362146f),
            new Vector2(16.868769f, 50.473974f),
            new Vector2(16.719476f, 50.215747f),
            new Vector2(16.176253f, 50.422607f),
            new Vector2(16.238627f, 50.697733f),
            new Vector2(15.490972f, 50.784730f),
            new Vector2(15.016996f, 51.106674f),
            new Vector2(14.607098f, 51.745188f),
            new Vector2(14.685026f, 52.089947f),
            new Vector2(14.437600f, 52.624850f),
            new Vector2(14.074521f, 52.981263f),
            new Vector2(14.353315f, 53.248171f),
            new Vector2(14.119686f, 53.757029f),
            new Vector2(14.802900f, 54.050706f),
            new Vector2(16.363477f, 54.513159f),
            new Vector2(17.622832f, 54.851536f),
            new Vector2(18.620859f, 54.682606f),
            new Vector2(18.696255f, 54.438719f),
            new Vector2(19.660640f, 54.426084f),
            new Vector2(20.892245f, 54.312525f),
            new Vector2(22.731099f, 54.327537f),
            new Vector2(23.243987f, 54.220567f),
        };

        public static Vector2 Project(Vector2 longitudeLatitude, Vector2 size)
        {
            float scale = Mathf.Min(size.x / 6.35f, size.y / 6f);
            return new Vector2((longitudeLatitude.x - 19.1f) * 0.6157f,
                longitudeLatitude.y - 52f) * scale;
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var remaining = new List<int>();
            // GeoJSON ring is clockwise; reverse it for ear clipping.
            for (int i = Border.Length - 1; i >= 0; i--)
            {
                remaining.Add(Border.Length - 1 - i);
                mesh.AddVert(Project(Border[i], rectTransform.rect.size), color, Vector2.zero);
            }
            var vertices = new Vector2[Border.Length];
            for (int i = 0; i < vertices.Length; i++) vertices[i] = Border[Border.Length - 1 - i];
            int guard = vertices.Length * vertices.Length;
            while (remaining.Count > 2 && guard-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int a = remaining[(i + remaining.Count - 1) % remaining.Count];
                    int b = remaining[i], c = remaining[(i + 1) % remaining.Count];
                    if (Cross(vertices[b] - vertices[a], vertices[c] - vertices[b]) <= 0) continue;
                    bool contains = false;
                    foreach (int p in remaining)
                    {
                        if (p == a || p == b || p == c) continue;
                        if (Cross(vertices[b] - vertices[a], vertices[p] - vertices[a]) >= 0 &&
                            Cross(vertices[c] - vertices[b], vertices[p] - vertices[b]) >= 0 &&
                            Cross(vertices[a] - vertices[c], vertices[p] - vertices[c]) >= 0)
                        { contains = true; break; }
                    }
                    if (contains) continue;
                    mesh.AddTriangle(a, b, c);
                    remaining.RemoveAt(i); clipped = true; break;
                }
                if (!clipped) break;
            }
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
