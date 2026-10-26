using UnityEngine;
namespace Poopline
{
    public sealed class FlowVolume : MonoBehaviour
    {
        public Vector3 size = new Vector3(30, 1, 3.6f);
        public Vector3 localCurrent = new Vector3(0.9f, 0, 0);
        public Vector3 Current => transform.TransformDirection(localCurrent);
        public bool ContainsFeet(Vector3 feet)
        {
            Vector3 local = transform.InverseTransformPoint(feet + Vector3.up * 0.15f);
            return Mathf.Abs(local.x) < size.x * 0.5f && Mathf.Abs(local.z) < size.z * 0.5f && Mathf.Abs(local.y) < size.y * 0.5f;
        }
        private void OnDrawGizmosSelected()
        { Gizmos.color = Color.cyan; Gizmos.matrix = transform.localToWorldMatrix; Gizmos.DrawWireCube(Vector3.zero, size); Gizmos.DrawRay(Vector3.zero, localCurrent * 2); }
    }
}
