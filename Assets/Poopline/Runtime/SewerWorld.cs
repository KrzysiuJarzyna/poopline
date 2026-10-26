using UnityEngine;

namespace Poopline
{
    // Authoring recipe: executed by the editor tool, then saved as ordinary scene objects.
    public static class SewerWorld
    {
        public static readonly string[] MaterialNames = { "Basalt", "Concrete", "Brick", "Metal", "Brass", "Water", "Foam", "Orange", "Navy", "Skin", "Glow", "Rat", "Pink", "Dark" };
        public static readonly Color[] Colors = {
            new Color32(20,36,43,255), new Color32(87,112,113,255), new Color32(56,79,83,255),
            new Color32(34,57,66,255), new Color32(210,155,65,255), new Color32(25,102,101,255),
            new Color32(94,184,166,255), new Color32(230,117,51,255), new Color32(30,48,68,255),
            new Color32(216,166,128,255), new Color32(255,216,139,255), new Color32(73,67,75,255),
            new Color32(189,124,129,255), new Color32(8,18,24,255)
        };
        private sealed class Builder
        {
            private readonly Transform root;
            private readonly Material[] materials;
            public Builder(Transform root, Material[] materials) { this.root = root; this.materials = materials; }
            public GameObject Shape(string name, Vector3 p, Vector3 s, int material, bool solid = false, PrimitiveType type = PrimitiveType.Cube, Transform parent = null)
            {
                var go = GameObject.CreatePrimitive(type);
                go.name = name; go.transform.SetParent(parent == null ? root : parent, false);
                go.transform.localPosition = p; go.transform.localScale = s;
                go.GetComponent<Renderer>().sharedMaterial = materials[material];
                if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
                return go;
            }
            public Transform Group(string name, Vector3 p)
            { var go = new GameObject(name); go.transform.SetParent(root, false); go.transform.localPosition = p; return go.transform; }
            public void Beam(string name, Vector3 a, Vector3 b, float thickness, int mat)
            {
                var go = Shape(name, (a+b)/2, new Vector3(thickness, Vector3.Distance(a,b)/2, thickness), mat, false, PrimitiveType.Cylinder);
                go.transform.up = (b-a).normalized;
            }
            public void Lamp(float x, float z, float y = 2.7f)
            {
                Shape("Lamp bracket", new Vector3(x,y,z), new Vector3(.5f,.22f,.4f),3);
                Shape("Amber lamp", new Vector3(x,y-.1f,z-.22f), new Vector3(.3f,.12f,.06f),10);
                var go = new GameObject("Warm pool of light"); go.transform.SetParent(root,false); go.transform.localPosition = new Vector3(x,y-.3f,z-.5f);
                var light = go.AddComponent<Light>(); light.type=LightType.Point; light.color=new Color(1,.62f,.27f); light.range=4.4f; light.intensity=4.5f;
                light.shadows=LightShadows.Soft; light.shadowBias=.03f; light.shadowNormalBias=.2f;
            }
            public void Ladder(Vector3 bottom, float yaw)
            {
                var t = Group("Ladder — follow its facing direction",bottom); t.rotation=Quaternion.Euler(0,yaw,0);
                t.gameObject.AddComponent<SewerLadder>();
                foreach (float x in new[]{-.43f,.43f}) Shape("Ladder rail",new Vector3(x,1.7f,.28f),new Vector3(.09f,3.4f,.09f),4,false,PrimitiveType.Cube,t);
                for (int i=0;i<9;i++) Shape("Rung",new Vector3(0,.18f+i*.3f,.25f),new Vector3(.86f,.065f,.16f),4,false,PrimitiveType.Cube,t);
                Shape("Ladder landing stripe",new Vector3(0,.018f,-.5f),new Vector3(1.2f,.03f,.16f),4,false,PrimitiveType.Cube,t);
            }
            public void Human()
            {
                var player = Group("Player",new Vector3(1.5f,.05f,-3.5f));
                var controller=player.gameObject.AddComponent<CharacterController>();
                controller.height=1.75f; controller.radius=.28f; controller.center=new Vector3(0,.89f,0);
                controller.stepOffset=.16f; controller.slopeLimit=48; controller.skinWidth=.035f; controller.minMoveDistance=0;
                player.gameObject.AddComponent<SewerPlayer>();
                var human=new GameObject("Human").transform; human.SetParent(player,false);
                Shape("Jacket",new Vector3(0,1.12f,0),new Vector3(.55f,.59f,.34f),7,false,PrimitiveType.Capsule,human);
                Shape("Belt",new Vector3(0,.82f,0),new Vector3(.44f,.1f,.32f),8,false,PrimitiveType.Cube,human);
                Shape("Head",new Vector3(0,1.62f,0),new Vector3(.34f,.39f,.33f),9,false,PrimitiveType.Sphere,human);
                Shape("Hair",new Vector3(0,1.76f,-.03f),new Vector3(.35f,.18f,.32f),8,false,PrimitiveType.Sphere,human);
                Shape("Nose",new Vector3(0,1.62f,.18f),new Vector3(.08f,.09f,.1f),9,false,PrimitiveType.Sphere,human);
                Shape("Reflective band",new Vector3(0,1.11f,.174f),new Vector3(.45f,.07f,.015f),10,false,PrimitiveType.Cube,human);
                foreach (int side in new[]{-1,1})
                {
                    var leg=new GameObject(side<0?"Left leg":"Right leg").transform; leg.SetParent(human,false);leg.localPosition=new Vector3(side*.14f,.79f,0);
                    Shape("Trouser leg",new Vector3(0,-.28f,0),new Vector3(.18f,.57f,.2f),8,false,PrimitiveType.Cube,leg);
                    Shape("Boot",new Vector3(0,-.66f,.055f),new Vector3(.21f,.18f,.33f),13,false,PrimitiveType.Cube,leg);
                    var arm=new GameObject(side<0?"Left arm":"Right arm").transform;arm.SetParent(human,false);arm.localPosition=new Vector3(side*.32f,1.35f,0);
                    Shape("Sleeve",new Vector3(0,-.2f,0),new Vector3(.17f,.44f,.19f),7,false,PrimitiveType.Capsule,arm);
                    Shape("Hand",new Vector3(0,-.44f,0),new Vector3(.14f,.16f,.14f),9,false,PrimitiveType.Sphere,arm);
                    if(side<0)
                    {
                        Shape("Phone",new Vector3(0,-.45f,.1f),new Vector3(.13f,.21f,.035f),8,false,PrimitiveType.Cube,arm);
                        Shape("Screen",new Vector3(0,-.45f,.12f),new Vector3(.1f,.16f,.01f),10,false,PrimitiveType.Cube,arm);
                        var phone=new GameObject("Phone light");phone.transform.SetParent(arm,false);phone.transform.localPosition=new Vector3(0,-.4f,.15f);
                        var light=phone.AddComponent<Light>();light.type=LightType.Spot;light.range=7;light.spotAngle=62;light.innerSpotAngle=32;light.intensity=12;light.color=new Color(.68f,.85f,1);
                        light.shadows=LightShadows.Soft;light.shadowBias=.02f;light.shadowNormalBias=.15f;
                        var spill=new GameObject("Phone reflected glow");spill.transform.SetParent(arm,false);spill.transform.localPosition=new Vector3(0,-.35f,.35f);
                        var bounce=spill.AddComponent<Light>();bounce.type=LightType.Point;bounce.range=2.4f;bounce.intensity=1.4f;bounce.color=light.color;
                    }
                }
            }
            public void Rat(Vector3 start, Vector3 end, float speed = 1.1f)
            {
                var rat=Group("Szczur patrolujący pomost",start);
                var patrol=rat.gameObject.AddComponent<RatPatrol>();patrol.start=start;patrol.end=end;patrol.speed=speed;
                Shape("Rat body",new Vector3(0,.25f,0),new Vector3(.57f,.5f,.9f),11,false,PrimitiveType.Sphere,rat);
                Shape("Rat head",new Vector3(0,.28f,.48f),new Vector3(.4f,.36f,.45f),11,false,PrimitiveType.Sphere,rat);
                Shape("Nose",new Vector3(0,.25f,.73f),new Vector3(.13f,.12f,.1f),12,false,PrimitiveType.Sphere,rat);
                foreach(float side in new[]{-1f,1f})
                {
                    Shape("Ear",new Vector3(side*.19f,.48f,.35f),new Vector3(.2f,.23f,.08f),12,false,PrimitiveType.Sphere,rat);
                    Shape("Eye",new Vector3(side*.15f,.36f,.62f),new Vector3(.065f,.065f,.065f),10,false,PrimitiveType.Sphere,rat);
                    foreach(float z in new[]{-.27f,.28f}) Shape("Paw",new Vector3(side*.23f,.04f,z),new Vector3(.16f,.09f,.24f),12,false,PrimitiveType.Sphere,rat);
                }
                var tail=Shape("Tail",new Vector3(.1f,.12f,-.8f),new Vector3(.07f,.5f,.07f),12,false,PrimitiveType.Cylinder,rat);
                tail.transform.localRotation=Quaternion.Euler(80,15,0);
            }
            private void Wall(float length, float z, bool rear, float[] openings)
            {
                var holes = new System.Collections.Generic.List<float>(openings);
                holes.Sort();
                float start = 0;
                foreach (float hole in holes)
                {
                    WallPart(start, hole - 1.8f, z, rear);
                    if (rear) Shape("Portal lintel",new Vector3(hole,3.85f,z),new Vector3(3.6f,1.3f,.55f),2,true).AddComponent<SewerCutaway>();
                    start = hole + 1.8f;
                }
                WallPart(start, length, z, rear);
                if (!rear) return;
                for (int row=0;row<7;row++)
                    for (float x=1;x<length-1;x+=2)
                    {
                        float px=x+(row%2)*.65f;
                        bool opening=false;
                        foreach (float hole in holes) if (Mathf.Abs(px-hole)<2.7f) opening=true;
                        if (!opening) Shape("Stara cegła — wiązanie mijankowe",new Vector3(px,.33f+row*.58f,z-.32f),new Vector3(1.92f,.53f,.12f),((int)x+row)%7==0?1:2);
                    }
                for (float x=7;x<length-3;x+=9)
                {
                    bool opening=false;
                    foreach (float hole in holes) if (Mathf.Abs(x-hole)<2.5f) opening=true;
                    if(opening) continue;
                    Shape("Murowana przypora",new Vector3(x,2.2f,z-.5f),new Vector3(.48f,4.4f,.6f),0);
                    Shape("Zwieńczenie przypory",new Vector3(x,4.45f,z-.5f),new Vector3(.8f,.2f,.9f),1);
                    Lamp(x,z-.9f);
                }
            }
            private void WallPart(float from, float to, float z, bool rear)
            {
                if(to<=from) return;
                Shape(rear?"Ceglana ściana kolektora":"Niski murek od strony kamery",new Vector3((from+to)/2,rear?2.25f:.25f,z),new Vector3(to-from,rear?4.5f:.5f,.55f),rear?2:0,true);
            }
            private void Arch(Vector3 position, float yaw = 0, float radius = 1.7f)
            {
                var arch=Group("Ceglany łuk kanału",position);arch.localRotation=Quaternion.Euler(0,yaw,0);
                arch.gameObject.AddComponent<SewerCutaway>();
                foreach(float side in new[]{-1f,1f})
                    Shape("Ościeże",new Vector3(side*(radius+.2f),.7f,0),new Vector3(.4f,1.55f,.65f),2,false,PrimitiveType.Cube,arch);
                for(int i=0;i<=12;i++)
                {
                    float angle=i*Mathf.PI/12;
                    var stone=Shape("Klin sklepienia",new Vector3(Mathf.Cos(angle)*radius,1.45f+Mathf.Sin(angle)*radius,0),new Vector3(.49f,.42f,.65f),i%4==0?1:2,false,PrimitiveType.Cube,arch);
                    stone.transform.localRotation=Quaternion.Euler(0,0,i*15-90);
                }
            }
            private void Drips(Vector3 origin, float fall, int count=6)
            {
                for(int i=0;i<count;i++)
                {
                    var drop=Shape("Kropla z nieszczelnego przewodu",origin+new Vector3((i%3-1)*.18f,0,(i%2)*.12f),new Vector3(.035f,.15f+(i%3)*.06f,.035f),6);
                    var motion=drop.AddComponent<WaterDrip>();motion.fall=fall;motion.phase=i*.37f;
                }
                for(int i=0;i<3;i++)
                    Shape("Kręgi na wodzie",origin+new Vector3((i-1)*.22f,-fall+.02f,.1f),new Vector3(.24f+i*.13f,.009f,.035f),6);
            }
            private void WaterEnd(float x, float z, bool inlet)
            {
                // The water continues through a real opening, behind a grille, then drops out of sight.
                float outward=inlet?-1:1;
                Arch(new Vector3(x,0,z),90,2.15f);
                foreach(float side in new[]{-1f,1f})
                    Shape("Mur przy wlocie",new Vector3(x,1.35f,z+side*3.7f),new Vector3(.6f,4.5f,3),2,true);
                Shape("Nadproże odpływu",new Vector3(x,3.9f,z),new Vector3(.6f,1.4f,4.5f),2,true);
                Shape("Woda wewnątrz tunelu",new Vector3(x+outward*1.5f,-.28f,z),new Vector3(3,.045f,4.35f),5);
                Shape("Dno tunelu wodnego",new Vector3(x+outward*1.5f,-1,z),new Vector3(3,.3f,4.5f),2,true);
                Shape("Ciemna komora za kratą",new Vector3(x+outward*3,1,z),new Vector3(.3f,5,5),13);
                Shape("Spadek wody do kolektora",new Vector3(x+outward*2.5f,-1.25f,z),new Vector3(.08f,2,4.2f),5);
                for(float dz=-2;dz<=2;dz+=.45f)
                    Shape("Żeliwna krata wlotowa",new Vector3(x,1.05f,z+dz),new Vector3(.13f,3.8f,.07f),3);
                var stop=Group("Granica komory technicznej",new Vector3(x,1,z)).gameObject.AddComponent<BoxCollider>();stop.size=new Vector3(.3f,4.5f,4.4f);
                Drips(new Vector3(x+outward*.4f,inlet?2.1f:-.35f,z),inlet?2.35f:1.7f,8);
            }
            private void Canal(float length, float z, float[] northOpenings, float[] southOpenings, bool reverse)
            {
                Shape("Fundament kolektora",new Vector3(length/2,-1.3f,z),new Vector3(length,.8f,10.5f),0,true);
                Shape("Dno kanału",new Vector3(length/2,-1,z),new Vector3(length,.3f,4.4f),2,true);
                foreach(float side in new[]{-1f,1f})
                {
                    Shape("Betonowy chodnik",new Vector3(length/2,-.45f,z+side*3.55f),new Vector3(length,.9f,2.7f),1,true);
                    var bank=Shape("Łagodne zejście do wody",new Vector3(length/2,-.52f,z+side*1.03f),new Vector3(length,.2f,2.55f),1,true);
                    bank.transform.localRotation=Quaternion.Euler(-side*19.5f,0,0);
                    for(float x=1;x<length;x+=2)
                    {
                        Shape("Łączenie płyt chodnika",new Vector3(x,.007f,z+side*3.55f),new Vector3(.025f,.015f,2.65f),0);
                        if(((int)x)%4==1) Shape("Wytarty znacznik krawędzi",new Vector3(x,.025f,z+side*2.23f),new Vector3(.55f,.04f,.09f),4);
                    }
                }
                Wall(length,z+5.15f,true,northOpenings);
                Wall(length,z-5.15f,false,southOpenings);
                Shape("Lustro płynącej wody",new Vector3(length/2,-.28f,z),new Vector3(length,.045f,4.35f),5);
                var flow=Group("Nurt głównego kanału",new Vector3(length/2,-.65f,z)).gameObject.AddComponent<FlowVolume>();
                flow.size=new Vector3(length,1,4.35f);flow.localCurrent=new Vector3(reverse?-.75f:.9f,0,0);
                for(int i=0;i<length;i++)
                {
                    var streak=Shape("Refleks nurtu",new Vector3(1.4f+i*.96f,-.25f,z-1.6f+(i%4)*.95f),new Vector3(.45f+(i%3)*.14f,.012f,.025f),6);
                    streak.AddComponent<WaterMotion>().direction=reverse?Vector3.left:Vector3.right;
                }
                Beam("Rura ciepłownicza",new Vector3(.5f,4.1f,z+4.05f),new Vector3(length-.5f,4.1f,z+4.05f),.35f,4);
                for(float x=3;x<length;x+=6) Beam("Obejma przewodu",new Vector3(x,4.1f,z+4.05f),new Vector3(x+.12f,4.1f,z+4.05f),.44f,3);
                WaterEnd(0,z,true);WaterEnd(length,z,false);
            }
            private void Passage(float x, float from, float to, bool closed = false)
            {
                float middle=(from+to)/2, length=to-from;
                Shape("Dno bocznego kanału",new Vector3(x,-.15f,middle),new Vector3(3.5f,.3f,length),1,true);
                Shape("Woda w bocznym kanale",new Vector3(x,.075f,middle),new Vector3(3.35f,.035f,length),5);
                foreach(float side in new[]{-1f,1f})
                    Shape("Ściana bocznego kanału",new Vector3(x+side*1.9f,1.5f,middle),new Vector3(.35f,3.3f,length),2,true);
                var current=Group("Nurt bocznego kanału",new Vector3(x,.1f,middle)).gameObject.AddComponent<FlowVolume>();
                current.size=new Vector3(3.35f,1,length);current.localCurrent=new Vector3(0,0,.32f);
                for(float z=from+.8f;z<to-.8f;z+=1.5f)
                {
                    var streak=Shape("Refleks w odnodze",new Vector3(x+.5f,.1f,z),new Vector3(.035f,.012f,.4f),6);
                    streak.AddComponent<WaterMotion>().direction=Vector3.forward;
                }
                for(float z=from;z<to;z+=4.5f) Arch(new Vector3(x,0,z));
                Lamp(x+.8f,middle+1,2.6f);
                Beam("Przeciekający przewód",new Vector3(x-1.5f,2.8f,middle),new Vector3(x+1.5f,2.8f,middle),.22f,3);
                Drips(new Vector3(x-.6f,2.55f,middle),2.45f);
                if(closed)
                {
                    Shape("Ściana komory bocznej",new Vector3(x,1.5f,to),new Vector3(3.6f,3.3f,.3f),2,true);
                    Shape("Ciemny odpływ w dół",new Vector3(x,.09f,to-.8f),new Vector3(2,.04f,1.2f),13);
                    for(float dx=-.9f;dx<=.9f;dx+=.3f) Shape("Krata studzienki",new Vector3(x+dx,.12f,to-.8f),new Vector3(.06f,.04f,1.2f),3);
                    Drips(new Vector3(x,2,to-.8f),1.85f);
                }
            }
            private void Barrier(float x, float z)
            {
                Shape("Zamknięta krata chodnika",new Vector3(x,1.1f,z),new Vector3(.18f,2.2f,2.75f),3,true);
                for(int i=0;i<5;i++) Shape("Stary żółty pas na kracie",new Vector3(x-.1f,.3f+i*.37f,z),new Vector3(.025f,.08f,2.5f),4);
            }
            private void ServiceDetails(float length, float z, int row)
            {
                foreach(float x in new[]{length*.28f, length*.74f})
                {
                    Beam("Rura deszczowa z kamienicy",new Vector3(x,2.5f,z+4.8f),new Vector3(x,2.5f,z+1.6f),.32f,3);
                    Beam("Kolano odpływu",new Vector3(x,2.5f,z+1.6f),new Vector3(x,.7f,z+1.6f),.32f,3);
                    Shape("Ciemny wylot rury",new Vector3(x,.68f,z+1.6f),new Vector3(.27f,.02f,.27f),13,false,PrimitiveType.Cylinder);
                    Drips(new Vector3(x,.65f,z+1.6f),.9f,8);
                }
                float coverX=length*(row%2==0?.38f:.69f);
                Shape("Żeliwny właz rewizyjny",new Vector3(coverX,.035f,z-3.55f),new Vector3(1.35f,.03f,1.35f),3,false,PrimitiveType.Cylinder);
                for(int i=-2;i<=2;i++) Shape("Żebro pokrywy włazu",new Vector3(coverX+i*.2f,.07f,z-3.55f),new Vector3(.045f,.02f,.8f),0);
                Vector3 hub=new Vector3(length*.7f,1.6f,z+4.7f);
                for(int i=0;i<12;i++)
                {
                    float a=i*Mathf.PI/6, b=(i+1)*Mathf.PI/6;
                    Vector3 pa=hub+new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*.55f;
                    Vector3 pb=hub+new Vector3(Mathf.Cos(b),Mathf.Sin(b),0)*.55f;
                    Beam("Koło starej zasuwy",pa,pb,.055f,4);
                    if(i%3==0) Beam("Szprycha zasuwy",hub,pa,.045f,3);
                }
            }
            private void Rail(Vector3 a, Vector3 b)
            {
                Beam("Poręcz",a+Vector3.up*.8f,b+Vector3.up*.8f,.075f,4);
                Beam("Dolna poprzeczka",a+Vector3.up*.35f,b+Vector3.up*.35f,.045f,3);
                int posts=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(a,b)/1.5f));
                for(int i=0;i<=posts;i++) {Vector3 p=Vector3.Lerp(a,b,i/(float)posts);Beam("Słupek balustrady",p,p+Vector3.up*.85f,.065f,4);}
                var guard=Group("Zabezpieczenie balustrady",(a+b)/2+Vector3.up*.65f).gameObject.AddComponent<BoxCollider>();
                Vector3 d=b-a;guard.size=new Vector3(Mathf.Abs(d.x)+.08f,1.3f,Mathf.Abs(d.z)+.08f);
            }
            private void Bridge(float x,float z)
            {
                Shape("Górny most nad nurtem",new Vector3(x,2.45f,z+.5f),new Vector3(1.8f,.3f,7.6f),3,true).AddComponent<ElevatedWalkway>();
                Shape("Górny pomost wzdłuż ściany",new Vector3(x+4.8f,2.45f,z+3.55f),new Vector3(10.8f,.3f,1.9f),3,true).AddComponent<ElevatedWalkway>();
                for(float dz=-2.8f;dz<4;dz+=.55f) Shape("Stopnica mostu",new Vector3(x,2.613f,z+dz),new Vector3(1.7f,.025f,.06f),1);
                for(float dx=.7f;dx<10;dx+=.55f) Shape("Stopnica pomostu",new Vector3(x+dx,2.613f,z+3.55f),new Vector3(.06f,.025f,1.8f),1);
                Rail(new Vector3(x-.85f,2.6f,z-3.25f),new Vector3(x-.85f,2.6f,z+4.45f));
                Rail(new Vector3(x+.85f,2.6f,z-3.25f),new Vector3(x+.85f,2.6f,z+2.6f));
                Rail(new Vector3(x-.85f,2.6f,z+4.45f),new Vector3(x+10.2f,2.6f,z+4.45f));
                Rail(new Vector3(x+.9f,2.6f,z+2.65f),new Vector3(x+10.2f,2.6f,z+2.65f));
                foreach(float side in new[]{-1f,1f})
                {
                    Rail(new Vector3(x+side*.85f,2.6f,z-3.25f),new Vector3(x+side*.55f,2.6f,z-3.25f));
                    Rail(new Vector3(x+10.2f,2.6f,z+3.55f+side*.9f),new Vector3(x+10.2f,2.6f,z+3.55f+side*.55f));
                }
                for(float dx=2;dx<10;dx+=2) Shape("Podpora stalowego pomostu",new Vector3(x+dx,1.2f,z+4.2f),new Vector3(.14f,2.4f,.14f),3);
                Ladder(new Vector3(x,0,z-3.9f),0);
                Ladder(new Vector3(x+10.8f,0,z+3.55f),-90);
                Lamp(x,z-4.5f,4.1f);
            }
            public void Stage(int index)
            {
                var level=root.gameObject.AddComponent<SewerLevel>();
                float length=index==0?100:124, spacing=index==0?22:26, right=length-4;
                level.title=index==0?"Pod kamienicami":"Żeliwne mosty";
                level.introduction=index==0?"Telefon oświetla drogę. Ruszaj wzdłuż kanału.":"Głębiej pod miastem. Szukaj przejść między pomostami.";
                level.start=new Vector3(2,.05f,-3.55f);
                level.playableBounds=new Bounds(new Vector3(length/2,2,spacing*1.5f+3),new Vector3(length+8,12,spacing*3+25));
                level.checkpoints=new Vector3[3];
                level.objectives=index==0?new[]{"Znajdź przejście do bocznego kanału.","Przejdź przez stary kolektor.","Znajdź drogę przez odnogi kanału.","Przejdź kanałem do następnego poziomu."}:new[]{"Znajdź przejście obok pierwszego patrolu.","Poszukaj drogi między mostami.","Przedostań się przez galerię pomp.","Odszukaj końcowy kanał odpływowy."};
                var route=new System.Collections.Generic.List<Vector3>();
                for(int row=0;row<4;row++)
                {
                    float z=row*spacing, exitX=row%2==0?right:4, entryX=row%2==0?4:right;
                    float alcoveX=row%2==0?length*.44f:length*.58f;
                    Canal(length,z,new[]{exitX,alcoveX},row==0?new float[0]:new[]{entryX},row%2!=0);
                    ServiceDetails(length,z,row);
                    Passage(alcoveX,z+4.9f,z+10.1f,true);
                    if(row<3)
                    {
                        Passage(exitX,z+4.9f,z+spacing-4.9f);
                        level.checkpoints[row]=new Vector3(exitX,.05f,z+spacing-3.55f);
                    }
                    else
                    {
                        Passage(exitX,z+4.9f,z+13,true);
                        level.exit=new Bounds(new Vector3(exitX,1,z+10.5f),new Vector3(2.8f,3,2.2f));
                        Lamp(exitX,z+12,2.7f);
                    }
                    if(index==0)
                    {
                        float approach=row%2==0?12:length-12, after=row%2==0?25:length-25;
                        Barrier(row%2==0?18:length-18,z-3.55f);
                        // A narrow, covered service bridge gives a dry place to stop and look around.
                        Shape("Niski mostek serwisowy",new Vector3(length*.63f,-.15f,z),new Vector3(2,.3f,4.7f),1,true);
                        route.Add(new Vector3(approach,0,z-3.55f));route.Add(new Vector3(approach,0,z));
                        route.Add(new Vector3(after,0,z));route.Add(new Vector3(after,0,z-3.55f));
                        route.Add(new Vector3(exitX,0,z-3.55f));
                    }
                    else
                    {
                        if(row<3)
                        {
                            float bx=row==0?20:row==1?76:42;
                            Bridge(bx,z);
                            Rat(new Vector3(bx+3,.12f,z-3.55f),new Vector3(bx+9,.12f,z-3.55f),row==0?.85f:1.15f);
                            if(row%2==0)
                            {
                                route.Add(new Vector3(bx,0,z-3.9f));
                                route.Add(new Vector3(bx,2.6f,z-3.05f));
                                route.Add(new Vector3(bx,2.6f,z+3.55f));
                                route.Add(new Vector3(bx+10,2.6f,z+3.55f));
                                route.Add(new Vector3(bx+11.45f,0,z+3.55f));
                            }
                            else
                            {
                                route.Add(new Vector3(100,0,z-3.55f));route.Add(new Vector3(100,0,z+3.55f));
                                route.Add(new Vector3(bx+11.45f,0,z+3.55f));
                                route.Add(new Vector3(bx+9.95f,2.6f,z+3.55f));
                                route.Add(new Vector3(bx,2.6f,z+3.55f));
                                route.Add(new Vector3(bx,2.6f,z-3.05f));
                                route.Add(new Vector3(bx,0,z-4.55f));
                                route.Add(new Vector3(exitX,0,z-3.55f));
                            }
                        }
                        else
                        {
                            Rat(new Vector3(82,.12f,z-3.55f),new Vector3(94,.12f,z-3.55f),1.25f);
                            Rat(new Vector3(30,.12f,z+3.55f),new Vector3(43,.12f,z+3.55f),1.05f);
                            route.Add(new Vector3(100,0,z-3.55f));route.Add(new Vector3(100,0,z));
                            route.Add(new Vector3(72,0,z));route.Add(new Vector3(72,0,z-3.55f));
                            route.Add(new Vector3(exitX,0,z-3.55f));
                        }
                        // Pump machinery and a closed side of the channel create distinct silhouettes.
                        for(int n=0;n<3;n++)
                        {
                            Shape("Pompa tłocząca",new Vector3(length*.48f+n*2,1,z+4.2f),new Vector3(1,1.5f,1),3,false,PrimitiveType.Cylinder);
                            Beam("Rura ssąca pompy",new Vector3(length*.48f+n*2,.45f,z+3.9f),new Vector3(length*.48f+n*2,-.35f,z+1.4f),.23f,4);
                        }
                        Shape("Dolny most betonowy",new Vector3(length-12,-.15f,z),new Vector3(2,.3f,4.7f),1,true);
                    }
                    route.Add(new Vector3(exitX,0,z+3.55f));
                    route.Add(new Vector3(exitX,0,row<3?z+spacing-3.55f:z+10.5f));
                }
                level.suggestedRoute=route.ToArray();
            }
            public void Camera()
            {
                var camera=Group("Poopline Camera",new Vector3(5,12.6f,-16.2f)).gameObject;
                camera.tag="MainCamera";camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-12,15));
                var view=camera.AddComponent<Camera>();view.orthographic=true;view.orthographicSize=5.8f;view.nearClipPlane=.1f;view.farClipPlane=80;
                view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color32(2,4,7,255);
                camera.AddComponent<AudioListener>();camera.AddComponent<SewerCamera>();
            }
        }
        public static void Build(Transform root, Material[] materials)
        {
            var shared=new Builder(root,materials);shared.Human();shared.Camera();
            for(int i=0;i<2;i++)
            {
                var stage=new GameObject(i==0?"01 — Pod kamienicami":"02 — Żeliwne mosty");stage.transform.SetParent(root,false);
                new Builder(stage.transform,materials).Stage(i);
                stage.SetActive(i==0);
            }
        }
    }
}
