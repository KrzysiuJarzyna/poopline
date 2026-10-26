using UnityEngine;

namespace Poopline
{
    public sealed class SluicePuzzle : MonoBehaviour
    {
        public Transform valve, water, gate, discharge;
        public BoxCollider barrier;
        public Bounds chamber=new Bounds(new Vector3(32,1,41),new Vector3(16,4,14));
        // Both entry routes and the only side-passage entrance meet in this dry vestibule.
        public Bounds approach=new Bounds(new Vector3(26,1,31.5f),new Vector3(4.8f,4,5));
        public Vector3 valveStand=new Vector3(37,0,29);
        public bool Solved {get;private set;}
        public bool Seen {get;private set;}
        public bool IsOpen => Solved && DrainProgress>=1;
        public float HoldProgress {get;private set;}
        public float DrainProgress {get;private set;}
        public const float DrainHintDelay=45, ValveHintDelay=60;
        public int HintStep {get;private set;}
        public float HintElapsed {get;private set;}
        public bool ValveFound {get;private set;}
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
        public void Restore(bool solved,bool seen=false,int hintStep=0,float hintElapsed=0,bool valveFound=false)
        {
            Solved=solved;Seen=seen;HoldProgress=solved?1:0;DrainProgress=solved?1:0;
            HintStep=seen?Mathf.Clamp(hintStep,1,3):0;
            HintElapsed=seen?Mathf.Clamp(hintElapsed,0,ValveHintDelay):0;
            ValveFound=seen && valveFound;Apply();
        }
        private void Update()
        {
            if(game==null || game.Frozen) return;
            if(!Seen && approach.Contains(game.Player.transform.position))
            {
                Seen=true;HintStep=1;HintElapsed=0;
                game.Thoughts.Say(ThoughtCue.FloodBlocked);game.SaveProgress();
            }
            if(!Solved)
            {
                if(Seen && InReach && !ValveFound)
                {
                    ValveFound=true;game.Thoughts.Say(ThoughtCue.ValveFound);game.SaveProgress();
                }
                if(Seen && !ValveFound && HintStep<3)
                {
                    float delay=HintStep==1?DrainHintDelay:ValveHintDelay;
                    HintElapsed=Mathf.Min(delay,HintElapsed+Time.deltaTime);
                    if(HintElapsed>=delay && !game.Thoughts.Speaking)
                    {
                        HintStep++;HintElapsed=0;
                        game.Thoughts.Say(HintStep==2?ThoughtCue.DrainIdea:ThoughtCue.ValveIdea);
                        game.SaveProgress();
                    }
                }
                if(Seen && InReach && game.Input.InteractHeld)
                {
                    if(HoldProgress==0) game.Sound.PlayValve();
                    HoldProgress=Mathf.Min(1,HoldProgress+Time.deltaTime/1.5f);
                    if(HoldProgress>=1)
                    {
                        Solved=true;game.Thoughts.Say(ThoughtCue.DrainStarted);
                        game.Sound.PlayDrain();game.SaveProgress(true);
                    }
                }
                else HoldProgress=Mathf.Max(0,HoldProgress-Time.deltaTime*2);
            }
            else if(DrainProgress<1)
            {
                DrainProgress=Mathf.Min(1,DrainProgress+Time.deltaTime/4.5f);
                if(DrainProgress>=1) {game.Thoughts.Say(ThoughtCue.PassageOpen);game.SaveProgress(true);}
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
