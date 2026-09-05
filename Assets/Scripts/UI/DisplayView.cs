using Chip8.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Chip8.UI
{
    public class DisplayView : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private RawImage _displayImage;
        [SerializeField] private Color _pixelOffColor = Color.black;
        [SerializeField] private Color _pixelOnColor = Color.white;
        [Space]
        [SerializeField] private Emulator _emulator;
        private Texture2D _displayTexture;

        private void Awake()
        {
            _displayTexture = new Texture2D(
                Chip8.Core.Display.WIDTH, 
                Chip8.Core.Display.HEIGHT, 
                TextureFormat.RGBA32, 
                false
            );

            _displayTexture.filterMode = FilterMode.Point;
            _displayTexture.wrapMode = TextureWrapMode.Clamp;
            _displayImage.texture = _displayTexture;
        }

        private void Update()
        {
            RenderDisplay();
        }

        private void RenderDisplay()
        {
            for (int y = 0; y < Chip8.Core.Display.HEIGHT; y++)
            {
                for (int x = 0; x < Chip8.Core.Display.WIDTH; x++)
                {
                    Color color = _emulator.Display.GetPixel(x, y) ? _pixelOnColor : _pixelOffColor;

                    _displayTexture.SetPixel(x, Chip8.Core.Display.HEIGHT - 1 - y, color);
                }
            }

            _displayTexture.Apply();
        }
    }
}