using UnityEngine;
using UnityEngine.EventSystems;

namespace Poopline
{
    // Jump on touch-down, not on release; independent pointers still reach Pause.
    public sealed class JumpButton : MonoBehaviour, IPointerDownHandler
    {
        public PooplineGame Game { get; set; }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && Game.State.Phase == RunPhase.Playing)
                Game.PrimaryAction();
        }
    }
}
