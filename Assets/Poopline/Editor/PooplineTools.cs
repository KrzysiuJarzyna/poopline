using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Poopline.Editor
{
    public static class PooplineTools
    {
        public const string ScenePath = "Assets/Poopline/Scenes/Poopline.unity";

        [MenuItem("Tools/Poopline/Open playable game", priority = 0)]
        public static void OpenGame()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        }

        [MenuItem("Tools/Poopline/Build macOS prototype", priority = 20)]
        public static void BuildMac()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ValidateProject();
            const string target = "Builds/macOS/Poopline.app";
            Directory.CreateDirectory("Builds/macOS");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = target,
                target = BuildTarget.StandaloneOSX, options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Poopline build failed: " + report.summary.result);
            Debug.Log("Poopline build ready: " + Path.GetFullPath(target));
        }

        // Can also be invoked with -executeMethod Poopline.Editor.PooplineTools.ValidateProject.
        public static void ValidateProject()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                throw new InvalidOperationException("Missing Poopline scene.");
            var scene = EditorSceneManager.OpenScene(ScenePath);
            int games = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                games += root.GetComponentsInChildren<PooplineGame>(true).Length;
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) != 0)
                        throw new InvalidOperationException("Missing script on " + child.name);
            }
            if (games != 1) throw new InvalidOperationException("Scene must have exactly one PooplineGame.");
            if (EditorBuildSettings.scenes.Length != 1 || EditorBuildSettings.scenes[0].path != ScenePath || !EditorBuildSettings.scenes[0].enabled)
                throw new InvalidOperationException("Build scene list must start with Poopline.");
            Debug.Log("POOPLINE_PROJECT_VALIDATED");
        }
    }
}
