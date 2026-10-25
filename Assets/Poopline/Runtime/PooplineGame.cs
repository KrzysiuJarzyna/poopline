using UnityEngine;

namespace Poopline
{
    [DisallowMultipleComponent]
    public sealed class PooplineGame : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("forwardSpeed")]
        [SerializeField, Range(3, 7)] private float moveSpeed = 5;
        [SerializeField, Range(9, 14)] private float jumpSpeed = 11;
        public float MoveSpeed => moveSpeed;
        public int SelectedCityIndex { get; private set; }
        public SewerCity CurrentCity => CityCatalog.All[SelectedCityIndex];
        public float[] Hazards => world.HazardPositions;
        public float JumpSpeed => jumpSpeed;
        public RunState State { get; } = new RunState();
        public RunnerInput Input { get; private set; }
        public RunnerBody Player { get; private set; }
        public int TotalCoins => world.CoinCount;
        public int BestCoins { get; private set; }
        public bool Muted { get; private set; }
        public float Progress => Player == null ? 0 : Mathf.Clamp01((Player.Position.x - 2) / (SewerWorld.FinishX - 2));
        private SewerWorld world;
        private RunnerHud hud;
        private Camera gameCamera;
        private AudioSource sound;
        private AudioClip tone;
        private int previousFrameRate;
        private bool mapAxisHeld;
        private float cameraLookAhead;

        private void Awake()
        {
            previousFrameRate = Application.targetFrameRate;
            Application.targetFrameRate = 60;
            Input = new RunnerInput();
            SelectedCityIndex = CityCatalog.FindIndex(PlayerPrefs.GetString("Poopline.SelectedCity.v2", "mielec"));
            BestCoins = PlayerPrefs.GetInt(CurrentCity.BestKey, 0);
            Muted = PlayerPrefs.GetInt("Poopline.Muted.v1", 0) != 0;
            world = new SewerWorld(transform, CurrentCity);
            Player = world.CreatePlayer(this);
            var cameraObject = new GameObject("Poopline Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.tag = "MainCamera";
            gameCamera = cameraObject.GetComponent<Camera>();
            gameCamera.orthographic = true;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color32(15, 30, 37, 255);
            gameCamera.nearClipPlane = 0.1f;
            gameCamera.farClipPlane = 60;
            sound = gameObject.AddComponent<AudioSource>();
            sound.playOnAwake = false;
            sound.volume = 0.15f;
            tone = CreateTone();
            hud = gameObject.AddComponent<RunnerHud>();
            hud.Initialize(this);
            UpdateCamera();
        }

        private static AudioClip CreateTone()
        {
            const int sampleRate = 22050;
            var samples = new float[sampleRate / 8];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = Mathf.Sin(2 * Mathf.PI * 440 * i / sampleRate) * Mathf.Sin(Mathf.PI * i / samples.Length) * 0.3f;
            var clip = AudioClip.Create("Poopline soft tone", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Update()
        {
            State.Tick(Time.deltaTime);
            if (State.Phase == RunPhase.CityMap)
            {
                float axis = Input.Horizontal;
                if (!mapAxisHeld && Mathf.Abs(axis) > 0.5f)
                    SelectCity((SelectedCityIndex + (axis > 0 ? 1 : CityCatalog.All.Length - 1)) % CityCatalog.All.Length);
                mapAxisHeld = Mathf.Abs(axis) > 0.2f;
                if (Input.AcceptPressed) PrimaryAction();
                return;
            }
            if (Input.PausePressed) TogglePause();
            else if (Input.RetryPressed) Restart();
            else if (Input.AcceptPressed) PrimaryAction();
        }

        private void LateUpdate() { UpdateCamera(); }
        private void UpdateCamera()
        {
            gameCamera.orthographicSize = Mathf.Max(4.4f, 5.6f / Mathf.Max(0.25f, gameCamera.aspect));
            cameraLookAhead = Mathf.MoveTowards(cameraLookAhead, Input.Horizontal * 1.6f, Time.deltaTime * 5);
            gameCamera.transform.position = new Vector3(Player.transform.position.x + cameraLookAhead, 2.4f, -10);
        }

        public void PrimaryAction()
        {
            switch (State.Phase)
            {
                case RunPhase.CityMap: StartSelectedCity(); break;
                case RunPhase.Ready: State.Start(); Player.SetRunning(true); break;
                case RunPhase.Playing: Player.QueueJump(); break;
                case RunPhase.Paused: TogglePause(); break;
                default: Restart(); break;
            }
        }

        public void TogglePause()
        {
            if (State.Phase == RunPhase.Playing) { State.Pause(); Input.ClearTouch(); Player.SetRunning(false); }
            else if (State.Phase == RunPhase.Paused) { State.Resume(); Player.SetRunning(true); }
        }

        public void Restart()
        {
            if (State.Phase == RunPhase.CityMap) return;
            Input.ClearTouch();
            State.Reset();
            world.ResetCoins();
            Player.ResetPosition();
            UpdateCamera();
            PrimaryAction();
        }

        public void SelectCity(int index)
        {
            if (State.Phase != RunPhase.CityMap || index < 0 || index >= CityCatalog.All.Length) return;
            SelectedCityIndex = index;
            BestCoins = PlayerPrefs.GetInt(CurrentCity.BestKey, 0);
        }

        private void StartSelectedCity()
        {
            Input.ClearTouch();
            world.Dispose();
            world = new SewerWorld(transform, CurrentCity);
            Player = world.CreatePlayer(this);
            State.Reset();
            State.Start();
            Player.SetRunning(true);
            cameraLookAhead = 0;
            PlayerPrefs.SetString("Poopline.SelectedCity.v2", CurrentCity.Id);
            PlayerPrefs.Save();
            UpdateCamera();
        }

        public void ReturnToMap()
        {
            Input.ClearTouch();
            Player.SetRunning(false);
            State.ShowMap();
            mapAxisHeld = Mathf.Abs(Input.Horizontal) > 0.2f;
        }

        public void CollectCoin() { State.Collect(); PlayTone(1.6f); }
        public void PlayJumpSound() { PlayTone(0.9f); }
        private void PlayTone(float pitch)
        {
            if (Muted) return;
            sound.pitch = pitch;
            sound.PlayOneShot(tone);
        }

        public void Finish(bool won)
        {
            if (State.Phase != RunPhase.Playing) return;
            if (won) State.Win(); else State.Lose();
            Player.SetRunning(false);
            Input.ClearTouch();
            PlayTone(won ? 2 : 0.45f);
            if (won && State.Coins > BestCoins)
            {
                BestCoins = State.Coins;
                PlayerPrefs.SetInt(CurrentCity.BestKey, BestCoins);
                PlayerPrefs.Save();
            }
        }

        public void ToggleSound()
        {
            Muted = !Muted;
            sound.Stop();
            PlayerPrefs.SetInt("Poopline.Muted.v1", Muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool paused) { if (paused) PauseForInterruption(); }
        private void OnApplicationFocus(bool focused) { if (!focused) PauseForInterruption(); }
        private void PauseForInterruption()
        {
            if (State.Phase == RunPhase.Playing) TogglePause();
        }

        private void OnDestroy()
        {
            Input?.Dispose();
            world?.Dispose();
            if (tone != null) Destroy(tone);
            Application.targetFrameRate = previousFrameRate;
        }
    }
}
