using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Obstacle : MonoBehaviour
{
    [SerializeField] private bool canLoseGame = true;

    private void Reset()
    {
        Collider2D obstacleCollider = GetComponent<Collider2D>();
        obstacleCollider.isTrigger = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryLose(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryLose(other);
    }

    private void TryLose(Collider2D other)
    {
        if (!canLoseGame || other == null || other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        GameManager.Instance?.Lose();
    }
}
