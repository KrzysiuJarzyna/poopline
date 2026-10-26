using UnityEngine;

namespace Poopline
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ElevatedWalkway : MonoBehaviour
    {
        private BoxCollider deck;
        public bool Supports(Vector3 feet, float inset = .14f)
        {
            if (deck == null) deck = GetComponent<BoxCollider>();
            Bounds bounds = deck.bounds;
            return Mathf.Abs(feet.y - bounds.max.y) < .4f &&
                feet.x >= bounds.min.x + inset && feet.x <= bounds.max.x - inset &&
                feet.z >= bounds.min.z + inset && feet.z <= bounds.max.z - inset;
        }
    }
}
