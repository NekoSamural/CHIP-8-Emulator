using System;
using UnityEngine;

namespace Chip8.Core
{
    public class Display : MonoBehaviour
    {
        public const int WIDTH = 64;
        public const int HEIGHT = 32;

        private readonly bool[,] _pixels = new bool[WIDTH, HEIGHT];

        public void Clear()
        {
            Array.Clear(_pixels, 0, _pixels.Length);
        }

        public bool TogglePixel(int x, int y)
        {
            bool wasEnabled = _pixels[x, y];
            _pixels[x, y] = !wasEnabled;
            return wasEnabled;
        }

        public bool GetPixel(int x, int y)
        {
            return _pixels[x, y];
        }
    }
}