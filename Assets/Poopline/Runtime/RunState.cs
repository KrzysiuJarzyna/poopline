namespace Poopline
{
    public enum RunPhase { CityMap, Ready, Playing, Paused, Won, Lost }

    // The rules are independent of input devices, graphics and scene loading.
    public sealed class RunState
    {
        public RunPhase Phase { get; private set; } = RunPhase.CityMap;
        public int Coins { get; private set; }
        public float Seconds { get; private set; }

        public void Reset()
        {
            Phase = RunPhase.Ready;
            Coins = 0;
            Seconds = 0;
        }

        public void Start() { if (Phase == RunPhase.Ready) Phase = RunPhase.Playing; }
        public void ShowMap() { Reset(); Phase = RunPhase.CityMap; }
        public void Pause() { if (Phase == RunPhase.Playing) Phase = RunPhase.Paused; }
        public void Resume() { if (Phase == RunPhase.Paused) Phase = RunPhase.Playing; }
        public void Win() { if (Phase == RunPhase.Playing) Phase = RunPhase.Won; }
        public void Lose() { if (Phase == RunPhase.Playing) Phase = RunPhase.Lost; }
        public void Collect() { if (Phase == RunPhase.Playing) Coins++; }
        public void Tick(float seconds) { if (Phase == RunPhase.Playing && seconds > 0) Seconds += seconds; }
    }
}
