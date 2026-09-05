using Chip8.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chip8.UI
{
    public class Key : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Emulator _emulator;
        [SerializeField] private Chip8.Core.KeyCode _chip8KeyCode;

        public void OnPointerDown(PointerEventData eventData)
        {
            _emulator.DownKey(_chip8KeyCode);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _emulator.UpKey(_chip8KeyCode);
        }
    }
}

