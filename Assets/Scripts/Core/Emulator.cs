using Chip8.Sound;
using System;
using System.IO;
using UnityEngine;

namespace Chip8.Core
{
    public class Emulator : MonoBehaviour
    {
        [SerializeField] private Audio _audio;
        [SerializeField] private string _romFileName;

        private const float CPU_FREQUENCY = 700f;
        private const float TIMER_FREQUENCY = 60f;
        private const int MAX_INSTRUCTIONS_PER_FRAME = 100;

        public Display Display { get; private set; }

        private Memory _memory;
        private Keyboard _keyboard;
        private Processor _processor;

        private float _cpuAccumulator;
        private float _timerAccumulator;


        private void Awake()
        {
            _memory = new();
            Display = new();
            _keyboard = new();
            _processor = new Processor(_memory, Display, _keyboard);
        }

        private void Start()
        {
            LoadRom(_romFileName);
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;

            UpdateProcessor(deltaTime);
            UpdateAudio();
            UpdateTimers(deltaTime);
        }

        private void UpdateAudio()
        {
            if (_processor.IsSoundActive)
            {
                _audio.Play();
            }
            else
            {
                _audio.Stop();
            }
        }

        public void LoadRom(string fileName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Roms", fileName);

            if (!File.Exists(path))
            {
                Debug.LogError($"ROM не найден: {path}");
                return;
            }

            byte[] rom = File.ReadAllBytes(path);

            Debug.Log($"ROM загружен: {fileName}, размер: {rom.Length} bytes");

            LoadProgram(rom);
        }

        public void LoadProgram(byte[] program)
        {
            int maxProgramSize = Memory.MEMORY_SIZE - Processor.PROGRAM_START_ADDRESS;

            if (program.Length > maxProgramSize)
            {
                throw new ArgumentException($"ROM слишком большой: {program.Length} bytes. Максимум: {maxProgramSize} bytes.");
            }

            _memory.Reset();
            _processor.Reset();
            _keyboard.Reset();
            Display.Clear();

            _cpuAccumulator = 0;
            _timerAccumulator = 0;

            for (int i = 0; i < program.Length; i++)
            {
                _memory.Write((ushort)(Processor.PROGRAM_START_ADDRESS + i), program[i]);
            }
            _memory.Write(0x1FF, 1);
        }

        public void DownKey(KeyCode chip8Key)
        {
            _keyboard.SetKey((byte)chip8Key, true);
        }

        public void UpKey(KeyCode chip8Key)
        {
            _keyboard.SetKey((byte)chip8Key, false);
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
                _processor.Tick60Hz();
                _timerAccumulator -= timerInterval;
            }
        }
    }
}