using UnityEngine;

namespace Poopline
{
    public enum MenuPage { Home, Settings, Controls, NewGame }
    [DisallowMultipleComponent]
    public sealed class PooplineGame : MonoBehaviour
    {
        public const int LayoutVersion=9;
        public int layoutVersion;
        public Transform menuBackdrop;
        public static readonly Vector3 MenuFocus=new Vector3(-39,1,1);
        public SewerPlayer Player {get;private set;}
        public RunnerInput Input {get;private set;}
        public FlowVolume[] Flows {get;private set;}
        public SewerLadder[] Ladders {get;private set;}
        public RatPatrol[] Rats {get;private set;}
        public ElevatedWalkway[] Decks {get;private set;}
        public SewerLevel[] Levels {get;private set;}
        public Camera GameCamera {get;private set;}
        public SewerAudio Sound {get;private set;}
        public SewerSettings Settings {get;private set;}
        public SluicePuzzle Sluice {get;private set;}
        public int LevelIndex {get;private set;}
        public SewerLevel CurrentLevel=>Levels[LevelIndex];
        public int CheckpointIndex {get;private set;}=-1;
        public bool CheckpointReached=>CheckpointIndex>=0;
        public bool Paused {get;private set;}
        public bool Completed {get;private set;}
        public bool Transitioning {get;private set;}
        public bool MapOpen {get;private set;}
        public bool InMainMenu {get;private set;}=true;
        public MenuPage Page {get;private set;}
        public bool HasSession {get;private set;}
        public bool CanContinue=>savedGame!=null && !savedGame.completed;
        public string ContinueDescription=>!CanContinue?"Rozpocznij swoją podróż.":(savedGame.level+1).ToString("00")+" / "+Levels[savedGame.level].title+" · "+(savedGame.checkpoint<0?"wejście":"punkt "+(savedGame.checkpoint+1));
        public bool Frozen=>InMainMenu || Paused || Completed || Transitioning || MapOpen;
        public bool BackdropAnimating=>InMainMenu && Settings.MenuMotion;
        public ExplorationMap Map=>maps[LevelIndex];
        public string Notice {get;private set;}
        public float NoticeUntil {get;private set;}
        public float SavedUntil {get;private set;}
        public float Elapsed {get;private set;}
        public float LevelElapsed {get;private set;}
        public float[] StageTimes {get;private set;}
        public int Recoveries {get;private set;}
        public Vector3 Spawn=>CheckpointReached?CurrentLevel.checkpoints[CheckpointIndex]:CurrentLevel.start;
        public string Objective
        {
            get
            {
                if(LevelIndex==0 && Sluice!=null)
                {
                    if(Sluice.Solved) return Sluice.IsOpen?"Przejdź przez osuszoną komorę.":"Woda opada. Odpływ odsłania przejście.";
                    if(Sluice.Seen) return "Znajdź zawór przy starych pompach.";
                }
                return CurrentLevel.objectives[Mathf.Clamp(CheckpointIndex+1,0,CurrentLevel.objectives.Length-1)];
            }
        }
        private ExplorationMap[] maps;
        private JourneyRecord savedGame;
        private float protectedUntil,transitionRemaining,nextSave;
        private int previousFrameRate;
        private void Awake()
        {
            Input=new RunnerInput();Settings=new SewerSettings();
            Player=GetComponentInChildren<SewerPlayer>(true);Levels=GetComponentsInChildren<SewerLevel>(true);
            GameCamera=GetComponentInChildren<Camera>();Sound=GetComponent<SewerAudio>();
            previousFrameRate=Application.targetFrameRate;Application.targetFrameRate=60;
            if(Player==null || GameCamera==null || Levels.Length!=2 || menuBackdrop==null)
            {Debug.LogError("Open Tools > Poopline > Rebuild playable canal before Play.");enabled=false;return;}
            StageTimes=new float[Levels.Length];maps=new ExplorationMap[Levels.Length];
            for(int i=0;i<Levels.Length;i++) maps[i]=new ExplorationMap(Levels[i],i);
            Sluice=Levels[0].GetComponentInChildren<SluicePuzzle>(true);Sluice.Initialize(this);Sluice.Restore(false);
            Player.Initialize(this);GameCamera.GetComponent<SewerCamera>().Initialize(this);
            GetComponent<RunnerHud>().Initialize(this);GetComponent<SewerLighting>().Initialize(this);Sound.Initialize(this);
            savedGame=JourneySave.Load(Levels);
            ConfigureLevel(0,-1);
            ShowMainMenu();
        }
        private void Update()
        {
            if(InMainMenu)
            {
                if(Input.PausePressed) BackFromPanel();
                if(Input.ConfirmPressed && Page==MenuPage.Home) {if(CanContinue) ContinueJourney();else RequestNewGame();}
                return;
            }
            if(Input.PausePressed)
            {
                if(Page!=MenuPage.Home) BackFromPanel();
                else if(Completed) return;
                else if(MapOpen) ToggleMap();else TogglePause();
            }
            if(Page!=MenuPage.Home) return;
            if(Input.RetryPressed && !Completed && !Transitioning) {RetryCheckpoint();return;}
            if(Input.MapPressed && !Completed && !Transitioning) ToggleMap();
            if(Paused || Completed || MapOpen) return;
            if(Transitioning)
            {
                transitionRemaining-=Time.deltaTime;
                if(transitionRemaining<=0) {ConfigureLevel(LevelIndex+1,-1);SaveProgress(true);}
                return;
            }
            Elapsed+=Time.deltaTime;LevelElapsed+=Time.deltaTime;
            Vector3 p=Player.transform.position;Map.Discover(p);
            for(int i=CheckpointIndex+1;i<CurrentLevel.checkpoints.Length;i++)
            {
                if(Vector3.Distance(p,CurrentLevel.checkpoints[i])>1.5f) continue;
                CheckpointIndex=i;SaveProgress(true);ShowNotice("Punkt kontrolny · "+Objective,5);break;
            }
            if(Time.unscaledTime>=nextSave) {SaveProgress();nextSave=Time.unscaledTime+3;}
            if(!CurrentLevel.ContainsPlayer(p)) {Recover("Wracasz na ostatni bezpieczny pomost.");return;}
            if(!CurrentLevel.ContainsExit(p) || (LevelIndex==0 && !Sluice.IsOpen)) return;
            StageTimes[LevelIndex]=LevelElapsed;Player.StopMotion();
            if(LevelIndex+1<Levels.Length) {Transitioning=true;transitionRemaining=1.4f;SaveProgress();}
            else {Completed=true;SaveProgress(true);}
        }
        private void ConfigureLevel(int index,int checkpoint)
        {
            LevelIndex=index;CheckpointIndex=checkpoint;
            InMainMenu=false;menuBackdrop.gameObject.SetActive(false);Player.gameObject.SetActive(true);
            for(int i=0;i<Levels.Length;i++) Levels[i].gameObject.SetActive(i==index);
            Flows=CurrentLevel.GetComponentsInChildren<FlowVolume>();Ladders=CurrentLevel.GetComponentsInChildren<SewerLadder>();
            Decks=CurrentLevel.GetComponentsInChildren<ElevatedWalkway>();Rats=CurrentLevel.GetComponentsInChildren<RatPatrol>();
            foreach(var rat in Rats) {rat.Initialize(this);rat.ResetPatrol();}
            Paused=Completed=Transitioning=MapOpen=false;Page=MenuPage.Home;LevelElapsed=0;
            protectedUntil=Time.time+1.5f;Player.ResetAt(Spawn);Map.Discover(Spawn,true);
            GameCamera.GetComponent<SewerCamera>().Snap();ShowNotice(CurrentLevel.introduction,7);
        }
        public void RequestNewGame() {if(savedGame!=null) Page=MenuPage.NewGame;else StartNewGame();Sound.PlayClick();}
        public void StartNewGame()
        {
            JourneySave.Clear();foreach(var map in maps) map.Clear();
            Sluice.Restore(false);Elapsed=0;Recoveries=0;System.Array.Clear(StageTimes,0,StageTimes.Length);
            HasSession=true;ConfigureLevel(0,-1);SaveProgress(true);
        }
        public void ContinueJourney()
        {
            if(!CanContinue) return;
            JourneyRecord record=savedGame;
            Sluice.Restore(record.sluiceSolved,record.sluiceSeen);
            for(int i=0;i<maps.Length;i++) maps[i].Restore(record.maps[i]);
            HasSession=true;ConfigureLevel(record.level,record.checkpoint);
            Elapsed=record.elapsed;LevelElapsed=record.levelElapsed;StageTimes=(float[])record.stageTimes.Clone();
            ShowNotice("Wracasz do ostatniego punktu kontrolnego.",4);
        }
        private void ShowMainMenu()
        {
            InMainMenu=true;Page=MenuPage.Home;Paused=MapOpen=false;Player.StopMotion();
            Player.gameObject.SetActive(false);foreach(var level in Levels) level.gameObject.SetActive(false);
            menuBackdrop.gameObject.SetActive(true);GameCamera.GetComponent<SewerCamera>().Snap();
        }
        public void ReturnToMenu() {SaveProgress(true);Settings.Save();ShowMainMenu();Sound.PlayClick();}
        public void OpenPanel(MenuPage page) {Page=page;if(!InMainMenu) Paused=true;Sound.PlayClick();}
        public void BackFromPanel() {Settings.Save();Page=MenuPage.Home;Sound.PlayClick();}
        public void SaveProgress(bool notify=false)
        {
            if(!HasSession) return;
            var record=new JourneyRecord{level=LevelIndex,checkpoint=CheckpointIndex,sluiceSolved=Sluice.Solved,sluiceSeen=Sluice.Seen,completed=Completed,elapsed=Elapsed,levelElapsed=LevelElapsed,stageTimes=(float[])StageTimes.Clone(),maps=new string[maps.Length],savedAt=System.DateTime.UtcNow.ToString("o")};
            for(int i=0;i<maps.Length;i++) {maps[i].Save();record.maps[i]=maps[i].Export();}
            JourneySave.Write(record,Levels);savedGame=record;
            if(notify) SavedUntil=Time.unscaledTime+2.8f;
        }
        public void ToggleMap() {if(InMainMenu) return;MapOpen=!MapOpen;Player.StopMotion();if(MapOpen) SaveProgress();}
        public void TogglePause() {if(InMainMenu) return;Paused=!Paused;Player.StopMotion();if(Paused) SaveProgress();}
        public void RetryCheckpoint()
        {
            Paused=MapOpen=false;Page=MenuPage.Home;Player.ResetAt(Spawn);foreach(var rat in Rats) rat.ResetPatrol();
            protectedUntil=Time.time+2;GameCamera.GetComponent<SewerCamera>().Snap();ShowNotice("Wracasz do punktu kontrolnego.",3);
        }
        // Explicit stage restart retained for authoring/tests; normal R returns to the checkpoint.
        public void Restart() {if(Completed) {StartNewGame();return;}Elapsed-=LevelElapsed;StageTimes[LevelIndex]=0;ConfigureLevel(LevelIndex,-1);SaveProgress();}
        public void RestartJourney()=>StartNewGame();
        public void Recover(string message)
        {
            if(Frozen || Time.time<protectedUntil) return;
            Recoveries++;RetryCheckpoint();ShowNotice(message,4);
        }
        public void ShowNotice(string message,float duration) {Notice=message;NoticeUntil=Time.unscaledTime+duration;}
        public void QuitGame()
        {
            SaveProgress();Settings.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        private void OnApplicationFocus(bool focused)
        {if(!focused && Player!=null) {SaveProgress();if(!InMainMenu && !Paused && !Completed) TogglePause();}}
        private void OnApplicationPause(bool paused) {if(paused) OnApplicationFocus(false);}
        private void OnApplicationQuit() {SaveProgress();Settings?.Save();}
        private void OnDestroy() {SaveProgress();Settings?.Save();Input?.Dispose();Application.targetFrameRate=previousFrameRate;}
    }
}
