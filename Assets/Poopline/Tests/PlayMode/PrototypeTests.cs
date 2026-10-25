using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Poopline.Tests
{
    public sealed class PrototypeTests
    {
        private PooplineGame game;
        private readonly System.Collections.Generic.Dictionary<string, int?> originalRecords = new System.Collections.Generic.Dictionary<string, int?>();
        private string originalCity;
        private float originalTimeScale;
        private Keyboard keyboard;
        private Gamepad gamepad;
        private InputSettings.BackgroundBehavior originalBackgroundInput;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode originalEditorInput;
#endif

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            originalBackgroundInput = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            // Batch mode has no focused Game view. Route synthetic keyboard events to the game.
            originalEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            originalTimeScale = Time.timeScale;
            originalCity = PlayerPrefs.HasKey("Poopline.SelectedCity.v2") ? PlayerPrefs.GetString("Poopline.SelectedCity.v2") : null;
            originalRecords.Clear();
            foreach (var city in CityCatalog.All)
                originalRecords[city.BestKey] = PlayerPrefs.HasKey(city.BestKey) ? (int?)PlayerPrefs.GetInt(city.BestKey) : null;
            yield return SceneManager.LoadSceneAsync("Poopline");
            game = Object.FindAnyObjectByType<PooplineGame>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.CityMap));
            game.SelectCity(0);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            InputSystem.settings.backgroundBehavior = originalBackgroundInput;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorInput;
#endif
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (gamepad != null) InputSystem.RemoveDevice(gamepad);
            keyboard = null;
            gamepad = null;
            Time.timeScale = originalTimeScale;
            foreach (var record in originalRecords)
                if (record.Value.HasValue) PlayerPrefs.SetInt(record.Key, record.Value.Value);
                else PlayerPrefs.DeleteKey(record.Key);
            if (originalCity == null) PlayerPrefs.DeleteKey("Poopline.SelectedCity.v2");
            else PlayerPrefs.SetString("Poopline.SelectedCity.v2", originalCity);
            PlayerPrefs.Save();
            if (game != null) Object.Destroy(game.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PauseFreezesBodyAndClockThenRestartClearsRun()
        {
            game.PrimaryAction();
            keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return new WaitForSeconds(0.3f);
            game.CollectCoin();
            game.TogglePause();
            Vector2 position = game.Player.Position;
            float seconds = game.State.Seconds;
            yield return new WaitForSeconds(0.2f);
            Assert.That(game.Player.Position, Is.EqualTo(position));
            Assert.That(game.State.Seconds, Is.EqualTo(seconds));
            game.TogglePause();
            yield return new WaitForSeconds(0.2f);
            Assert.That(game.Player.Position.x, Is.GreaterThan(position.x));
            game.Restart();
            Assert.That(game.State.Coins, Is.Zero);
            Assert.That(game.State.Seconds, Is.Zero);
            Assert.That(game.Player.Position.x, Is.EqualTo(2));
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.Playing));
        }

        [UnityTest]
        public IEnumerator WalkingIntoBlockageLosesAndRetryRestoresBubbles()
        {
            game.PrimaryAction();
            keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return new WaitForSeconds(3);
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.Lost));
            Assert.That(game.State.Coins, Is.EqualTo(2));
            game.Restart();
            yield return new WaitForSeconds(1.5f);
            Assert.That(game.State.Coins, Is.EqualTo(2));
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.Playing));
        }

        [UnityTest]
        public IEnumerator SpaceAndTouchDownBothJumpWithoutMidAirDoubleJump()
        {
            keyboard = InputSystem.AddDevice<Keyboard>();
            game.PrimaryAction();
            yield return new WaitForSeconds(0.15f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
            yield return null;
            yield return null;
            yield return new WaitForSeconds(0.06f);
            Assert.That(game.Player.GetComponent<Rigidbody2D>().linearVelocity.y, Is.GreaterThan(7));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(0.15f);
            float before = game.Player.GetComponent<Rigidbody2D>().linearVelocity.y;
            var button = Object.FindAnyObjectByType<JumpButton>();
            button.OnPointerDown(new PointerEventData(EventSystem.current));
            yield return new WaitForFixedUpdate();
            Assert.That(game.Player.GetComponent<Rigidbody2D>().linearVelocity.y, Is.LessThan(before));
            yield return new WaitForSeconds(0.7f);
            button.OnPointerDown(new PointerEventData(EventSystem.current));
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(game.Player.GetComponent<Rigidbody2D>().linearVelocity.y, Is.GreaterThan(7));
        }

        [UnityTest]
        public IEnumerator GamepadStartsAndPauses()
        {
            gamepad = InputSystem.AddDevice<Gamepad>();
            InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.South));
            yield return null;
            yield return null;
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.Playing));
            InputSystem.QueueStateEvent(gamepad, new GamepadState().WithButton(GamepadButton.Start));
            yield return null;
            yield return null;
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.Paused));
        }

        [UnityTest]
        public IEnumerator NoAutoRunAndKeyboardCanStopAndReverse()
        {
            game.PrimaryAction();
            yield return new WaitForSeconds(0.3f);
            Assert.That(game.Player.Position.x, Is.EqualTo(2).Within(0.02f));
            keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return new WaitForSeconds(0.3f);
            Assert.That(game.Player.Position.x, Is.GreaterThan(3));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(0.08f);
            float stopped = game.Player.Position.x;
            yield return new WaitForSeconds(0.3f);
            Assert.That(game.Player.Position.x, Is.EqualTo(stopped).Within(0.02f));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A));
            yield return new WaitForSeconds(0.25f);
            Assert.That(game.Player.Position.x, Is.LessThan(stopped - 0.8f));
        }

        [UnityTest]
        public IEnumerator TouchCanMoveAndJumpTogetherAndDoesNotStickAfterPause()
        {
            game.PrimaryAction();
            yield return null;
            var directions = Object.FindObjectsByType<DirectionButton>();
            var right = System.Array.Find(directions, d => d.Direction > 0);
            var finger = new PointerEventData(EventSystem.current) { pointerId = 12 };
            right.OnPointerDown(finger);
            yield return new WaitForSeconds(0.2f);
            Object.FindAnyObjectByType<JumpButton>().OnPointerDown(new PointerEventData(EventSystem.current) { pointerId = 13 });
            yield return new WaitForSeconds(0.08f);
            Assert.That(game.Player.Position.x, Is.GreaterThan(2.8f));
            Assert.That(game.Player.GetComponent<Rigidbody2D>().linearVelocity.y, Is.GreaterThan(7));
            game.TogglePause();
            yield return null;
            Assert.That(game.Input.Horizontal, Is.Zero);
            game.TogglePause();
            yield return new WaitForSeconds(0.08f);
            float stopped = game.Player.Position.x;
            yield return new WaitForSeconds(0.2f);
            Assert.That(game.Player.Position.x, Is.EqualTo(stopped).Within(0.02f));
            right.OnPointerDown(finger);
            yield return new WaitForSeconds(0.15f);
            right.OnPointerExit(finger);
            Assert.That(game.Input.Horizontal, Is.Zero);
        }

        [UnityTest]
        public IEnumerator MapButtonsSelectCityAndReturningToMapStopsRun()
        {
            Assert.That(CityCatalog.All.Length, Is.EqualTo(6));
            var label = GameObject.Find("City label: gdansk").GetComponent<UnityEngine.UI.Button>();
            label.onClick.Invoke();
            Assert.That(game.CurrentCity.Id, Is.EqualTo("gdansk"));
            game.PrimaryAction();
            yield return new WaitForSeconds(0.1f);
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.Playing));
            Assert.That(game.CurrentCity.Id, Is.EqualTo("gdansk"));
            game.ReturnToMap();
            Vector2 stopped = game.Player.Position;
            yield return new WaitForSeconds(0.1f);
            Assert.That(game.State.Phase, Is.EqualTo(RunPhase.CityMap));
            Assert.That(game.Player.Position, Is.EqualTo(stopped));
            game.SelectCity(4);
            game.PrimaryAction();
            yield return null;
            Assert.That(game.CurrentCity.Id, Is.EqualTo("krakow"));
            Assert.That(game.State.Coins, Is.Zero);
            Assert.That(game.Player.Position.x, Is.EqualTo(2).Within(0.02f));
        }

        [UnityTest, Timeout(180000)]
        public IEnumerator AllSixCityRoutesCanBeCompletedWithRealPhysics()
        {
            keyboard = InputSystem.AddDevice<Keyboard>();
            Time.timeScale = 2;
            for (int city = 0; city < CityCatalog.All.Length; city++)
            {
                game.ReturnToMap();
                game.SelectCity(city);
                game.PrimaryAction();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
                float deadline = Time.realtimeSinceStartup + 25;
                while (game.State.Phase == RunPhase.Playing && Time.realtimeSinceStartup < deadline)
                {
                    float x = game.Player.Position.x;
                    bool jump = false;
                    foreach (float obstacle in game.Hazards)
                        if (obstacle - x > 0 && obstacle - x < 1.9f) jump = true;
                    foreach (Vector2 segment in SewerWorld.GroundSegments)
                        if (segment.y < SewerWorld.FinishX && segment.y - x > 0 && segment.y - x < 0.7f) jump = true;
                    if (jump && game.Player.Grounded) game.Player.QueueJump();
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(game.State.Phase, Is.EqualTo(RunPhase.Won), game.CurrentCity.Name + " stopped at x=" + game.Player.Position.x);
                Assert.That(game.State.Coins, Is.GreaterThan(10));
                Assert.That(PlayerPrefs.GetInt(game.CurrentCity.BestKey), Is.GreaterThanOrEqualTo(game.State.Coins));
            }
        }

        [Test]
        public void FinishedRunCannotAccumulatePointsOrResume()
        {
            var state = new RunState();
            state.Reset(); state.Start(); state.Collect(); state.Tick(2); state.Lose();
            state.Collect(); state.Tick(2); state.Pause(); state.Resume(); state.Win();
            Assert.That(state.Phase, Is.EqualTo(RunPhase.Lost));
            Assert.That(state.Coins, Is.EqualTo(1));
            Assert.That(state.Seconds, Is.EqualTo(2));
        }
    }
}
