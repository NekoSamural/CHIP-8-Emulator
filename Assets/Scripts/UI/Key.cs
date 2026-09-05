using Chip8.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chip8.UI
{
    [RequireComponent(typeof(Image))]
    public class Key : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Emulator _emulator;
        [Space]
        [SerializeField] private Chip8.Core.KeyCode _chip8KeyCode;
        [Space]
        [SerializeField] private Color _idle;
        [SerializeField] private Color _pressed;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.color = _idle;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _emulator.DownKey(_chip8KeyCode);
            _image.color = _pressed;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _emulator.UpKey(_chip8KeyCode);
            _image.color = _idle;
        }
    }
}

