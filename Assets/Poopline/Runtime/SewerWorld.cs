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
            private readonly System.Collections.Generic.Dictionary<Vector2Int,bool> floor = new System.Collections.Generic.Dictionary<Vector2Int,bool>();
            private readonly System.Collections.Generic.HashSet<Vector2Int> rooms = new System.Collections.Generic.HashSet<Vector2Int>();
            private readonly System.Collections.Generic.List<MapLandmark> landmarks = new System.Collections.Generic.List<MapLandmark>();
            private void Footprint(int x,int z,int width,int depth,bool water=false,bool build=true)
            {
                for(int dx=0;dx<width;dx++) for(int dz=0;dz<depth;dz++)
                {
                    var p=new Vector2Int(x+dx,z+dz);
                    floor[p]=water;
                    if(build) rooms.Add(p);
                }
            }
            private void Landmark(string label,float x,float z,bool exit=false)
            { landmarks.Add(new MapLandmark(label,new Vector3(x,0,z),exit)); }
            private void RoomWall(Vector3 a,Vector3 b,bool low)
            {
                Vector3 d=b-a, middle=(a+b)/2;
                bool horizontal=Mathf.Abs(d.x)>.1f;
                float length=d.magnitude,height=low?.65f:3.4f;
                var wall=Group("Ceglany mur komory",middle);
                Shape("Mur",new Vector3(0,height/2,0),new Vector3(horizontal?length:.24f,height,horizontal?.24f:length),2,true,PrimitiveType.Cube,wall);
                // Foreground masonry disappears only along the camera-to-player ray; collision remains.
                wall.gameObject.AddComponent<SewerCutaway>();
                int courses=low?1:6;
                for(int row=0;row<courses;row++)
                    for(float step=.5f;step<length-.2f;step+=1.1f)
                    {
                        float along=-length/2+step+(row%2)*.12f;
                        Shape("Cegła starego kolektora",new Vector3(horizontal?along:0,.26f+row*.55f,horizontal?0:along),
                            new Vector3(horizontal?1.03f:.29f,.49f,horizontal?.29f:1.03f),row%3==0?0:2,false,PrimitiveType.Cube,wall);
                    }
                Shape("Kamienna czapa muru",new Vector3(0,height+.06f,0),new Vector3(horizontal?length+.08f:.36f,.12f,horizontal?.36f:length+.08f),1,false,PrimitiveType.Cube,wall);
            }
            private void BuildRooms()
            {
                // Merge adjacent cells into strips: no internal walls at room/corridor junctions.
                for(int z=5;z<48;z++)
                {
                    int x=0;
                    while(x<54)
                    {
                        if(!rooms.Contains(new Vector2Int(x,z))) {x++;continue;}
                        int from=x;
                        while(rooms.Contains(new Vector2Int(x,z))) x++;
                        Shape("Płyty posadzki komory",new Vector3((from+x)/2f,-.16f,z+.5f),new Vector3(x-from,.32f,1),1,true);
                        if(z%2==0) Shape("Spoina posadzki",new Vector3((from+x)/2f,.007f,z),new Vector3(x-from,.014f,.022f),0);
                    }
                }
                for(int direction=0;direction<4;direction++)
                {
                    Vector2Int normal=direction==0?Vector2Int.up:direction==1?Vector2Int.down:direction==2?Vector2Int.right:Vector2Int.left;
                    bool horizontal=direction<2;
                    for(int line=0;line<55;line++)
                    {
                        int step=0;
                        while(step<55)
                        {
                            Vector2Int p=horizontal?new Vector2Int(step,line):new Vector2Int(line,step);
                            if(!rooms.Contains(p)||floor.ContainsKey(p+normal)) {step++;continue;}
                            int from=step;
                            do { step++;p=horizontal?new Vector2Int(step,line):new Vector2Int(line,step); }
                            while(step<55 && rooms.Contains(p) && !floor.ContainsKey(p+normal));
                            float edge=line+(direction==0||direction==2?1:0);
                            RoomWall(horizontal?new Vector3(from,0,edge):new Vector3(edge,0,from),horizontal?new Vector3(step,0,edge):new Vector3(edge,0,step),direction==1);
                        }
                    }
                }
            }
            private void WetTunnel(int x,int from,int to)
            {
                Shape("Płytka woda w przecieku",new Vector3(x,.065f,(from+to)/2f),new Vector3(3.6f,.04f,to-from),5);
                var flow=Group("Przeciek w odnodze",new Vector3(x,.1f,(from+to)/2f)).gameObject.AddComponent<FlowVolume>();
                flow.size=new Vector3(3.6f,1,to-from);flow.localCurrent=new Vector3(0,0,.25f);
                for(float z=from+.8f;z<to;z+=2.5f)
                {
                    Arch(new Vector3(x,0,z));
                    var streak=Shape("Ruch płytkiej wody",new Vector3(x+.5f,.09f,z),new Vector3(.04f,.01f,.5f),6);
                    streak.AddComponent<WaterMotion>().direction=Vector3.forward;
                }
                Beam("Pęknięta rura pod sklepieniem",new Vector3(x-1.5f,2.7f,from+2),new Vector3(x+1.5f,2.7f,from+2),.2f,3);
                Drips(new Vector3(x-.6f,2.55f,from+2),2.45f);
            }
            private void MapObstacle(float x,float z,float radius)
            {
                var keys=new System.Collections.Generic.List<Vector2Int>(floor.Keys);
                foreach(var p in keys)
                    if(Vector2.Distance(new Vector2(p.x+.5f,p.y+.5f),new Vector2(x,z))<radius) floor.Remove(p);
            }
            private void Cistern(float x,float z)
            {
                Shape("Okrągła ceglana cysterna",new Vector3(x,.8f,z),new Vector3(3.8f,.8f,3.8f),2,true,PrimitiveType.Cylinder);
                Shape("Żeliwny pierścień cysterny",new Vector3(x,1.63f,z),new Vector3(4.1f,.1f,4.1f),3,false,PrimitiveType.Cylinder);
                Shape("Woda w cysternie",new Vector3(x,1.75f,z),new Vector3(3.65f,.025f,3.65f),5,false,PrimitiveType.Cylinder);
                Beam("Zasilanie cysterny",new Vector3(x,3,z+4),new Vector3(x,3,z),.28f,4);
                Drips(new Vector3(x,2.8f,z),1.02f,10);
                Lamp(x-3,z+3,3.1f);
                Landmark("Cysterna",x-3,z);MapObstacle(x,z,2);
            }
            private void Pumps(float x,float z)
            {
                for(int n=0;n<3;n++)
                {
                    Shape("Stara pompa żeliwna",new Vector3(x+n*2,1,z),new Vector3(1.2f,1,1.2f),3,true,PrimitiveType.Cylinder);
                    Beam("Miedziane kolano pompy",new Vector3(x+n*2,.6f,z),new Vector3(x+n*2,.6f,z-1),.26f,4);
                    MapObstacle(x+n*2,z,.85f);
                    Shape("Koło zaworu",new Vector3(x+n*2,1.4f,z-.7f),new Vector3(.7f,.07f,.7f),4,false,PrimitiveType.Cylinder).transform.rotation=Quaternion.Euler(90,0,0);
                }
                Lamp(x+2,z+.6f,3.1f);Landmark("Stare pompy",x+2,z-2.5f);
            }
            private void Drain(float x,float z)
            {
                Shape("Studnia odpływowa",new Vector3(x,.018f,z),new Vector3(2.2f,.025f,2.2f),13);
                for(float dx=-1;dx<=1;dx+=.3f) Shape("Krata studni",new Vector3(x+dx,.04f,z),new Vector3(.07f,.04f,2.2f),3);
                Drips(new Vector3(x,3,z),2.94f,8);
                Beam("Odpływ z podwórka kamienicy",new Vector3(x,3.3f,z+2),new Vector3(x,3.3f,z),.3f,3);
            }
            private void ExitDoor(SewerLevel level,float x,float z)
            {
                Arch(new Vector3(x,0,z),90,1.7f);
                Shape("Ciemność następnego kolektora",new Vector3(x+.5f,1.35f,z),new Vector3(.08f,2.7f,3),13);
                for(int n=0;n<5;n++) Shape("Jasne płytki przy odpływie",new Vector3(x-3+n*.55f,.025f,z),new Vector3(.22f,.035f,.8f),1);
                var l=Group("Chłodne światło z dalszego kanału",new Vector3(x-.4f,2.1f,z)).gameObject.AddComponent<Light>();
                l.color=new Color(.4f,.9f,.8f);l.range=5;l.intensity=5;l.shadows=LightShadows.Soft;
                level.exit=new Bounds(new Vector3(x-.4f,1,z),new Vector3(1.5f,3,2.6f));
                Landmark("Dalszy kanał →",x-1,z,true);
            }
            public void Stage(int index)
            {
                var level=root.gameObject.AddComponent<SewerLevel>();
                level.title=index==0?"Pod kamienicami":"Żeliwne mosty";
                level.introduction=index==0?"Przejście dalej poznasz po chłodnym świetle. E / M — mapa.":"Mosty, woda czy boczne komory? Wybierz drogę obok patroli.";
                level.start=new Vector3(3,.05f,-3.55f);
                int length=index==0?32:44;
                Canal(length,0,index==0?new[]{8f,24f}:new[]{8f,24f,36f},new float[0],false);
                ServiceDetails(length,0,0);
                Lamp(9.9f,4.4f);Lamp(26,4.4f);
                for(int x=0;x<length;x++) for(int z=-5;z<5;z++) floor[new Vector2Int(x,z)]=z>=-2&&z<2;
                // Both entries rejoin through different chambers; the upper gallery closes a second loop.
                Footprint(6,5,4,9,true);
                if(index==0)
                {
                    Footprint(2,14,12,10);Footprint(22,5,4,5);Footprint(20,10,12,10);
                    Footprint(14,16,6,4);Footprint(6,24,4,6);Footprint(24,20,4,10);
                    Footprint(6,30,22,4);Footprint(0,28,6,6);Footprint(28,26,12,12);
                    BuildRooms();WetTunnel(8,5,14);Cistern(8,19);Pumps(24,18);
                    Drain(2,31);Landmark("Studnia pod podwórkiem",3,30);
                    Arch(new Vector3(16,0,18),90);Arch(new Vector3(26,0,24));
                    Lamp(11,23);Lamp(25,29);Lamp(34,37);
                    // A dry crossing is optional; water remains traversable everywhere along both banks.
                    Shape("Niski mostek serwisowy",new Vector3(17,-.15f,0),new Vector3(2,.3f,4.7f),1,true);
                    ExitDoor(level,38,34);
                    level.checkpoints=new[]{new Vector3(8,.05f,15),new Vector3(26,.05f,31)};
                    level.objectives=new[]{"Znajdź przejście do dalszego kanału.","Komory łączą się. Szukaj dalszego kanału.","Szukaj chłodnego światła przy odpływie."};
                    level.suggestedRoute=new[]{P(8,-3.55f),P(8,3.55f),P(8,15),P(4,15),P(4,23),P(8,23),P(8,32),P(26,32),P(34,32),P(37.6f,34)};
                    level.alternateRoute=new[]{P(24,-3.55f),P(24,3.55f),P(24,12),P(30,12),P(30,19),P(26,19),P(26,32),P(34,32),P(37.6f,34)};
                    level.playableBounds=new Bounds(new Vector3(19,2,17),new Vector3(46,12,48));
                }
                else
                {
                    Footprint(2,14,14,10);Footprint(22,5,4,7);Footprint(34,5,4,7);
                    Footprint(22,12,16,10);Footprint(16,16,6,4);Footprint(12,24,4,8);
                    Footprint(12,32,14,8);Footprint(24,22,4,10);Footprint(34,22,4,12);
                    Footprint(26,32,18,4);Footprint(40,28,10,14);
                    BuildRooms();WetTunnel(8,5,14);Cistern(8,19);Pumps(27,20);
                    Drain(17,37);Landmark("Komora pod włazem",18,33);
                    Arch(new Vector3(20,0,18),90);Arch(new Vector3(36,0,27));
                    Lamp(13,23);Lamp(14,31);Lamp(26,29);Lamp(44,40);
                    Bridge(8,0);Bridge(28,0);
                    Rat(new Vector3(11,.12f,-3.55f),new Vector3(17,.12f,-3.55f),.85f);
                    Rat(new Vector3(26,.12f,14),new Vector3(34,.12f,14),1.05f);
                    Rat(new Vector3(16,.12f,35),new Vector3(23,.12f,35),1.1f);
                    ExitDoor(level,48,38);
                    level.checkpoints=new[]{new Vector3(24,.05f,9),new Vector3(36,.05f,32)};
                    level.objectives=new[]{"Znajdź drogę obok patroli. E / M — mapa.","Boczne komory prowadzą wokół pomp.","Chłodne światło prowadzi do wyjścia."};
                    level.suggestedRoute=new[]{P(8,-3.9f),new Vector3(8,2.6f,-3.05f),new Vector3(8,2.6f,3.55f),new Vector3(18,2.6f,3.55f),P(19.45f,3.55f),P(24,3.55f),P(24,9),P(24,17),P(36,17),P(36,26),P(36,34),P(44,34),P(47.6f,38)};
                    level.alternateRoute=new[]{P(5,-3.55f),P(5,3.55f),P(8,3.55f),P(8,15),P(4,15),P(4,23),P(14,23),P(14,33),P(25,33),P(36,33),P(44,33),P(47.6f,38)};
                    level.playableBounds=new Bounds(new Vector3(24,2,18),new Vector3(56,12,52));
                }
                Landmark("Wejście",3,-3.55f);
                var ordered=new System.Collections.Generic.List<Vector2Int>(floor.Keys);
                ordered.Sort((a,b)=>a.y==b.y?a.x.CompareTo(b.x):a.y.CompareTo(b.y));
                level.mapCells=new MapCell[ordered.Count];
                for(int i=0;i<ordered.Count;i++) {var p=ordered[i];level.mapCells[i]=new MapCell(p.x+.5f,p.y+.5f,floor[p]);}
                level.landmarks=landmarks.ToArray();
            }
            private static Vector3 P(float x,float z) => new Vector3(x,0,z);
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
