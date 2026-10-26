using UnityEngine;

namespace Poopline
{
    public static class ThoughtBubbleView
    {
        private static GUIStyle frame, text;
        private static Texture2D circle;
        private static readonly Color Paper=new Color(.91f,.9f,.82f);
        private static readonly Color Ink=new Color(.055f,.095f,.105f);

        private static Texture2D Shape(bool round)
        {
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave};
            for(int y=0;y<32;y++) for(int x=0;x<32;x++)
            {
                Vector2 delta=round?new Vector2(x-15.5f,y-15.5f):new Vector2(Mathf.Max(0,Mathf.Abs(x-15.5f)-6),Mathf.Max(0,Mathf.Abs(y-15.5f)-6));
                float alpha=Mathf.Clamp01((round?15.5f:9.5f)-delta.magnitude+.5f);
                texture.SetPixel(x,y,new Color(1,1,1,alpha));
            }
            texture.Apply();return texture;
        }
        private static void Box(Rect rect,Color color)
        {GUI.color=color;GUI.Box(rect,GUIContent.none,frame);}
        public static void Draw(PooplineGame game,float scale)
        {
            var thought=game.Thoughts;
            if(!thought.Speaking || game.Frozen) return;
            Vector3 screen=game.GameCamera.WorldToScreenPoint(game.Player.transform.position+Vector3.up*1.98f);
            if(screen.z<=0) return;
            if(frame==null)
            {
                frame=new GUIStyle{normal={background=Shape(false)},border=new RectOffset(10,10,10,10)};
                circle=Shape(true);
                text=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true,alignment=TextAnchor.MiddleLeft,normal={textColor=Ink}};
            }
            var head=new Vector2((screen.x-(Screen.width-1280*scale)/2)/scale,
                (Screen.height-screen.y-(Screen.height-720*scale)/2)/scale);
            const float width=316;
            float height=text.CalcHeight(new GUIContent(thought.Text),width-32)+24;
            var box=new Rect(Mathf.Clamp(head.x-width/2,24,1256-width),Mathf.Clamp(head.y-height-31,125,548-height),width,height);
            Color previous=GUI.color;
            float alpha=thought.Opacity;
            Box(box,new Color(Ink.r,Ink.g,Ink.b,alpha));
            Box(new Rect(box.x+1.5f,box.y+1.5f,box.width-3,box.height-3),new Color(Paper.r,Paper.g,Paper.b,alpha));
            // Small trailing circles link the thought to the moving character, not to a world landmark.
            for(int n=0;n<3;n++)
            {
                float size=10-n*2.5f;
                float x=Mathf.Clamp(head.x-8+n*4,box.x+15,box.xMax-15);
                float y=box.yMax+4+n*7;
                GUI.color=new Color(Ink.r,Ink.g,Ink.b,alpha);GUI.DrawTexture(new Rect(x,y,size,size),circle);
                GUI.color=new Color(Paper.r,Paper.g,Paper.b,alpha);GUI.DrawTexture(new Rect(x+1,y+1,size-2,size-2),circle);
            }
            GUI.color=new Color(1,1,1,alpha);
            GUI.Label(new Rect(box.x+16,box.y+12,box.width-32,box.height-24),thought.Text,text);
            GUI.color=previous;
        }
    }
}
