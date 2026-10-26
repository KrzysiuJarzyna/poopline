using UnityEngine;

namespace Poopline
{
    [System.Serializable]
    public struct MapCell
    {
        public Vector2 position;
        public bool water;
        public MapCell(float x, float z, bool wet) { position=new Vector2(x,z); water=wet; }
    }
    [System.Serializable]
    public struct MapLandmark
    {
        public string name;
        public Vector3 position;
        public bool exit;
        public MapLandmark(string label, Vector3 point, bool isExit=false) { name=label; position=point; exit=isExit; }
    }
    // Both stages are authored scene objects. Only the current stage is active in Play.
    public sealed class SewerLevel : MonoBehaviour
    {
        public string title;
        [TextArea] public string introduction;
        public Vector3 start;
        public Bounds exit;
        public Bounds playableBounds;
        public Vector3[] checkpoints;
        public string[] objectives;
        public Vector3[] suggestedRoute;
        public MapCell[] mapCells;
        public MapLandmark[] landmarks;
        public Vector3[] alternateRoute;

        public bool ContainsExit(Vector3 feet) => exit.Contains(feet);
        public bool ContainsPlayer(Vector3 feet) => playableBounds.Contains(feet);
    }
}
