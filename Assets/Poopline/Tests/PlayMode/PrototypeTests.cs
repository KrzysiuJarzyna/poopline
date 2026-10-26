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
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorInput;
#endif
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale=1;
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
            if(game.Paused) game.TogglePause();
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
        }
        private void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); }
        private Key Direction(Vector3 v) => Mathf.Abs(v.x)>.5f?(v.x>0?Key.D:Key.A):(v.z>0?Key.W:Key.S);
        private IEnumerator WalkTo(Vector3 point,float tolerance=.18f)
        {
            int stage=game.LevelIndex;
            float deadline=Time.realtimeSinceStartup+65;
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
        // A fixture shortcut for isolated component tests. The campaign test below never uses it.
        private IEnumerator SecondStage()
        {
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
            foreach(float x in new[]{8f,35f,72f})
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
            game.Player.ResetAt(new Vector3(96,.05f,3.55f));
            yield return WalkTo(new Vector3(96,0,9));
            Assert.That(game.Player.InWater,Is.True);
            ScreenCapture.CaptureScreenshot("TestResults/side-channel.png");
            yield return new WaitForEndOfFrame();
            Assert.That(game.CurrentLevel.GetComponentsInChildren<WaterDrip>().Length,Is.GreaterThan(20));
            yield return WalkTo(new Vector3(96,0,18.45f));
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
                    Assert.That(game.Recoveries,Is.Zero,"Route waypoint "+point);
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
                    Assert.That(game.LevelIndex,Is.EqualTo(1));Assert.That(game.Rats.Length,Is.EqualTo(5));
                    Assert.That(game.CheckpointReached,Is.False);
                }
            }
            Assert.That(game.Completed,Is.True);
            Assert.That(game.StageTimes[0],Is.InRange(125,230));
            Assert.That(game.StageTimes[1],Is.GreaterThan(game.StageTimes[0]));
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
                if(light.transform.IsChildOf(game.CurrentLevel.transform) && Mathf.Abs(light.transform.position.x-25)<.1f && light.transform.position.z<5) farLamp=light;
            }
            Assert.That(farLamp,Is.Not.Null);Assert.That(farLamp.intensity,Is.Zero);
            Keys(Key.D);yield return new WaitForSeconds(.2f);Assert.That(phone.transform.forward.x,Is.GreaterThan(.7f));
            Keys(Key.A);yield return new WaitForSeconds(.35f);Keys();Assert.That(phone.transform.forward.x,Is.LessThan(-.7f));
            game.Player.ResetAt(new Vector3(25,.05f,3.55f));yield return new WaitForSeconds(.15f);
            Assert.That(farLamp.intensity,Is.GreaterThan(1));Assert.That(phone.transform.forward.y,Is.LessThan(-.2f));
            game.Restart();yield return new WaitForSeconds(.15f);Assert.That(farLamp.intensity,Is.Zero);
        }
        [UnityTest]
        public IEnumerator CameraFollowsSideChannelsAndFocusLossPauses()
        {
            Quaternion rotation=game.GameCamera.transform.rotation;
            Vector3 before=game.GameCamera.transform.position;
            game.Player.ResetAt(new Vector3(96,.05f,8));yield return new WaitForSeconds(.8f);
            Assert.That(Quaternion.Angle(rotation,game.GameCamera.transform.rotation),Is.LessThan(.01f));
            Assert.That(game.GameCamera.transform.position.z,Is.GreaterThan(before.z+5));
            game.SendMessage("OnApplicationFocus",false);Assert.That(game.Paused,Is.True);
            game.SendMessage("OnApplicationFocus",true);Assert.That(game.Paused,Is.True);
        }
        [UnityTest]
        public IEnumerator TunnelLintelRevealsThePlayerAndReturnsWhenLeaving()
        {
            game.Player.ResetAt(new Vector3(96,.05f,3.55f));
            yield return WalkTo(new Vector3(96,0,9));
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
            yield return WalkTo(new Vector3(96,0,3.55f));
            yield return new WaitForSeconds(.3f);
            Assert.That(lintel.Hidden,Is.False);
            Assert.That(lintel.GetComponent<Renderer>().shadowCastingMode,Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.On));
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
