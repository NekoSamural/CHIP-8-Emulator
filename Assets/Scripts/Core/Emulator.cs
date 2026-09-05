using Chip8.UI;
using UnityEngine;

namespace Chip8.Core
{
    public class Emulator : MonoBehaviour
    {
        private const float CPU_FREQUENCY = 700f;
        private const float TIMER_FREQUENCY = 60f;
        private const int MAX_INSTRUCTIONS_PER_FRAME = 100;

        public Display Display { get; private set; }

        private Memory _memory;
        private Keyboard _keyboard;
        private Processor _processor;

        private float _cpuAccumulator;
        private float _timerAccumulator;


        public void Awake()
        {
            _memory = new();
            Display = new();
            _keyboard = new();
            _processor = new Processor(_memory, Display, _keyboard);
        }

        private void Start()
        {
            LoadProgram(new byte[]
            {
                0x63, 0x0A, // V3 = 0xA
                0xF3, 0x29, // I = адрес спрайта A

                0x61, 0x0A, // V1 = 10
                0x62, 0x05, // V2 = 5

                0xD1, 0x25, // Draw(V1, V2, 5)

                0x12, 0x0A  // Jump 0x20A
            });
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;

            UpdateProcessor(deltaTime);
            UpdateTimers(deltaTime);
            Debug.Log($"0x{_processor.GetRegister(0x3):X2}");
        }

        private void UpdateProcessor(float deltaTime)
        {
            float instructionInterval = 1f / CPU_FREQUENCY;
            _cpuAccumulator += deltaTime;
            int executed = 0;

            while (_cpuAccumulator >= instructionInterval && executed < MAX_INSTRUCTIONS_PER_FRAME)
            {
                _processor.Step();
                _cpuAccumulator -= instructionInterval;
                executed++;
            }
        }

        private void UpdateTimers(float deltaTime)
        {
            float timerInterval = 1f / TIMER_FREQUENCY;

            _timerAccumulator += deltaTime;

            while (_timerAccumulator >= timerInterval)
            {
                _processor.TickTimers();
                _timerAccumulator -= timerInterval;
            }
        }

        public void LoadProgram(byte[] program)
        {
            _memory.Reset();
            _processor.Reset();

            for (int i = 0; i < program.Length; i++)
            {
                _memory.Write((ushort)(Processor.PROGRAM_START_ADDRESS + i), program[i]);
            }
        }

        public void DownKey(KeyCode chip8Key)
        {
            _keyboard.SetKey((byte)chip8Key, true);
        }

        public void UpKey(KeyCode chip8Key)
        {
            _keyboard.SetKey((byte)chip8Key, false);
        }
    }
}