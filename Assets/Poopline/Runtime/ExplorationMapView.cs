using UnityEngine;

namespace Poopline
{
    public static class ExplorationMapView
    {
        private static GUIStyle heading, text, caption, marker, button;
        private static readonly Color Paper=new Color(.76f,.81f,.73f);
        private static readonly Color Amber=new Color(.96f,.66f,.30f);
        private static void Box(Rect rect,Color color)
        { Color old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old; }
        private static GUIStyle Label(int size,Color color,FontStyle weight=FontStyle.Normal)
        { return new GUIStyle(GUI.skin.label){fontSize=size,wordWrap=true,fontStyle=weight,normal={textColor=color}}; }
        public static void Draw(PooplineGame game)
        {
            if(heading==null)
            {
                heading=Label(30,Paper,FontStyle.Bold);text=Label(17,Paper);caption=Label(12,new Color(.52f,.66f,.66f));
                marker=Label(13,Paper,FontStyle.Bold);button=new GUIStyle(GUI.skin.button){fontSize=15};
            }
            Box(new Rect(0,0,1280,720),new Color(.008f,.018f,.027f,1));
            Box(new Rect(32,32,4,54),Amber);
            GUI.Label(new Rect(52,32,820,42),"ŚLADY POD MIASTEM",heading);
            GUI.Label(new Rect(54,77,820,24),(game.LevelIndex+1).ToString("00")+"  /  "+game.CurrentLevel.title.ToUpperInvariant(),caption);
            if(GUI.Button(new Rect(1020,38,214,42),"WRÓĆ   ·   E / M / ESC",button)) game.ToggleMap();
            Rect chart=new Rect(48,121,900,539);
            Box(chart,new Color(.025f,.055f,.065f));
            // A fixed scale per level keeps visited paths stable as the survey grows.
            Bounds bounds=game.CurrentLevel.playableBounds;
            float scale=Mathf.Min((chart.width-90)/bounds.size.x,(chart.height-70)/bounds.size.z);
            Vector2 center=chart.center;
            Vector2 MapPoint(Vector3 world) => new Vector2(center.x+(world.x-bounds.center.x)*scale,center.y-(world.z-bounds.center.z)*scale);
            var map=game.Map;
            var cells=game.CurrentLevel.mapCells;
            for(int i=0;i<cells.Length;i++)
            {
                if(!map.IsDiscovered(i)) continue;
                var p=MapPoint(new Vector3(cells[i].position.x,0,cells[i].position.y));
                bool bridge=false;
                foreach(var deck in game.Decks)
                {
                    Bounds b=deck.GetComponent<BoxCollider>().bounds;
                    if(cells[i].position.x>=b.min.x && cells[i].position.x<=b.max.x && cells[i].position.y>=b.min.z && cells[i].position.y<=b.max.z) bridge=true;
                }
                bool water=cells[i].water && !(game.LevelIndex==0 && game.Sluice.IsOpen && game.Sluice.chamber.Contains(new Vector3(cells[i].position.x,0,cells[i].position.y)));
                Box(new Rect(p.x-scale/2,p.y-scale/2,scale+.2f,scale+.2f),bridge?new Color(.64f,.65f,.51f):water?new Color(.12f,.43f,.46f):new Color(.36f,.45f,.43f));
                if(map.IsFrontier(i)) Box(new Rect(p.x-1.2f,p.y-1.2f,2.4f,2.4f),Amber);
            }
            foreach(var ladder in game.Ladders)
            {
                if(!map.IsDiscovered(ladder.Bottom)) continue;
                Vector2 p=MapPoint(ladder.Bottom);
                Box(new Rect(p.x-3,p.y-4,1,8),Amber);Box(new Rect(p.x+3,p.y-4,1,8),Amber);
                for(int n=-3;n<=3;n+=3) Box(new Rect(p.x-3,p.y+n,7,1),Amber);
            }
            foreach(var landmark in game.CurrentLevel.landmarks)
            {
                if(!map.IsDiscovered(landmark.position)) continue;
                Vector2 p=MapPoint(landmark.position);
                Box(new Rect(p.x-3,p.y-3,6,6),landmark.exit?new Color(.4f,1,.8f):Paper);
                GUI.Label(new Rect(p.x+8,p.y-11,170,32),landmark.name=="Zalana komora" && game.Sluice.IsOpen?"Osuszona komora":landmark.name,marker);
            }
            Vector2 player=MapPoint(game.Player.transform.position);
            Box(new Rect(player.x-7,player.y-7,14,14),new Color(.025f,.055f,.065f));
            Box(new Rect(player.x-4,player.y-4,8,8),Amber);
            Vector3 facing=game.Player.transform.Find("Human").forward;
            Vector2 tip=player+new Vector2(facing.x,-facing.z)*12;
            Box(new Rect(tip.x-2,tip.y-2,4,4),Amber);
            GUI.Label(new Rect(chart.x+18,chart.y+12,160,26),"↑  PÓŁNOC",caption);
            GUI.Label(new Rect(981,132,240,62),"Tyle już znasz.",heading);
            GUI.Label(new Rect(983,214,231,85),"Mapa dopisuje tylko miejsca w twoim zasięgu. Ciemne obszary czekają na odkrycie.",text);
            Box(new Rect(984,329,12,12),Amber);GUI.Label(new Rect(1007,322,215,32),"Twoja pozycja i kierunek",text);
            Box(new Rect(984,373,12,12),new Color(.12f,.43f,.46f));GUI.Label(new Rect(1007,366,215,30),"Woda",text);
            Box(new Rect(984,417,12,12),new Color(.36f,.45f,.43f));GUI.Label(new Rect(1007,410,215,30),"Odkryte przejścia",text);
            Box(new Rect(988,464,4,4),Amber);GUI.Label(new Rect(1007,452,215,58),"Kropki: tu droga biegnie dalej",text);
            GUI.Label(new Rect(983,535,242,70),"Odkrycia zapisują się automatycznie. Podczas czytania mapy gra czeka.",caption);
            if(GUI.Button(new Rect(982,613,250,36),"WYCZYŚĆ MAPĘ TEGO ETAPU",button))
            {map.Clear();map.Discover(game.Player.transform.position,true);game.SaveProgress();}
            GUI.Label(new Rect(50,679,1160,24),"Bez pośpiechu. Wracaj do poznanych miejsc i sprawdzaj nieodkryte odnogi.",caption);
        }
    }
}
