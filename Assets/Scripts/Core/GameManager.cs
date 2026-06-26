using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Won,
        Lost,
        Paused
    }

    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState currentState = GameState.Playing;

    private GameState stateBeforePause = GameState.Playing;

    public GameState CurrentState => currentState;
    public bool IsPlaying => currentState == GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && (currentState == GameState.Won || currentState == GameState.Lost))
        {
            RestartScene();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && (currentState == GameState.Playing || currentState == GameState.Paused))
        {
            TogglePause();
        }
    }

    public void Win()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentState = GameState.Won;
        Time.timeScale = 1f;
        Debug.Log("Game won. Press R to restart.");
    }

    public void Lose()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentState = GameState.Lost;
        Time.timeScale = 1f;
        Debug.Log("Game lost. Press R to restart.");
    }

    public void TogglePause()
    {
        if (currentState == GameState.Paused)
        {
            currentState = stateBeforePause;
            Time.timeScale = 1f;
            Debug.Log("Game resumed.");
            return;
        }

        stateBeforePause = currentState;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        Debug.Log("Game paused. Press Escape to resume.");
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        if (activeScene.buildIndex < 0 && !string.IsNullOrEmpty(activeScene.path))
        {
            EditorSceneManager.LoadSceneInPlayMode(activeScene.path, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
            return;
        }

        SceneManager.LoadScene(activeScene.name);
    }
}
