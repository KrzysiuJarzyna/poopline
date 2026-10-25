using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Poopline
{
    public sealed class DirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public PooplineGame Game { get; set; }
        public float Direction { get; set; }
        private readonly HashSet<int> pointers = new HashSet<int>();

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || Game.State.Phase != RunPhase.Playing) return;
            pointers.Add(data.pointerId);
            Game.Input.HoldDirection(data.pointerId, Direction);
        }

        public void OnPointerUp(PointerEventData data) { Release(data.pointerId); }
        public void OnPointerExit(PointerEventData data) { Release(data.pointerId); }
        private void Release(int id) { pointers.Remove(id); Game.Input.ReleaseDirection(id); }
        private void OnDisable()
        {
            if (Game != null && Game.Input != null)
                foreach (int id in pointers) Game.Input.ReleaseDirection(id);
            pointers.Clear();
        }
    }
}
