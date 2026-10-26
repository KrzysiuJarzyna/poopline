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
            Directory.CreateDirectory("TestResults");
            Runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,assemblyNames=new[]{"Poopline.PlayModeTests"}}));
        }
        [MenuItem("Tools/Poopline/Run tunnel visibility checks",priority=40)]
        public static void RunVisibility()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=null;
            SessionState.SetBool("Poopline.VisibilityChecks",true);
            Directory.CreateDirectory("TestResults");
            Runner.Execute(new ExecutionSettings(new Filter{testMode=TestMode.PlayMode,assemblyNames=new[]{"Poopline.PlayModeTests"},testNames=new[]{
                "Poopline.Tests.PrototypeTests.TunnelLintelRevealsThePlayerAndReturnsWhenLeaving",
                "Poopline.Tests.PrototypeTests.SideTunnelConnectsChambersAndRecordsCheckpoint",
                "Poopline.Tests.PrototypeTests.PhoneFollowsFacingAndDistantChambersStayDark",
                "Poopline.Tests.PrototypeTests.CameraFollowsSideChannelsAndFocusLossPauses"}}));
        }
        private sealed class Results : ICallbacks
        {
            private static bool Visibility => SessionState.GetBool("Poopline.VisibilityChecks",false);
            private static string Progress => Visibility?"TestResults/0.4-visibility-progress.txt":"TestResults/0.4-progress.txt";
            public void RunStarted(ITestAdaptor tests) { File.WriteAllText(Progress,"RUNNING\n"); }
            public void TestStarted(ITestAdaptor test) {}
            public void TestFinished(ITestResultAdaptor result)
            { File.AppendAllText(Progress,result.Name+": "+result.ResultState+" "+result.Message+"\n"); }
            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result,Visibility?"TestResults/Visibility-0.4.xml":"TestResults/PlayMode-0.4.xml");
                File.AppendAllText(Progress,"FINISHED "+result.PassCount+" passed, "+result.FailCount+" failed\n");
                Debug.Log("POOPLINE_04_TESTS: "+result.PassCount+" passed, "+result.FailCount+" failed");
            }
        }
    }
}
