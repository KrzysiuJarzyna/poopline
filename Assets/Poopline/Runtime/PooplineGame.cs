using UnityEngine;

namespace Poopline
{
    [DisallowMultipleComponent]
    public sealed class PooplineGame : MonoBehaviour
    {
        public const int LayoutVersion = 8;
        public int layoutVersion;
        public SewerPlayer Player { get; private set; }
        public RunnerInput Input { get; private set; }
        public FlowVolume[] Flows { get; private set; }
        public SewerLadder[] Ladders { get; private set; }
        public RatPatrol[] Rats { get; private set; }
        public ElevatedWalkway[] Decks { get; private set; }
        public SewerLevel[] Levels { get; private set; }
        public Camera GameCamera { get; private set; }
        public int LevelIndex { get; private set; }
        public SewerLevel CurrentLevel => Levels[LevelIndex];
        public int CheckpointIndex { get; private set; } = -1;
        public bool CheckpointReached => CheckpointIndex >= 0;
        public bool Paused { get; private set; }
        public bool Completed { get; private set; }
        public bool Transitioning { get; private set; }
        public bool MapOpen { get; private set; }
        public ExplorationMap Map => maps[LevelIndex];
        private ExplorationMap[] maps;
        public bool Frozen => Paused || Completed || Transitioning || MapOpen;
        public string Notice { get; private set; }
        public float NoticeUntil { get; private set; }
        public float Elapsed { get; private set; }
        public float LevelElapsed { get; private set; }
        public float[] StageTimes { get; private set; }
        public int Recoveries { get; private set; }
        public Vector3 Spawn => CheckpointReached ? CurrentLevel.checkpoints[CheckpointIndex] : CurrentLevel.start;
        public string Objective => CurrentLevel.objectives[Mathf.Clamp(CheckpointIndex + 1, 0, CurrentLevel.objectives.Length - 1)];
        private float protectedUntil, transitionRemaining;
        private int previousFrameRate;

        private void Awake()
        {
            Input = new RunnerInput();
            Player = GetComponentInChildren<SewerPlayer>();
            Levels = GetComponentsInChildren<SewerLevel>(true);
            GameCamera = GetComponentInChildren<Camera>();
            previousFrameRate = Application.targetFrameRate;
            Application.targetFrameRate = 60;
            if (Player == null || GameCamera == null || Levels.Length != 2)
            { Debug.LogError("Open Tools > Poopline > Rebuild playable canal before Play."); enabled = false; return; }
            StageTimes = new float[Levels.Length];
            maps = new ExplorationMap[Levels.Length];
            for(int i=0;i<Levels.Length;i++) maps[i]=new ExplorationMap(Levels[i], i);
            Player.Initialize(this);
            GameCamera.GetComponent<SewerCamera>().Initialize(this);
            GetComponent<RunnerHud>().Initialize(this);
            // Cache original lamp values once, including the inactive second stage.
            GetComponent<SewerLighting>().Initialize(this);
            RestartJourney();
        }

        private void Update()
        {
            if (Input.RetryPressed) { Restart(); return; }
            if (Input.MapPressed && !Completed && !Transitioning) ToggleMap();
            if (Input.PausePressed && !Completed)
            {
                if(MapOpen) ToggleMap(); else TogglePause();
            }
            if (Paused || Completed || MapOpen) return;
            if (Transitioning)
            {
                transitionRemaining -= Time.deltaTime;
                if (transitionRemaining <= 0) StartLevel(LevelIndex + 1);
                return;
            }
            Elapsed += Time.deltaTime;
            LevelElapsed += Time.deltaTime;
            Vector3 p = Player.transform.position;
            Map.Discover(p);
            for (int i = CheckpointIndex + 1; i < CurrentLevel.checkpoints.Length; i++)
            {
                if (Vector3.Distance(p, CurrentLevel.checkpoints[i]) > 2) continue;
                CheckpointIndex = i;
                ShowNotice("Punkt kontrolny · " + Objective, 5);
                break;
            }
            if (!CurrentLevel.ContainsPlayer(p)) { Recover("Wracasz na ostatni bezpieczny pomost."); return; }
            if (!CurrentLevel.ContainsExit(p)) return;
            Map.Save();
            StageTimes[LevelIndex] = LevelElapsed;
            Player.StopMotion();
            if (LevelIndex + 1 < Levels.Length)
            { Transitioning = true; transitionRemaining = 1.4f; }
            else Completed = true;
        }

        private void StartLevel(int index)
        {
            LevelIndex = index;
            for (int i = 0; i < Levels.Length; i++) Levels[i].gameObject.SetActive(i == index);
            Flows = CurrentLevel.GetComponentsInChildren<FlowVolume>();
            Ladders = CurrentLevel.GetComponentsInChildren<SewerLadder>();
            Decks = CurrentLevel.GetComponentsInChildren<ElevatedWalkway>();
            Rats = CurrentLevel.GetComponentsInChildren<RatPatrol>();
            foreach (var rat in Rats) { rat.Initialize(this); rat.ResetPatrol(); }
            CheckpointIndex = -1;
            Paused = Completed = Transitioning = MapOpen = false;
            LevelElapsed = 0;
            protectedUntil = Time.time + 1.5f;
            Player.ResetAt(Spawn);
            Map.Discover(Spawn, true);
            GameCamera.GetComponent<SewerCamera>().Snap();
            ShowNotice(CurrentLevel.introduction, 7);
        }

        public void ToggleMap()
        {
            MapOpen = !MapOpen; Player.StopMotion();
            if(MapOpen) Map.Save();
        }
        public void TogglePause() { Paused = !Paused; Player.StopMotion(); }
        public void Restart()
        {
            if (Completed) { RestartJourney(); return; }
            Elapsed -= LevelElapsed;
            StageTimes[LevelIndex] = 0;
            StartLevel(LevelIndex);
        }
        public void RestartJourney()
        {
            Elapsed = 0; Recoveries = 0;
            System.Array.Clear(StageTimes, 0, StageTimes.Length);
            StartLevel(0);
        }
        public void Recover(string message)
        {
            if (Frozen || Time.time < protectedUntil) return;
            Recoveries++;
            Player.ResetAt(Spawn);
            foreach (var rat in Rats) rat.ResetPatrol();
            protectedUntil = Time.time + 2;
            GameCamera.GetComponent<SewerCamera>().Snap();
            ShowNotice(message, 4);
        }
        public void ShowNotice(string message, float duration) { Notice = message; NoticeUntil = Time.unscaledTime + duration; }
        private void OnApplicationFocus(bool focused)
        { if (!focused && Player != null && !Paused && !Completed) TogglePause(); }
        private void OnApplicationPause(bool paused) { if (paused) OnApplicationFocus(false); }
        private void OnDestroy() { if(maps!=null) foreach(var map in maps) map.Save(); Input?.Dispose(); Application.targetFrameRate = previousFrameRate; }
    }
}
