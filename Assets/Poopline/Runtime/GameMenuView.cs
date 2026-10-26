using UnityEngine;

namespace Poopline
{
    // The background is a real, animated piece of sewer. Only the menu is drawn here.
    public static class GameMenuView
    {
        private static GUIStyle heading, label, hint, item;
        private static readonly Color Cream=new Color(.91f,.91f,.84f);
        private static readonly Color Copper=new Color(.78f,.54f,.3f);
        private static GUIStyle Text(int size,Color color,FontStyle weight=FontStyle.Normal)
            =>new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=weight,wordWrap=true,normal={textColor=color}};
        private static void Fill(Rect r,Color color)
        {var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        private static bool Button(float y,string text,bool enabled=true)
        {
            var r=new Rect(78,y,390,47);bool hover=r.Contains(Event.current.mousePosition)&&enabled;
            Fill(r,hover?new Color(.17f,.22f,.23f,.93f):new Color(.025f,.055f,.066f,.86f));
            if(hover) Fill(new Rect(78,y,3,47),Copper);
            GUI.enabled=enabled;
            bool clicked=GUI.Button(r,text,item);GUI.enabled=true;return clicked;
        }
        public static void Draw(PooplineGame game)
        {
            if(heading==null)
            {
                heading=Text(58,Cream,FontStyle.Bold);label=Text(19,Cream);hint=Text(13,new Color(.56f,.65f,.64f));
                item=new GUIStyle(GUI.skin.label){fontSize=18,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,12,0,0),normal={textColor=Cream},hover={textColor=Color.white}};
            }
            if(!game.InMainMenu) Fill(new Rect(0,0,1280,720),new Color(.005f,.015f,.023f,.78f));
            for(int x=0;x<700;x+=8) Fill(new Rect(x,0,8,720),new Color(.007f,.018f,.025f,Mathf.Lerp(.97f,0,x/700f)));
            GUI.Label(new Rect(78,62,550,80),"POOPLINE",heading);
            GUI.Label(new Rect(82,145,510,26),"P O D  M I A S T E M .   P O  O M A C K U .",hint);
            Fill(new Rect(82,190,48,2),Copper);
            float y=224;
            if(game.Page==MenuPage.Settings)
            {
                GUI.Label(new Rect(82,y,480,35),"Ustawienia",label);
                GUI.Label(new Rect(82,y+57,390,27),"Głośność  ·  "+Mathf.RoundToInt(game.Settings.Volume*100)+"%",label);
                game.Settings.Volume=GUI.HorizontalSlider(new Rect(84,y+97,376,22),game.Settings.Volume,0,1);
                GUI.Label(new Rect(82,y+138,390,27),"Tło i kapanie  ·  "+Mathf.RoundToInt(game.Settings.Ambience*100)+"%",label);
                game.Settings.Ambience=GUI.HorizontalSlider(new Rect(84,y+178,376,22),game.Settings.Ambience,0,1);
                if(Button(y+223,"Ruch tła: "+(game.Settings.MenuMotion?"włączony":"wyłączony"))) {game.Settings.MenuMotion=!game.Settings.MenuMotion;game.Sound.PlayClick();}
                if(Button(y+284,"Wróć")) game.BackFromPanel();
            }
            else if(game.Page==MenuPage.Controls)
            {
                GUI.Label(new Rect(82,y,450,35),"Sterowanie",label);
                GUI.Label(new Rect(82,y+51,435,220),"WASD / strzałki    Ruch\n\nKierunek drabiny    Wspinanie\nF (przytrzymaj)    Obrót zaworu\nE / M    Odkryta mapa\nEsc    Pauza\nR    Powrót do punktu kontrolnego",label);
                if(Button(y+293,"Wróć")) game.BackFromPanel();
            }
            else if(game.Page==MenuPage.NewGame)
            {
                GUI.Label(new Rect(82,y,420,100),"Zacząć od nowa?\nObecny zapis i odkryta mapa zostaną zastąpione.",label);
                if(Button(y+128,"Rozpocznij nową grę")) game.StartNewGame();
                if(Button(y+187,"Zachowaj zapis")) game.BackFromPanel();
            }
            else if(game.InMainMenu)
            {
                if(Button(y,"Kontynuuj",game.CanContinue)) game.ContinueJourney();
                GUI.Label(new Rect(84,y+54,470,28),game.ContinueDescription,hint);
                if(Button(y+96,"Nowa gra")) game.RequestNewGame();
                if(Button(y+155,"Ustawienia")) game.OpenPanel(MenuPage.Settings);
                if(Button(y+214,"Sterowanie")) game.OpenPanel(MenuPage.Controls);
                if(Button(y+273,"Wyjdź")) game.QuitGame();
            }
            else if(game.Completed)
            {
                GUI.Label(new Rect(82,y,470,48),"Wyszedłeś z kanałów.",label);
                GUI.Label(new Rect(82,y+58,420,70),"Dwie dzielnice pod miastem są już za tobą. Ciąg dalszy jeszcze przed nami.",label);
                if(Button(y+155,"Nowa gra")) game.RequestNewGame();
                if(Button(y+214,"Menu główne")) game.ReturnToMenu();
            }
            else
            {
                GUI.Label(new Rect(82,y,420,40),"Chwila na oddech.",label);
                if(Button(y+64,"Wróć do gry")) game.TogglePause();
                if(Button(y+123,"Wróć do punktu kontrolnego")) game.RetryCheckpoint();
                if(Button(y+182,"Ustawienia")) game.OpenPanel(MenuPage.Settings);
                if(Button(y+241,"Zapisz i wróć do menu")) game.ReturnToMenu();
            }
            GUI.Label(new Rect(82,655,460,35),"0.6.2  /  POD KAMIENICAMI + ŻELIWNE MOSTY",hint);
            GUI.Label(new Rect(900,655,315,35),game.InMainMenu?"POLSKA. GDZIEŚ POD TWOJĄ ULICĄ.":"Postęp zapisuje się automatycznie.",hint);
        }
    }
}
