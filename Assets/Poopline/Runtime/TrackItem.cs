using UnityEngine;

namespace Poopline
{
    public enum TrackItemKind { Hazard, Coin, Finish }

    public sealed class TrackItem : MonoBehaviour
    {
        public TrackItemKind Kind { get; set; }
        public void Touch(PooplineGame game)
        {
            if (!gameObject.activeSelf || game.State.Phase != RunPhase.Playing) return;
            switch (Kind)
            {
                case TrackItemKind.Coin:
                    game.CollectCoin();
                    gameObject.SetActive(false);
                    break;
                case TrackItemKind.Hazard: game.Finish(false); break;
                case TrackItemKind.Finish: game.Finish(true); break;
            }
        }
    }
}
