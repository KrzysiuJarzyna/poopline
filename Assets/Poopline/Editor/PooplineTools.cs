using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Poopline.Editor
{
    public static class PooplineTools
    {
        public const string ScenePath = "Assets/Poopline/Scenes/Poopline.unity";
        [MenuItem("Tools/Poopline/Open playable canal",priority=0)]
        public static void OpenGame()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.playModeStartScene=null;
        }
        [MenuItem("Tools/Poopline/Rebuild playable canal",priority=10)]
        public static void Rebuild()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Directory.CreateDirectory("Assets/Poopline/Materials");
            AssetDatabase.Refresh();
            var materials=new Material[SewerWorld.MaterialNames.Length];
            for(int i=0;i<materials.Length;i++)
            {
                string path="Assets/Poopline/Materials/"+SewerWorld.MaterialNames[i]+".mat";
                var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m==null) { m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path); }
                m.SetColor("_BaseColor",SewerWorld.Colors[i]);m.SetFloat("_Smoothness",i==5?.72f:.23f);
                if(i==10)
                {m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",SewerWorld.Colors[i]*.8f);}
                else {m.DisableKeyword("_EMISSION");m.SetColor("_EmissionColor",Color.black);}
                EditorUtility.SetDirty(m);materials[i]=m;
            }
            var root=new GameObject("Poopline");root.AddComponent<PooplineGame>().layoutVersion=PooplineGame.LayoutVersion;
            var audio=root.AddComponent<SewerAudio>();
            audio.ambienceClip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Poopline/Audio/CanalBed.wav");
            audio.dripClip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Poopline/Audio/Drip.wav");
            audio.valveClip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Poopline/Audio/Valve.wav");
            audio.drainClip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Poopline/Audio/Drain.wav");
            audio.stepClip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Poopline/Audio/Step.wav");
            audio.clickClip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Poopline/Audio/Click.wav");
            root.AddComponent<RunnerHud>();root.AddComponent<SewerLighting>();SewerWorld.Build(root.transform,materials);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=Color.black;
            RenderSettings.ambientIntensity=0;
            RenderSettings.reflectionIntensity=0;
            RenderSettings.skybox=null;
            RenderSettings.fog=false;
            // The long channel meshes must receive the phone as well as all nearby lamps.
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Poopline/Settings/URP_ForwardRenderer.asset");
            renderer.renderingMode=RenderingMode.ForwardPlus;
            EditorUtility.SetDirty(renderer);
            foreach(var light in root.GetComponentsInChildren<Light>(true))
            {
                if(light.shadows==LightShadows.None) continue;
                var data=new SerializedObject(light.GetUniversalAdditionalLightData());
                data.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=light.type==LightType.Spot?2:0;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            EditorSceneManager.playModeStartScene=null;
            PlayerSettings.bundleVersion="0.6.2";
            AssetDatabase.SaveAssets();
            if(SceneView.lastActiveSceneView!=null)
            {
                SceneView.lastActiveSceneView.in2DMode=false;
                SceneView.lastActiveSceneView.LookAt(new Vector3(14,1,0),Quaternion.Euler(40,0,0),22,true);
                SceneView.lastActiveSceneView.Repaint();
            }
            Selection.activeGameObject=null;
            Debug.Log("POOPLINE_062_SCENE_READY");
        }
        [MenuItem("Tools/Poopline/Validate playable canal",priority=20)]
        public static void ValidateProject()
        {
            var game=UnityEngine.Object.FindAnyObjectByType<PooplineGame>();
            if(game==null || game.layoutVersion!=PooplineGame.LayoutVersion) throw new InvalidOperationException("Missing two-stage scene.");
            var stages=game.GetComponentsInChildren<SewerLevel>(true);
            if(stages.Length!=2 || game.GetComponentsInChildren<SewerPlayer>().Length!=1)
                throw new InvalidOperationException("Incomplete campaign.");
            if(stages[0].GetComponentsInChildren<RatPatrol>(true).Length!=0 || stages[1].GetComponentsInChildren<RatPatrol>(true).Length!=3 || stages[1].GetComponentsInChildren<SewerLadder>(true).Length!=4)
                throw new InvalidOperationException("Incorrect stage encounters.");
            var sluice=stages[0].GetComponent<SluicePuzzle>();
            var audio=game.GetComponent<SewerAudio>();
            if(game.menuBackdrop==null || sluice==null || sluice.valve==null || sluice.water==null || sluice.gate==null || sluice.discharge==null || sluice.barrier==null)
                throw new InvalidOperationException("Missing menu or sluice parts.");
            if(audio==null || audio.ambienceClip==null || audio.dripClip==null || audio.valveClip==null || audio.drainClip==null || audio.stepClip==null || audio.clickClip==null)
                throw new InvalidOperationException("Missing sound assets.");
            foreach(var stage in stages) if(stage.mapCells.Length<500 || stage.landmarks.Length<4) throw new InvalidOperationException("Missing exploration layout.");
            if(game.GetComponentsInChildren<TextMesh>(true).Length!=0)
                throw new InvalidOperationException("World-space labels are not allowed.");
            foreach(var child in game.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject)!=0) throw new InvalidOperationException("Missing script: "+child.name);
            Debug.Log("POOPLINE_062_VALIDATED");
        }
    }
}
