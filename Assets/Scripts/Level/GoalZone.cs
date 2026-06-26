using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GoalZone : MonoBehaviour
{
    private void Reset()
    {
        Collider2D goalCollider = GetComponent<Collider2D>();
        goalCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D goalCollider = GetComponent<Collider2D>();
        if (goalCollider != null)
        {
            goalCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        GameManager.Instance?.Win();
    }
}
