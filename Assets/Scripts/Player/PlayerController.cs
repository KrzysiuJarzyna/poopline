using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float horizontalSpeed = 6f;
    [SerializeField] private float horizontalLimit = 4.25f;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = new Vector2(horizontalInput * horizontalSpeed, forwardSpeed);
        body.linearVelocity = velocity;

        if (horizontalLimit > 0f)
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, -horizontalLimit, horizontalLimit);
            transform.position = position;
        }
    }
}
