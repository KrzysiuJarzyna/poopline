using UnityEngine;

namespace Poopline
{
    public enum ThoughtCue { EnterCanal, EnterBridges, FloodBlocked, DrainIdea, ValveIdea, ValveFound, DrainStarted, PassageOpen, RatEscape }

    // Stable cue + variant identifiers can later be paired with the author's recorded voice.
    // Only one thought is spoken at a time; obsolete puzzle hints are never queued.
    public sealed class PlayerThoughts
    {
        public string LineId {get;private set;}="";
        public string Text {get;private set;}="";
        public float Remaining {get;private set;}
        public float Duration {get;private set;}
        public bool Speaking=>Remaining>0;
        public float Opacity=>Mathf.Min(1,Remaining/.35f,(Duration-Remaining)/.2f);

        public void Say(ThoughtCue cue)
        {
            string[] variants=Lines(cue);
            int variant=Random.Range(0,variants.Length);
            LineId=cue.ToString()+"."+variant;
            Text=variants[variant];
            Duration=Mathf.Clamp(2+Text.Length*.065f,5.5f,9);
            Remaining=Duration;
        }
        public void Tick(float elapsed) {Remaining=Mathf.Max(0,Remaining-elapsed);}
        public void Clear() {Remaining=0;Text="";LineId="";}
        public static string[] Lines(ThoughtCue cue)
        {
            switch(cue)
            {
                case ThoughtCue.EnterCanal: return new[]{"No dobra. Zobaczmy, dokąd prowadzi ten kanał."};
                case ThoughtCue.EnterBridges: return new[]{"Słyszę coś między rurami. Lepiej się rozejrzę."};
                case ThoughtCue.FloodBlocked: return new[]{"Kurde, przejście jest zalane. Tędy nie dam rady.","Tędy nie przejdę. Za dużo wody.","No pięknie. Woda odcina mi drogę."};
                case ThoughtCue.DrainIdea: return new[]{"Może da się jakoś spuścić tę wodę?","Ta woda musi mieć jakiś odpływ. Tylko gdzie?"};
                case ThoughtCue.ValveIdea: return new[]{"Ta rura… może prowadzi do zaworu odpływu? Sprawdzę boczny korytarz.","Może w bocznym korytarzu znajdę zawór? Mógłbym nim spuścić wodę."};
                case ThoughtCue.ValveFound: return new[]{"Jest zawór. Sprawdzę, czy otwiera odpływ."};
                case ThoughtCue.DrainStarted: return new[]{"Działa. Słyszę, jak woda schodzi."};
                case ThoughtCue.PassageOpen: return new[]{"Woda zeszła. Mogę wracać do przejścia."};
                case ThoughtCue.RatEscape: return new[]{"Uff. Następnym razem muszę ominąć tego szczura."};
                default: return new[]{"…"};
            }
        }
    }
}
