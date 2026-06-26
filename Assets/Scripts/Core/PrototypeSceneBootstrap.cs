using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeSceneBootstrap : MonoBehaviour
{
    [SerializeField] private string bootstrapSceneName = "GamePrototype";
    [SerializeField] private Vector2 playerStartPosition = new Vector2(0f, -6f);
    [SerializeField] private Vector2 playerSize = new Vector2(0.7f, 0.7f);
    [SerializeField] private float goalY = 42f;
    [SerializeField] private Vector2 channelSize = new Vector2(9f, 56f);
    [SerializeField] private Color channelColor = new Color(0.12f, 0.18f, 0.2f, 1f);
    [SerializeField] private Color wallColor = new Color(0.28f, 0.28f, 0.25f, 1f);
    [SerializeField] private Color playerColor = new Color(0.55f, 0.9f, 0.35f, 1f);
    [SerializeField] private Color obstacleColor = new Color(0.65f, 0.22f, 0.16f, 1f);
    [SerializeField] private Color goalColor = new Color(0.15f, 0.7f, 0.95f, 0.75f);

    private static Sprite squareSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "GamePrototype")
        {
            return;
        }

        if (FindFirstObjectByType<PrototypeSceneBootstrap>() == null)
        {
            GameObject bootstrapObject = new GameObject("PrototypeSceneBootstrap");
            bootstrapObject.AddComponent<PrototypeSceneBootstrap>();
        }
    }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != bootstrapSceneName)
        {
            return;
        }

        CreateSceneObjects();
    }

    public void CreateSceneObjects()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            player = CreatePlayer();
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = CreateCamera(player.transform);
        }
        else
        {
            SimpleCameraFollow follow = mainCamera.GetComponent<SimpleCameraFollow>();
            if (follow == null)
            {
                follow = mainCamera.gameObject.AddComponent<SimpleCameraFollow>();
            }

            follow.SetTarget(player.transform);
        }

        if (FindFirstObjectByType<GoalZone>() == null)
        {
            CreateGoalZone();
        }

        if (FindFirstObjectByType<Obstacle>() == null)
        {
            CreateObstacle("Obstacle A", new Vector2(-2.2f, 3f), new Vector2(1.25f, 1.25f));
            CreateObstacle("Obstacle B", new Vector2(1.8f, 10f), new Vector2(1.5f, 1.1f));
            CreateObstacle("Obstacle C", new Vector2(0f, 17f), new Vector2(1.2f, 1.8f));
            CreateObstacle("Obstacle D", new Vector2(-1.7f, 25f), new Vector2(1.6f, 1.2f));
            CreateObstacle("Obstacle E", new Vector2(2.1f, 32f), new Vector2(1.2f, 1.6f));
        }

        if (GameObject.Find("Prototype Channel") == null)
        {
            CreateChannel();
        }
    }

    private PlayerController CreatePlayer()
    {
        GameObject playerObject = CreateColoredRectangle("Player", playerStartPosition, playerSize, playerColor, 2);
        playerObject.tag = "Player";

        Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;

        BoxCollider2D collider = playerObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;

        return playerObject.AddComponent<PlayerController>();
    }

    private Camera CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.06f, 0.07f, 1f);

        SimpleCameraFollow follow = cameraObject.AddComponent<SimpleCameraFollow>();
        follow.SetTarget(target);
        cameraObject.transform.position = target.position + new Vector3(0f, 2f, -10f);

        return camera;
    }

    private void CreateGoalZone()
    {
        GameObject goalObject = CreateColoredRectangle("GoalZone", new Vector2(0f, goalY), new Vector2(channelSize.x, 1.25f), goalColor, 1);
        BoxCollider2D collider = goalObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        goalObject.AddComponent<GoalZone>();
    }

    private void CreateObstacle(string objectName, Vector2 position, Vector2 size)
    {
        GameObject obstacleObject = CreateColoredRectangle(objectName, position, size, obstacleColor, 1);
        BoxCollider2D collider = obstacleObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        obstacleObject.AddComponent<Obstacle>();
    }

    private void CreateChannel()
    {
        GameObject parent = new GameObject("Prototype Channel");

        GameObject water = CreateColoredRectangle("Water", new Vector2(0f, channelSize.y * 0.35f), channelSize, channelColor, -2);
        water.transform.SetParent(parent.transform);

        float wallHeight = channelSize.y;
        float wallY = channelSize.y * 0.35f;
        float wallX = channelSize.x * 0.5f + 0.4f;

        GameObject leftWall = CreateColoredRectangle("Left Wall", new Vector2(-wallX, wallY), new Vector2(0.8f, wallHeight), wallColor, -1);
        leftWall.transform.SetParent(parent.transform);

        GameObject rightWall = CreateColoredRectangle("Right Wall", new Vector2(wallX, wallY), new Vector2(0.8f, wallHeight), wallColor, -1);
        rightWall.transform.SetParent(parent.transform);
    }

    private GameObject CreateColoredRectangle(string objectName, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        GameObject rectangle = new GameObject(objectName);
        rectangle.transform.position = position;
        rectangle.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer spriteRenderer = rectangle.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSquareSprite();
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;

        return rectangle;
    }

    private static Sprite GetSquareSprite()
    {
        if (squareSprite != null)
        {
            return squareSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return squareSprite;
    }
}
