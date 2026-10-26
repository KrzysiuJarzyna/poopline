using UnityEngine;

namespace Poopline
{
    public sealed class SluicePuzzle : MonoBehaviour
    {
        public Transform valve, water, gate, discharge;
        public BoxCollider barrier;
        public Bounds chamber=new Bounds(new Vector3(34,1,32),new Vector3(11,4,11));
        public bool Solved {get;private set;}
        public bool Seen {get;private set;}
        public bool IsOpen => Solved && DrainProgress>=1;
        public float HoldProgress {get;private set;}
        public float DrainProgress {get;private set;}
        public bool InReach => game!=null && !game.InMainMenu && !game.Player.Climbing && Vector3.Distance(game.Player.transform.position+Vector3.up*1.1f,valve.position)<1.75f;
        private PooplineGame game;
        private Vector3 waterStart,gateStart;
        private Quaternion valveStart;
        private bool initialized;
        public void Initialize(PooplineGame owner)
        {
            game=owner;
            if(initialized) return;
            waterStart=water.localPosition;gateStart=gate.localPosition;valveStart=valve.localRotation;initialized=true;
        }
        public void Restore(bool solved,bool seen=false)
        {
            Solved=solved;Seen=seen;HoldProgress=solved?1:0;DrainProgress=solved?1:0;Apply();
        }
        private void Update()
        {
            if(game==null || game.Frozen) return;
            if(!Seen && Vector3.Distance(game.Player.transform.position,new Vector3(27,0,32))<5)
            {Seen=true;game.ShowNotice("Zalana komora. Znajdź zawór przy pompach.",5);game.SaveProgress();}
            if(!Solved)
            {
                if(InReach && game.Input.InteractHeld)
                {
                    if(HoldProgress==0) game.Sound.PlayValve();
                    HoldProgress=Mathf.Min(1,HoldProgress+Time.deltaTime/1.5f);
                    if(HoldProgress>=1)
                    {
                        Solved=true;game.ShowNotice("Odpływ otwarty. Woda opuszcza komorę.",5);
                        game.Sound.PlayDrain();game.SaveProgress(true);
                    }
                }
                else HoldProgress=Mathf.Max(0,HoldProgress-Time.deltaTime*2);
            }
            else if(DrainProgress<1)
            {
                DrainProgress=Mathf.Min(1,DrainProgress+Time.deltaTime/4.5f);
                if(DrainProgress>=1) {game.ShowNotice("Komora jest przechodnia. Droga dalej stoi otworem.",5);game.SaveProgress(true);}
            }
            Apply();
        }
        private void Apply()
        {
            if(!initialized) return;
            float t=Mathf.SmoothStep(0,1,DrainProgress);
            water.localPosition=waterStart+Vector3.down*(1.55f*t);
            gate.localPosition=gateStart+Vector3.up*(3.1f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.62f,1,DrainProgress)));
            // Keep the physical barrier until the water and portcullis have finished moving.
            barrier.enabled=!IsOpen;
            valve.localRotation=valveStart*Quaternion.AngleAxis(-HoldProgress*270,Vector3.forward);
            discharge.gameObject.SetActive(Solved && !IsOpen);
        }
    }
}
