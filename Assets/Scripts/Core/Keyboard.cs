using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chip8.Core
{
    public class Keyboard
    {
        private const int KEYS_COUNT = 16;
        private readonly bool[] _keys = new bool[KEYS_COUNT];
        private readonly Queue<byte> _releasedKeys = new Queue<byte>();

        public bool IsPressed(int key)
        {
            if (key < 0 || key >= KEYS_COUNT)
            {
                return false;
            }

            return _keys[key];
        }

        public void SetKey(byte key, bool pressed)
        {
            if (key >= KEYS_COUNT)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            bool wasPressed = _keys[key];

            if (wasPressed == pressed)
            {
                return;
            }

            _keys[key] = pressed;

            if (wasPressed && !pressed)
            {
                _releasedKeys.Enqueue(key);
            }
        }


        public bool TryGetReleasedKey(out byte key)
        {
            if (_releasedKeys.Count > 0)
            {
                key = _releasedKeys.Dequeue();
                return true;
            }

            key = 0;
            return false;
        }

        public void ClearReleasedKeys()
        {
            _releasedKeys.Clear();
        }

        public void Reset()
        {
            Array.Clear(_keys, 0, _keys.Length);
            ClearReleasedKeys();
        }
    }
}