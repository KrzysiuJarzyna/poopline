using UnityEngine;
namespace Poopline
{
    public sealed class RunnerHud : MonoBehaviour
    {
        private PooplineGame game;
        private GUIStyle title, body, small, button, large;
        private readonly Color ink = new Color(.035f,.075f,.105f,.94f);
        private readonly Color cream = new Color(.92f,.94f,.88f);
        public void Initialize(PooplineGame owner) { game=owner; }
        private GUIStyle Style(int size, Color color, FontStyle weight = FontStyle.Normal)
        { return new GUIStyle(GUI.skin.label) { fontSize=size, fontStyle=weight, normal={textColor=color}, wordWrap=true }; }
        private void Panel(Rect rect, Color color) { Color old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old; }
        private void OnGUI()
        {
            if(game==null || game.Player==null) return;
            if(title==null)
            {
                title=Style(25,cream,FontStyle.Bold);body=Style(17,cream);small=Style(12,new Color(.55f,.73f,.72f));large=Style(38,cream,FontStyle.Bold);
                button=new GUIStyle(GUI.skin.button){fontSize=17,fixedHeight=44};
            }
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            Matrix4x4 old=GUI.matrix;GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            Panel(new Rect(24,24,316,91),ink);Panel(new Rect(24,24,4,91),new Color(.91f,.62f,.24f));
            GUI.Label(new Rect(43,32,280,34),"POOPLINE",title);
            GUI.Label(new Rect(44,72,280,23),"POD MIASTEM   /   "+(game.LevelIndex+1)+" Z 2",small);
            Panel(new Rect(882,24,374,91),ink);
            string objective=game.Completed?"Oba poziomy za tobą.":game.Objective;
            GUI.Label(new Rect(901,37,335,48),objective,body);
            GUI.Label(new Rect(901,84,335,23),(game.LevelIndex+1).ToString("00")+"  /  "+game.CurrentLevel.title.ToUpperInvariant(),small);
            Panel(new Rect(24,644,1232,52),ink);
            GUI.Label(new Rect(43,659,930,26),"WASD / STRZAŁKI  ruch i drabiny     E / M  mapa     ESC  pauza     R  restart",body);
            GUI.Label(new Rect(1075,662,165,25),game.Player.Climbing?"NA DRABINIE":game.Player.InWater?"W NURCIE":"SUCHY POMOST",small);
            if(!game.Frozen)
            {
                var ladder=game.Player.ActiveLadder!=null?game.Player.ActiveLadder:game.Player.NearbyLadder;
                string prompt="";
                if(ladder!=null)
                {
                    bool ascending=ladder.EntryDirection(game.Player.transform.position)>0;
                    prompt=game.Player.Climbing
                        ?ladder.DirectionHint(game.GameCamera,true)+"  w górę     "+ladder.DirectionHint(game.GameCamera,false)+"  w dół"
                        :ladder.DirectionHint(game.GameCamera,ascending)+(ascending?"   Wejdź po drabinie":"   Zejdź po drabinie");
                }
                else if(game.Player.InWater) prompt="Idź w stronę brzegu, żeby wyjść z nurtu.";
                if(prompt!="") {Panel(new Rect(376,569,528,46),ink);GUI.Label(new Rect(392,580,504,29),prompt,body);}
                if(Time.unscaledTime<game.NoticeUntil) {Panel(new Rect(340,126,600,52),ink);GUI.Label(new Rect(356,139,572,35),game.Notice,body);}
            }
            if(game.Paused || game.Completed)
            {
                Panel(new Rect(0,0,1280,720),new Color(.015f,.035f,.05f,.68f));Panel(new Rect(380,210,520,298),ink);
                GUI.Label(new Rect(414,238,455,55),game.Completed?"Wyszedłeś z kanałów.":"Chwila na oddech.",large);
                GUI.Label(new Rect(415,307,450,77),game.Completed?"Przeszedłeś oba etapy. Pod kamienicami i Żeliwne mosty są już za tobą.":"Rozgrywka jest zatrzymana. Woda i patrole też czekają.",body);
                if(!game.Completed && GUI.Button(new Rect(415,401,450,44),"WRACAMY",button)) game.TogglePause();
                if(GUI.Button(new Rect(415,453,450,38),game.Completed?"ZACZNIJ OBA ETAPY OD NOWA":"ZACZNIJ TEN ETAP OD NOWA",button)) game.Restart();
            }
            if(game.Transitioning && !game.Paused)
            {
                Panel(new Rect(0,0,1280,720),new Color(.008f,.018f,.025f,.98f));
                GUI.Label(new Rect(375,275,560,70),"Dalej pod miasto…",large);
                GUI.Label(new Rect(377,355,530,50),"02 / ŻELIWNE MOSTY",body);
            }
            if(game.MapOpen) ExplorationMapView.Draw(game);
            GUI.matrix=old;
        }
    }
}
