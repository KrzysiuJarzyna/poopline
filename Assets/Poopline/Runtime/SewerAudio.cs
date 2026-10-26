using UnityEngine;

namespace Poopline
{
    // Original, temporary synthesized effects. Replace clips without changing gameplay code.
    public sealed class SewerAudio : MonoBehaviour
    {
        public AudioClip ambienceClip,dripClip,valveClip,drainClip,stepClip,clickClip;
        public AudioSource AmbienceSource {get;private set;}
        public AudioSource EffectsSource {get;private set;}
        private AudioSource drops,ui;
        private bool effectsPaused;
        private PooplineGame game;
        private float nextDrip,distance;
        private Vector3 lastPosition;
        public void Initialize(PooplineGame owner)
        {
            game=owner;
            AmbienceSource=Source("Ambience");EffectsSource=Source("Interactions");drops=Source("Dripping water");ui=Source("Menu clicks");
            AmbienceSource.clip=ambienceClip;AmbienceSource.loop=true;AmbienceSource.volume=0;AmbienceSource.Play();
            lastPosition=game.Player.transform.position;
        }
        private AudioSource Source(string label)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);
            var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;return source;
        }
        private void Update()
        {
            if(game==null) return;
            float volume=game.Settings.Volume;
            float mood=game.InMainMenu?.8f:game.Frozen?.24f:game.Player.InWater?1:.7f;
            AmbienceSource.volume=volume*game.Settings.Ambience*mood;
            drops.volume=volume*game.Settings.Ambience*.36f;
            EffectsSource.volume=volume*.65f;ui.volume=volume*.65f;
            if(game.Frozen!=effectsPaused)
            {
                effectsPaused=game.Frozen;
                if(effectsPaused) EffectsSource.Pause();else EffectsSource.UnPause();
            }
            if(game.InMainMenu) EffectsSource.Stop();
            if((game.InMainMenu || !game.Frozen) && Time.unscaledTime>nextDrip)
            {drops.panStereo=Random.Range(-.65f,.65f);drops.pitch=Random.Range(.8f,1.15f);drops.PlayOneShot(dripClip);nextDrip=Time.unscaledTime+Random.Range(1.1f,2.6f);}
            Vector3 p=game.Player.transform.position;
            if(!game.Frozen && !game.Player.Climbing)
            {
                float step=Vector3.Distance(p,lastPosition);
                if(step<.5f) distance+=step;
                if(distance>.95f) {EffectsSource.pitch=Random.Range(.86f,1.06f);EffectsSource.PlayOneShot(game.Player.InWater?dripClip:stepClip,.38f);distance=0;}
            }
            else distance=0;
            lastPosition=p;
        }
        public void PlayValve() {EffectsSource.pitch=1;EffectsSource.PlayOneShot(valveClip,.55f);}
        public void PlayDrain() {EffectsSource.pitch=1;EffectsSource.PlayOneShot(drainClip,.8f);}
        public void PlayClick() {ui.PlayOneShot(clickClip,.45f);}
    }
}
