using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
namespace Poopline.Editor
{
    [InitializeOnLoad]
    public static class PooplineTestTools
    {
        private static readonly TestRunnerApi Runner;
        static PooplineTestTools()
        {
            Runner=ScriptableObject.CreateInstance<TestRunnerApi>();
            Runner.RegisterCallbacks(new Results());
        }
        [MenuItem("Tools/Poopline/Run PlayMode checks",priority=30)]
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=null;
            SessionState.SetBool("Poopline.VisibilityChecks",false);
            SessionState.SetBool("Poopline.InteractionChecks",false);
            Directory.CreateDirectory("TestResults");
            Runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,assemblyNames=new[]{"Poopline.PlayModeTests"}}));
        }
        [MenuItem("Tools/Poopline/Run tunnel visibility checks",priority=40)]
        public static void RunVisibility()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=null;
            SessionState.SetBool("Poopline.VisibilityChecks",true);
            SessionState.SetBool("Poopline.InteractionChecks",false);
            Directory.CreateDirectory("TestResults");
            Runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,assemblyNames=new[]{"Poopline.PlayModeTests"},testNames=new[]{
                "Poopline.Tests.PrototypeTests.TunnelLintelRevealsThePlayerAndReturnsWhenLeaving",
                "Poopline.Tests.PrototypeTests.SideTunnelConnectsChambersAndRecordsCheckpoint",
                "Poopline.Tests.PrototypeTests.PhoneFollowsFacingAndDistantChambersStayDark",
                "Poopline.Tests.PrototypeTests.CameraFollowsSideChannelsAndFocusLossPauses"}}));
        }
        [MenuItem("Tools/Poopline/Run interaction regression checks",priority=50)]
        public static void RunInteractions()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=null;
            SessionState.SetBool("Poopline.VisibilityChecks",false);
            SessionState.SetBool("Poopline.InteractionChecks",true);
            Directory.CreateDirectory("TestResults");
            Runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,assemblyNames=new[]{"Poopline.PlayModeTests"},testNames=new[]{
                "Poopline.Tests.PrototypeTests.EveryLadderWorksInBothDirectionsAndWaitsForRelease",
                "Poopline.Tests.PrototypeTests.FloodedGateBlocksExitAndValveNeedsReachAndUninterruptedHold",
                "Poopline.Tests.PrototypeTests.WaterDriftsButCanBeCrossedAndExitedOnBothBanks"}}));
        }
        private sealed class Results : ICallbacks
        {
            private static bool Visibility => SessionState.GetBool("Poopline.VisibilityChecks",false);
            private static bool Interaction => SessionState.GetBool("Poopline.InteractionChecks",false);
            private static string Progress => Interaction?"TestResults/0.6.2-interaction-progress.txt":Visibility?"TestResults/0.6.2-visibility-progress.txt":"TestResults/0.6.2-progress.txt";
            public void RunStarted(ITestAdaptor tests) { File.WriteAllText(Progress,"RUNNING\n"); }
            public void TestStarted(ITestAdaptor test) {}
            public void TestFinished(ITestResultAdaptor result)
            { File.AppendAllText(Progress,result.Name+": "+result.ResultState+" "+result.Message+"\n"); }
            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result,Interaction?"TestResults/Interaction-0.6.2.xml":Visibility?"TestResults/Visibility-0.6.2.xml":"TestResults/PlayMode-0.6.2.xml");
                File.AppendAllText(Progress,"FINISHED "+result.PassCount+" passed, "+result.FailCount+" failed\n");
                Debug.Log("POOPLINE_062_TESTS: "+result.PassCount+" passed, "+result.FailCount+" failed");
            }
        }
    }
}
