using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Poopline.Tests
{
    public sealed class PrototypeTests
    {
        private PooplineGame game;
        private Keyboard keyboard;
        private string previousSaveNamespace, testSaveNamespace;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorInput;
#endif
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale=1;
            previousSaveNamespace=ExplorationMap.SaveNamespace;
            testSaveNamespace="Poopline.Tests."+System.Guid.NewGuid();
            ExplorationMap.SaveNamespace=testSaveNamespace;
            background=InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard=InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Poopline");
            game=Object.FindAnyObjectByType<PooplineGame>();
            Assert.That(game,Is.Not.Null);Assert.That(game.Player,Is.Not.Null);
            Assert.That(game.InMainMenu,Is.True);
            game.StartNewGame();
            yield return new WaitForSeconds(.2f);
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale=1;
            InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior=background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
#endif
            if(game!=null) Object.Destroy(game.gameObject);
            yield return null;
            for(int i=0;i<2;i++) PlayerPrefs.DeleteKey(testSaveNamespace+".v"+PooplineGame.LayoutVersion+".level"+i);
            foreach(string suffix in new[]{".Journey",".Journey.backup",".Settings.volume",".Settings.ambience",".Settings.motion"}) PlayerPrefs.DeleteKey(testSaveNamespace+suffix);
            PlayerPrefs.Save();ExplorationMap.SaveNamespace=previousSaveNamespace;
        }
        private void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); }
        private Key Direction(Vector3 v) => Mathf.Abs(v.x)>.5f?(v.x>0?Key.D:Key.A):(v.z>0?Key.W:Key.S);
        private IEnumerator WalkTo(Vector3 point,float tolerance=.18f)
        {
            int stage=game.LevelIndex;
            float deadline=Time.realtimeSinceStartup+20;
            while(Time.realtimeSinceStartup<deadline)
            {
                if(game.Transitioning || game.Completed || stage!=game.LevelIndex) {Keys();yield return null;yield break;}
                Vector3 p=game.Player.transform.position;
                Vector2 delta=new Vector2(point.x-p.x,point.z-p.z);
                if(delta.magnitude<tolerance) {Keys();yield return null;yield break;}
                var keys=new List<Key>();
                if(Mathf.Abs(delta.x)>.09f) keys.Add(delta.x>0?Key.D:Key.A);
                if(Mathf.Abs(delta.y)>.09f) keys.Add(delta.y>0?Key.W:Key.S);
                Keys(keys.ToArray());yield return null;
            }
            Assert.Fail("Could not reach "+point+" from "+game.Player.transform.position+" stage "+game.LevelIndex);
        }
        private IEnumerator Climb(SewerLadder ladder,bool up)
        {
            Keys();yield return new WaitForSeconds(.55f);
            Assert.That(game.Player.NearbyLadder,Is.EqualTo(ladder),"Ladder entrance");
            Keys(Direction(ladder.FaceDirection*(up?1:-1)));
            yield return null;yield return null;
            Assert.That(game.Player.Climbing,Is.True);
            yield return new WaitForSeconds(2.3f);
            Keys();yield return new WaitForSeconds(.15f);
            Assert.That(game.Player.Climbing,Is.False);
            Assert.That(Vector3.Distance(game.Player.transform.position,up?ladder.TopLanding:ladder.BottomLanding),Is.LessThan(.18f));
        }
        private IEnumerator UseValveAtWaypoint(Vector3 point)
        {
            if(game.LevelIndex!=0 || game.Sluice.Solved || Vector3.Distance(point,new Vector3(30,0,14.5f))>.2f) yield break;
            Assert.That(game.Sluice.InReach,Is.True);
            Keys(Key.F);yield return new WaitForSeconds(1.7f);Keys();
            Assert.That(game.Sluice.Solved,Is.True);
            ScreenCapture.CaptureScreenshot("TestResults/sluice-valve.png");yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(4.7f);
            Assert.That(game.Sluice.IsOpen,Is.True);
        }
        // A fixture shortcut for isolated component tests. The campaign test below never uses it.
        private IEnumerator SecondStage()
        {
            game.Sluice.Restore(true);
            Vector3 exit=game.CurrentLevel.exit.center;exit.y=.05f;
            game.Player.ResetAt(exit);
            yield return new WaitForSeconds(1.6f);
            Assert.That(game.LevelIndex,Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator PlayStartsInFirstStageWithNoEnemiesAndSecondStageInactive()
        {
            Assert.That(game.Frozen,Is.False);Assert.That(game.Levels.Length,Is.EqualTo(2));
            Assert.That(game.LevelIndex,Is.Zero);Assert.That(game.Rats,Is.Empty);
            Assert.That(game.Levels[0].gameObject.activeSelf,Is.True);
            Assert.That(game.Levels[1].gameObject.activeSelf,Is.False);
            Vector3 start=game.Player.transform.position;
            yield return new WaitForSeconds(.3f);
            Assert.That(Vector3.Distance(start,game.Player.transform.position),Is.LessThan(.03f));
            Assert.That(game.GetComponentsInChildren<TextMesh>(true),Is.Empty,"Instructions belong in the HUD only");
        }
        [UnityTest]
        public IEnumerator DryMovementStopsAndReverses()
        {
            float start=game.Player.transform.position.x;
            Keys(Key.D);yield return new WaitForSeconds(.4f);
            float x=game.Player.transform.position.x;Assert.That(x,Is.GreaterThan(start+1));
            Keys();yield return new WaitForSeconds(.3f);Assert.That(game.Player.transform.position.x,Is.EqualTo(x).Within(.03f));
            Keys(Key.A);yield return new WaitForSeconds(.3f);Assert.That(game.Player.transform.position.x,Is.LessThan(x-.7f));
        }
        [UnityTest]
        public IEnumerator WaterDriftsButCanBeCrossedAndExitedOnBothBanks()
        {
            foreach(float x in new[]{8f,21f,28f})
            {
                game.Player.ResetAt(new Vector3(x,.05f,-2.75f));
                yield return WalkTo(new Vector3(x,0,0));Assert.That(game.Player.InWater,Is.True);
                yield return WalkTo(new Vector3(x,0,2.75f));Assert.That(game.Player.InWater,Is.False);
                Assert.That(game.Player.transform.position.y,Is.EqualTo(0).Within(.12f));
                yield return WalkTo(new Vector3(x,0,0));
                float before=game.Player.transform.position.x;Keys();yield return new WaitForSeconds(.5f);
                Assert.That(game.Player.transform.position.x,Is.GreaterThan(before+.3f));
                before=game.Player.transform.position.x;Keys(Key.A);yield return new WaitForSeconds(.5f);
                Assert.That(game.Player.transform.position.x,Is.LessThan(before-.4f));
                yield return WalkTo(new Vector3(x,0,-2.75f));Assert.That(game.Player.InWater,Is.False);
            }
            Assert.That(game.Recoveries,Is.Zero);
        }
        [UnityTest]
        public IEnumerator SideTunnelConnectsChambersAndRecordsCheckpoint()
        {
            game.Player.ResetAt(new Vector3(8,.05f,3.55f));
            yield return WalkTo(new Vector3(8,0,9));
            Assert.That(game.Player.InWater,Is.True);
            ScreenCapture.CaptureScreenshot("TestResults/side-channel.png");
            yield return new WaitForEndOfFrame();
            Assert.That(game.CurrentLevel.GetComponentsInChildren<WaterDrip>().Length,Is.GreaterThan(15));
            yield return WalkTo(new Vector3(8,0,15));
            Assert.That(game.CheckpointIndex,Is.Zero);
            Assert.That(game.Player.InWater,Is.False);
            Assert.That(game.Recoveries,Is.Zero);
        }
        [UnityTest,Timeout(600000)]
        public IEnumerator EntireCampaignTraversesBothLevelsWithRealMovementAndNoRecoveries()
        {
            // Double clock speed shortens automation, without changing player speed or physics rules.
            Time.timeScale=2;
            Directory.CreateDirectory("TestResults");
            for(int stage=0;stage<2;stage++)
            {
                Assert.That(game.LevelIndex,Is.EqualTo(stage));
                Vector3[] route=game.CurrentLevel.suggestedRoute;
                bool captured=false;
                foreach(Vector3 point in route)
                {
                    if(point.y>1 && game.Player.transform.position.y<1)
                    {
                        SewerLadder match=null;
                        foreach(var ladder in game.Ladders) if(Vector3.Distance(ladder.TopLanding,point)<.3f) match=ladder;
                        Assert.That(match,Is.Not.Null,"Ascending route waypoint "+point);
                        yield return WalkTo(match.BottomLanding);yield return Climb(match,true);
                    }
                    else if(point.y<1 && game.Player.transform.position.y>1)
                    {
                        SewerLadder match=null;
                        foreach(var ladder in game.Ladders) if(Vector3.Distance(ladder.BottomLanding,point)<.3f) match=ladder;
                        Assert.That(match,Is.Not.Null,"Descending route waypoint "+point);
                        yield return WalkTo(match.TopLanding);yield return Climb(match,false);
                    }
                    else yield return WalkTo(point);
                    yield return UseValveAtWaypoint(point);
                    Assert.That(game.Recoveries,Is.Zero,"Route waypoint "+point);
                    if(stage==0 && Mathf.Abs(point.x-4)<.1f && point.z>20)
                    {
                        yield return new WaitForSeconds(.3f);
                        ScreenCapture.CaptureScreenshot("TestResults/cistern.png");yield return new WaitForEndOfFrame();
                    }
                    if(stage==0 && Mathf.Abs(point.x-26)<.1f && point.z>30)
                    {
                        game.ToggleMap();
                        ScreenCapture.CaptureScreenshot("TestResults/maze-explored.png");yield return new WaitForEndOfFrame();
                        game.ToggleMap();
                    }
                    if(!captured && ((stage==0 && game.Player.InWater) || (stage==1 && game.Player.transform.position.y>2)))
                    {
                        ScreenCapture.CaptureScreenshot("TestResults/stage-"+(stage+1)+".png");captured=true;
                        yield return new WaitForEndOfFrame();
                    }
                }
                if(stage==0)
                {
                    Assert.That(game.Transitioning,Is.True);
                    yield return new WaitForSeconds(1.6f);
                    Assert.That(game.LevelIndex,Is.EqualTo(1));Assert.That(game.Rats.Length,Is.EqualTo(3));
                    Assert.That(game.CheckpointReached,Is.False);
                }
            }
            Assert.That(game.Completed,Is.True);
            Assert.That(game.StageTimes[0],Is.InRange(10,120));
            Assert.That(game.StageTimes[1],Is.InRange(10,150));
            File.WriteAllText("TestResults/stage-times.txt","Level 1: "+game.StageTimes[0].ToString("F1")+" seconds\nLevel 2: "+game.StageTimes[1].ToString("F1")+" seconds\nRecoveries: "+game.Recoveries+"\n");
        }
        [UnityTest,Timeout(120000)]
        public IEnumerator AllUpperDeckEdgesBlockCardinalAndDiagonalFalls()
        {
            yield return SecondStage();
            Key[][] directions={new[]{Key.W},new[]{Key.S},new[]{Key.A},new[]{Key.D},new[]{Key.W,Key.A},new[]{Key.W,Key.D},new[]{Key.S,Key.A},new[]{Key.S,Key.D}};
            foreach(var deck in game.Decks)
                foreach(var direction in directions)
                {
                    Bounds b=deck.GetComponent<BoxCollider>().bounds;
                    game.Player.ResetAt(new Vector3(b.center.x,b.max.y+.05f,b.center.z));
                    Keys();yield return new WaitForSeconds(.35f);
                    Keys(direction);yield return new WaitForSeconds(.8f);
                    Assert.That(game.Player.transform.position.y,Is.GreaterThan(2.45f),deck.name+" "+string.Join(",",direction));
                    Assert.That(game.Player.InWater,Is.False);
                }
            Assert.That(game.Recoveries,Is.Zero);
        }
        [UnityTest,Timeout(90000)]
        public IEnumerator EveryLadderWorksInBothDirectionsAndWaitsForRelease()
        {
            yield return SecondStage();
            foreach(var ladder in game.Ladders)
            {
                Keys();game.Player.ResetAt(ladder.BottomLanding);
                yield return new WaitForSeconds(.4f);
                yield return Climb(ladder,true);
                yield return Climb(ladder,false);
            }
            Assert.That(game.Recoveries,Is.Zero);
        }
        [UnityTest]
        public IEnumerator LadderCanStopReverseAndPause()
        {
            yield return SecondStage();var ladder=game.Ladders[0];
            game.Player.ResetAt(ladder.BottomLanding);yield return new WaitForSeconds(.4f);
            Keys(Key.W);yield return new WaitForSeconds(.7f);Assert.That(game.Player.Climbing,Is.True);
            Keys();yield return null;Vector3 middle=game.Player.transform.position;
            yield return new WaitForSeconds(.2f);Assert.That(Vector3.Distance(game.Player.transform.position,middle),Is.LessThan(.002f));
            game.TogglePause();Vector3 paused=game.Player.transform.position;
            Keys(Key.W);yield return new WaitForSeconds(.2f);Assert.That(game.Player.transform.position,Is.EqualTo(paused));
            game.TogglePause();Keys(Key.S);yield return new WaitForSeconds(1);
            Assert.That(game.Player.Climbing,Is.False);Assert.That(game.Player.transform.position.y,Is.EqualTo(0).Within(.15f));
        }
        [UnityTest]
        public IEnumerator SideLadderCatchesApproachBeforeTheEdge()
        {
            yield return SecondStage();var ladder=game.Ladders[1];
            game.Player.ResetAt(ladder.TopLanding+Vector3.left*2.2f);yield return new WaitForSeconds(.4f);
            Keys(Key.D);float deadline=Time.realtimeSinceStartup+2;
            while(!game.Player.Climbing && Time.realtimeSinceStartup<deadline)
            {Assert.That(game.Player.transform.position.y,Is.GreaterThan(2.45f));yield return null;}
            Assert.That(game.Player.Climbing,Is.True);
            Assert.That(ladder.DirectionHint(game.GameCamera,false),Is.EqualTo("D / →"));
            yield return new WaitForSeconds(2);Keys();yield return null;
            Assert.That(game.Player.Climbing,Is.False);
            Assert.That(Vector3.Distance(game.Player.transform.position,ladder.BottomLanding),Is.LessThan(.18f));
        }
        [UnityTest]
        public IEnumerator DiagonalDepartureDoesNotGrabTheLadderAgain()
        {
            yield return SecondStage();
            foreach(var ladder in game.Ladders)
            {
                Keys();game.Player.ResetAt(ladder.TopLanding);yield return new WaitForSeconds(.4f);
                yield return Climb(ladder,false);
                Vector3 start=game.Player.transform.position;
                Key forward=Direction(ladder.FaceDirection), side=Direction(ladder.transform.right);
                Keys(forward,side);yield return new WaitForSeconds(.8f);Keys();
                Assert.That(game.Player.Climbing,Is.False);
                Assert.That(game.Player.transform.position.y,Is.LessThan(.2f));
                Assert.That(Vector3.Distance(start,game.Player.transform.position),Is.GreaterThan(1));
            }
        }
        [UnityTest]
        public IEnumerator RatContactRestoresCheckpointWithinCurrentLevel()
        {
            yield return SecondStage();
            game.Player.ResetAt(game.CurrentLevel.checkpoints[0]);yield return new WaitForSeconds(.2f);
            Assert.That(game.CheckpointIndex,Is.Zero);
            yield return new WaitForSeconds(1.5f);
            game.Player.ResetAt(game.Rats[1].transform.position);
            yield return new WaitForSeconds(.15f);
            Assert.That(game.Recoveries,Is.EqualTo(1));Assert.That(game.LevelIndex,Is.EqualTo(1));
            Assert.That(Vector3.Distance(game.Player.transform.position,game.Spawn),Is.LessThan(.15f));
        }
        [UnityTest]
        public IEnumerator PauseFreezesWaterPatrolAndClockAndRetryStaysInSecondStage()
        {
            yield return SecondStage();
            game.Player.ResetAt(new Vector3(8,-.7f,0));yield return new WaitForSeconds(.2f);
            Assert.That(game.Player.InWater,Is.True);
            game.TogglePause();Vector3 p=game.Player.transform.position,rat=game.Rats[0].transform.position;float seconds=game.Elapsed;
            var drop=game.CurrentLevel.GetComponentInChildren<WaterDrip>();Vector3 water=drop.transform.position;
            Keys(Key.D);yield return new WaitForSeconds(.4f);
            Assert.That(game.Player.transform.position,Is.EqualTo(p));Assert.That(game.Rats[0].transform.position,Is.EqualTo(rat));
            Assert.That(drop.transform.position,Is.EqualTo(water));Assert.That(game.Elapsed,Is.EqualTo(seconds));
            Keys();game.Restart();yield return new WaitForSeconds(.2f);
            Assert.That(game.LevelIndex,Is.EqualTo(1));Assert.That(game.Frozen||game.CheckpointReached||game.Player.InWater,Is.False);
        }
        [UnityTest]
        public IEnumerator TransitionFreezesInputAndFinalExitCompletesOnlySecondStage()
        {
            game.Sluice.Restore(true);
            Vector3 exit=game.CurrentLevel.exit.center;exit.y=.05f;game.Player.ResetAt(exit);
            yield return null;yield return null;Assert.That(game.Transitioning,Is.True);Assert.That(game.Completed,Is.False);
            Vector3 position=game.Player.transform.position;Keys(Key.D);yield return new WaitForSeconds(.3f);
            Assert.That(game.Player.transform.position,Is.EqualTo(position));
            game.TogglePause();yield return new WaitForSeconds(1.5f);Assert.That(game.LevelIndex,Is.Zero);
            Keys();game.TogglePause();yield return new WaitForSeconds(1.3f);Assert.That(game.LevelIndex,Is.EqualTo(1));
            exit=game.CurrentLevel.exit.center;exit.y=.05f;game.Player.ResetAt(exit);yield return null;yield return null;
            Assert.That(game.Completed,Is.True);
            game.Restart();yield return null;Assert.That(game.LevelIndex,Is.Zero);Assert.That(game.Completed,Is.False);
            Assert.That(game.Levels[1].gameObject.activeSelf,Is.False);
        }
        [UnityTest]
        public IEnumerator PhoneFollowsFacingAndDistantChambersStayDark()
        {
            var phone=game.Player.transform.Find("Human/Left arm/Phone light").GetComponent<Light>();
            Assert.That(RenderSettings.ambientIntensity,Is.Zero);Assert.That(RenderSettings.reflectionIntensity,Is.Zero);
            Light farLamp=null;
            foreach(var light in game.GetComponentsInChildren<Light>(true))
            {
                Assert.That(light.type,Is.Not.EqualTo(LightType.Directional));
                if(light.transform.IsChildOf(game.CurrentLevel.transform) && Mathf.Abs(light.transform.position.x-25)<.1f && light.transform.position.z>25) farLamp=light;
            }
            Assert.That(farLamp,Is.Not.Null);Assert.That(farLamp.intensity,Is.Zero);
            Keys(Key.D);yield return new WaitForSeconds(.2f);Assert.That(phone.transform.forward.x,Is.GreaterThan(.7f));
            Keys(Key.A);yield return new WaitForSeconds(.35f);Keys();Assert.That(phone.transform.forward.x,Is.LessThan(-.7f));
            game.Player.ResetAt(new Vector3(25,.05f,28));yield return new WaitForSeconds(.15f);
            Assert.That(farLamp.intensity,Is.GreaterThan(1));Assert.That(phone.transform.forward.y,Is.LessThan(-.2f));
            game.Restart();yield return new WaitForSeconds(.15f);Assert.That(farLamp.intensity,Is.Zero);
        }
        [UnityTest]
        public IEnumerator CameraFollowsSideChannelsAndFocusLossPauses()
        {
            Quaternion rotation=game.GameCamera.transform.rotation;
            Vector3 before=game.GameCamera.transform.position;
            game.Player.ResetAt(new Vector3(8,.05f,8));yield return new WaitForSeconds(.8f);
            Assert.That(Quaternion.Angle(rotation,game.GameCamera.transform.rotation),Is.LessThan(.01f));
            Assert.That(game.GameCamera.transform.position.z,Is.GreaterThan(before.z+5));
            game.SendMessage("OnApplicationFocus",false);Assert.That(game.Paused,Is.True);
            game.SendMessage("OnApplicationFocus",true);Assert.That(game.Paused,Is.True);
        }
        [UnityTest]
        public IEnumerator TunnelLintelRevealsThePlayerAndReturnsWhenLeaving()
        {
            game.Player.ResetAt(new Vector3(8,.05f,3.55f));
            yield return WalkTo(new Vector3(8,0,9));
            yield return new WaitForSeconds(.25f);
            Vector3 from=game.GameCamera.transform.position;
            Vector3 delta=game.Player.transform.position+Vector3.up*.9f-from;
            SewerCutaway lintel=null;
            foreach(var hit in Physics.RaycastAll(from,delta.normalized,delta.magnitude))
            {
                if(hit.collider.name!="Portal lintel") continue;
                lintel=hit.collider.GetComponent<SewerCutaway>();
                Assert.That(lintel,Is.Not.Null);Assert.That(lintel.Hidden,Is.True);
                Assert.That(hit.collider.GetComponent<Renderer>().shadowCastingMode,Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly));
            }
            Assert.That(lintel,Is.Not.Null,"This camera angle must exercise the actual doorway obstruction");
            ScreenCapture.CaptureScreenshot("TestResults/side-channel.png");yield return new WaitForEndOfFrame();
            yield return WalkTo(new Vector3(8,0,3.55f));
            yield return new WaitForSeconds(.3f);
            Assert.That(lintel.Hidden,Is.False);
            Assert.That(lintel.GetComponent<Renderer>().shadowCastingMode,Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.On));
        }
        [UnityTest,Timeout(180000)]
        public IEnumerator AlternateRoutesAlsoReachBothExitsWithoutRecoveries()
        {
            Time.timeScale=2;
            for(int stage=0;stage<2;stage++)
            {
                foreach(var point in game.CurrentLevel.alternateRoute) {yield return WalkTo(point);yield return UseValveAtWaypoint(point);}
                Assert.That(game.Recoveries,Is.Zero);
                if(stage==0) { Assert.That(game.Transitioning,Is.True);yield return new WaitForSeconds(1.6f); }
            }
            Assert.That(game.Completed,Is.True);
        }
        [UnityTest]
        public IEnumerator MapKeysPauseExplorationAndEscapeReturnsToTheGame()
        {
            yield return SecondStage();
            game.Player.ResetAt(new Vector3(22,-.6f,0));yield return new WaitForSeconds(.3f);
            Assert.That(game.Player.InWater,Is.True);
            Keys(Key.E);yield return null;yield return null;Keys();yield return null;
            Assert.That(game.MapOpen,Is.True);Assert.That(game.Frozen,Is.True);
            Vector3 player=game.Player.transform.position,rat=game.Rats[0].transform.position;
            float clock=game.Elapsed;
            Keys(Key.D);yield return new WaitForSeconds(.4f);Keys();yield return null;
            Assert.That(game.Player.transform.position,Is.EqualTo(player));Assert.That(game.Rats[0].transform.position,Is.EqualTo(rat));
            Assert.That(game.Elapsed,Is.EqualTo(clock));
            Keys(Key.M);yield return null;yield return null;Keys();yield return null;
            Assert.That(game.MapOpen,Is.False);Assert.That(game.Frozen,Is.False);
            Keys(Key.M);yield return null;yield return null;Keys();yield return null;
            Assert.That(game.MapOpen,Is.True);
            Keys(Key.Escape);yield return null;yield return null;Keys();yield return null;
            Assert.That(game.MapOpen||game.Paused,Is.False);
            game.TogglePause();game.ToggleMap();game.ToggleMap();Assert.That(game.Paused,Is.True,"Closing map preserves explicit pause");
        }
        [UnityTest]
        public IEnumerator ExplorationHidesUnknownRoomsAndSurvivesRestartAndReload()
        {
            Assert.That(game.Map.Count,Is.InRange(1,70));
            Assert.That(game.Map.HasCell(new Vector2Int(8,19)),Is.False,"Solid cistern is not an unexplored path");
            Assert.That(game.Map.IsDiscovered(game.CurrentLevel.exit.center),Is.False);
            Assert.That(game.Map.IsDiscovered(new Vector3(8,0,19)),Is.False);
            // Two nearby points on opposite sides of the masonry between the two branches.
            game.Player.ResetAt(new Vector3(12,.05f,15));yield return new WaitForSeconds(.3f);
            Assert.That(game.Map.IsDiscovered(new Vector3(15.5f,0,16.5f)),Is.False,"A diagonal cannot survey through the solid room corner");
            Vector3 visited=game.Player.transform.position;Assert.That(game.Map.IsDiscovered(visited),Is.True);
            int count=game.Map.Count;game.Restart();yield return new WaitForSeconds(.3f);
            Assert.That(game.Map.Count,Is.GreaterThanOrEqualTo(count));Assert.That(game.Map.IsDiscovered(visited),Is.True);
            game.Map.Save();
            var loaded=new ExplorationMap(game.CurrentLevel,0);
            Assert.That(loaded.Export(),Is.EqualTo(game.Map.Export()));
            yield return SceneManager.LoadSceneAsync("Poopline");
            game=Object.FindAnyObjectByType<PooplineGame>();
            game.ContinueJourney();
            if(game.Paused) game.TogglePause();
            Assert.That(game.Map.IsDiscovered(visited),Is.True,"Reload restores the saved survey");
            Assert.That(game.Map.Count,Is.GreaterThanOrEqualTo(count));
            yield return SecondStage();Assert.That(game.Map.IsDiscovered(visited),Is.False,"Each stage has its own survey");
        }
        [UnityTest]
        public IEnumerator MapShowsOnlyDiscoveredLandmarksAndCanStartFresh()
        {
            game.Player.ResetAt(new Vector3(4,.05f,19));yield return new WaitForSeconds(.4f);
            Assert.That(game.Map.IsDiscovered(game.CurrentLevel.landmarks[0].position),Is.True);
            game.ToggleMap();
            ScreenCapture.CaptureScreenshot("TestResults/exploration-map.png");yield return new WaitForEndOfFrame();
            game.Map.Clear();Assert.That(game.Map.Count,Is.Zero);
            game.Map.Restore("invalid record!");Assert.That(game.Map.Count,Is.Zero);
            game.Map.Restore(System.Convert.ToBase64String(new byte[]{1}));Assert.That(game.Map.Count,Is.Zero);
            game.Map.Discover(game.Player.transform.position,true);Assert.That(game.Map.Count,Is.GreaterThan(0));
            Assert.That(game.Map.IsDiscovered(game.CurrentLevel.exit.center),Is.False);
        }
        [UnityTest]
        public IEnumerator MenuFreezesCampaignButAnimatesTheNoirBackground()
        {
            game.ReturnToMenu();yield return null;
            Assert.That(game.InMainMenu && game.Frozen,Is.True);
            Assert.That(game.Player.gameObject.activeSelf,Is.False);
            Assert.That(game.Levels[0].gameObject.activeSelf || game.Levels[1].gameObject.activeSelf,Is.False);
            var drop=game.menuBackdrop.GetComponentInChildren<WaterDrip>();
            Vector3 before=drop.transform.position;float elapsed=game.Elapsed;
            Keys(Key.D);yield return new WaitForSeconds(.3f);Keys();
            Assert.That(drop.transform.position,Is.Not.EqualTo(before));Assert.That(game.Elapsed,Is.EqualTo(elapsed));
            ScreenCapture.CaptureScreenshot("TestResults/menu-noir.png");yield return new WaitForEndOfFrame();
            game.OpenPanel(MenuPage.Settings);game.Settings.MenuMotion=false;yield return new WaitForEndOfFrame();
            before=drop.transform.position;Vector3 camera=game.GameCamera.transform.position;
            yield return new WaitForSeconds(.3f);
            Assert.That(drop.transform.position,Is.EqualTo(before));Assert.That(game.GameCamera.transform.position,Is.EqualTo(camera));
            game.BackFromPanel();game.ContinueJourney();yield return null;
            Assert.That(game.InMainMenu || game.Frozen,Is.False);
        }
        [UnityTest]
        public IEnumerator CampaignCheckpointMapAndSluiceSurviveSceneReload()
        {
            yield return SecondStage();
            game.Player.ResetAt(game.CurrentLevel.checkpoints[0]);yield return new WaitForSeconds(.3f);
            Assert.That(game.CheckpointIndex,Is.Zero);
            var visited=new Vector3(30,0,17);game.Player.ResetAt(visited);yield return new WaitForSeconds(.3f);
            game.ReturnToMenu();
            yield return SceneManager.LoadSceneAsync("Poopline");game=Object.FindAnyObjectByType<PooplineGame>();
            Assert.That(game.InMainMenu && game.CanContinue,Is.True);
            game.ContinueJourney();yield return null;
            Assert.That(game.LevelIndex,Is.EqualTo(1));Assert.That(game.CheckpointIndex,Is.Zero);
            Assert.That(game.Sluice.IsOpen,Is.True);Assert.That(game.Map.IsDiscovered(visited),Is.True);
            Assert.That(Vector3.Distance(game.Player.transform.position,game.CurrentLevel.checkpoints[0]),Is.LessThan(.15f));
            game.Player.ResetAt(new Vector3(26,0,9));yield return null;
            Keys(Key.R);yield return null;yield return null;Keys();
            Assert.That(Vector3.Distance(game.Player.transform.position,game.Spawn),Is.LessThan(.15f));
        }
        [UnityTest]
        public IEnumerator NewGameRequiresConfirmationAndResetsJourneyButKeepsSettings()
        {
            yield return SecondStage();game.Map.Discover(new Vector3(36,0,32),true);
            game.Settings.Volume=.23f;game.ReturnToMenu();game.RequestNewGame();
            Assert.That(game.Page,Is.EqualTo(MenuPage.NewGame));Assert.That(game.LevelIndex,Is.EqualTo(1));
            game.BackFromPanel();Assert.That(game.CanContinue,Is.True);
            game.RequestNewGame();game.StartNewGame();yield return null;
            Assert.That(game.LevelIndex,Is.Zero);Assert.That(game.CheckpointIndex,Is.EqualTo(-1));
            Assert.That(game.Sluice.Solved,Is.False);Assert.That(game.Sluice.barrier.enabled,Is.True);
            Assert.That(game.Map.Count,Is.LessThan(70));Assert.That(game.Settings.Volume,Is.EqualTo(.23f));
            var save=JourneySave.Load(game.Levels);Assert.That(save.level,Is.Zero);
            Assert.That(System.Convert.FromBase64String(save.maps[1]),Is.All.EqualTo(0));
        }
        [UnityTest]
        public IEnumerator SettingsMuteAudioAndPersistIndependentlyOfTheJourney()
        {
            game.OpenPanel(MenuPage.Settings);game.Settings.Volume=0;game.Settings.Ambience=.31f;game.Settings.MenuMotion=false;
            yield return null;yield return null;
            foreach(var source in game.GetComponentsInChildren<AudioSource>()) Assert.That(source.volume,Is.Zero);
            game.BackFromPanel();var settings=new SewerSettings();
            Assert.That(settings.Volume,Is.Zero);Assert.That(settings.Ambience,Is.EqualTo(.31f));Assert.That(settings.MenuMotion,Is.False);
            game.Settings.Volume=.7f;game.Settings.Ambience=0;yield return null;yield return null;
            Assert.That(game.Sound.AmbienceSource.volume,Is.Zero);Assert.That(game.Sound.EffectsSource.volume,Is.GreaterThan(0));
            Assert.That(game.Sound.AmbienceSource.isPlaying,Is.True);
            foreach(var clip in new[]{game.Sound.ambienceClip,game.Sound.dripClip,game.Sound.valveClip,game.Sound.drainClip,game.Sound.stepClip,game.Sound.clickClip})
            {Assert.That(clip,Is.Not.Null);Assert.That(clip.samples,Is.GreaterThan(1000));Assert.That(clip.loadState,Is.EqualTo(AudioDataLoadState.Loaded));}
        }
        [UnityTest]
        public IEnumerator CorruptOrIncompatibleSaveCannotBeContinuedAndBackupRecovers()
        {
            game.SaveProgress();game.SaveProgress();
            PlayerPrefs.SetString(JourneySave.Key,"{broken json");
            Assert.That(JourneySave.Load(game.Levels),Is.Not.Null,"Last complete backup should recover");
            PlayerPrefs.DeleteKey(JourneySave.Key+".backup");Assert.That(JourneySave.Load(game.Levels),Is.Null);
            var invalid=new JourneyRecord{layout=-10};
            PlayerPrefs.SetString(JourneySave.Key,JsonUtility.ToJson(invalid));Assert.That(JourneySave.Load(game.Levels),Is.Null);
            // Dispose the active session before clearing, so OnDestroy cannot overwrite the fixture.
            Object.Destroy(game.gameObject);yield return null;JourneySave.Clear();
            yield return SceneManager.LoadSceneAsync("Poopline");game=Object.FindAnyObjectByType<PooplineGame>();
            Assert.That(game.InMainMenu,Is.True);Assert.That(game.CanContinue,Is.False);
            game.RequestNewGame();Assert.That(game.InMainMenu,Is.False,"No confirmation for an empty save slot");
        }
        [UnityTest]
        public IEnumerator FloodedGateBlocksExitAndValveNeedsReachAndUninterruptedHold()
        {
            Keys(Key.F);yield return new WaitForSeconds(1.7f);Keys();Assert.That(game.Sluice.Solved,Is.False);
            game.Player.ResetAt(new Vector3(26,0,32));yield return new WaitForSeconds(.2f);
            Assert.That(game.Sluice.Seen,Is.True);
            Keys(Key.D);yield return new WaitForSeconds(1);Keys();yield return null;
            Assert.That(game.Player.transform.position.x,Is.LessThan(28));
            ScreenCapture.CaptureScreenshot("TestResults/sluice-flooded.png");yield return new WaitForEndOfFrame();
            game.Player.ResetAt(game.CurrentLevel.exit.center);yield return new WaitForSeconds(.2f);
            Assert.That(game.Transitioning,Is.False,"Exit stays locked even if a fixture bypasses the gate");
            game.Player.ResetAt(new Vector3(30,0,14.5f));yield return null;
            Keys(Key.F);yield return new WaitForSeconds(.5f);Keys();yield return new WaitForSeconds(.4f);
            Assert.That(game.Sluice.HoldProgress,Is.Zero);Assert.That(game.Sluice.Solved,Is.False);
            Keys(Key.F);yield return new WaitForSeconds(.4f);game.ToggleMap();float hold=game.Sluice.HoldProgress;
            yield return new WaitForSeconds(.5f);Assert.That(game.Sluice.HoldProgress,Is.EqualTo(hold));
            game.ToggleMap();yield return new WaitForSeconds(1.3f);Keys();
            Assert.That(game.Sluice.Solved,Is.True);
            Assert.That(game.Sound.EffectsSource.isPlaying,Is.True,"Drain audio is running");
            game.TogglePause();float drain=game.Sluice.DrainProgress;Vector3 water=game.Sluice.water.position;
            yield return new WaitForSeconds(.5f);Assert.That(game.Sound.EffectsSource.isPlaying,Is.False,"Pause also stops interaction audio");Assert.That(game.Sluice.DrainProgress,Is.EqualTo(drain));Assert.That(game.Sluice.water.position,Is.EqualTo(water));
            game.TogglePause();yield return new WaitForSeconds(4.7f);
            Assert.That(game.Sluice.IsOpen,Is.True);Assert.That(game.Sluice.barrier.enabled,Is.False);
            Assert.That(game.Sluice.water.position.y,Is.LessThan(0));Assert.That(game.Sluice.gate.position.y,Is.GreaterThan(3));
            game.Player.ResetAt(new Vector3(26,0,32));yield return WalkTo(new Vector3(34,0,32));
            ScreenCapture.CaptureScreenshot("TestResults/sluice-drained.png");yield return new WaitForEndOfFrame();
            yield return WalkTo(new Vector3(37.6f,0,34));Assert.That(game.Transitioning,Is.True);
        }
        [UnityTest]
        public IEnumerator EnterCannotOverwriteCompletedJourneyWithoutConfirmation()
        {
            yield return SecondStage();var exit=game.CurrentLevel.exit.center;exit.y=0;
            game.Player.ResetAt(exit);yield return null;yield return null;
            Assert.That(game.Completed,Is.True);game.RequestNewGame();
            Assert.That(game.Page,Is.EqualTo(MenuPage.NewGame));
            Keys(Key.Escape);yield return null;yield return null;Keys();yield return null;
            Assert.That(game.Page,Is.EqualTo(MenuPage.Home));Assert.That(game.Completed,Is.True);
            game.ReturnToMenu();Assert.That(game.CanContinue,Is.False);
            Keys(Key.Enter);yield return null;yield return null;Keys();yield return null;
            Assert.That(game.InMainMenu,Is.True);Assert.That(game.Page,Is.EqualTo(MenuPage.NewGame));
            Assert.That(JourneySave.Load(game.Levels).completed,Is.True);
        }
        [UnityTest]
        public IEnumerator SavingDuringDrainRestoresOpenPassageAndRetryKeepsItSolved()
        {
            game.Player.ResetAt(new Vector3(30,0,14.5f));yield return null;
            Keys(Key.F);yield return new WaitForSeconds(1.7f);Keys();
            Assert.That(game.Sluice.Solved && !game.Sluice.IsOpen,Is.True);
            game.ReturnToMenu();
            yield return SceneManager.LoadSceneAsync("Poopline");game=Object.FindAnyObjectByType<PooplineGame>();
            game.ContinueJourney();yield return null;
            Assert.That(game.Sluice.IsOpen,Is.True);Assert.That(game.Sluice.barrier.enabled,Is.False);
            game.RetryCheckpoint();Assert.That(game.Sluice.IsOpen,Is.True);
        }
        [Test]
        public void CurrentIsLocalAndDoesNotReachUpperWalkway()
        {
            var go=new GameObject("Rotated flow");
            try
            {
                var flow=go.AddComponent<FlowVolume>();go.transform.rotation=Quaternion.Euler(0,90,0);
                Assert.That(flow.Current.x,Is.EqualTo(0).Within(.001f));Assert.That(flow.Current.z,Is.LessThan(-.8f));
                Assert.That(flow.ContainsFeet(Vector3.zero),Is.True);Assert.That(flow.ContainsFeet(new Vector3(0,2.6f,0)),Is.False);
            }
            finally {Object.DestroyImmediate(go);}
        }
    }
}
