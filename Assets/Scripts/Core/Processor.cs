using System;

namespace Chip8.Core
{
    public class Processor
    {
        public const ushort PROGRAM_START_ADDRESS = 0x200;

        private readonly Memory _memory;
        private readonly Display _display;
        private readonly Keyboard _keyboard;
        private readonly Random _random;

        private readonly byte[] _registers = new byte[16];
        private ushort _indexRegister;

        private readonly ushort[] _stack = new ushort[16];
        private byte _stackPointer;

        private ushort _programCounter;
        private int _waitingForKeyRegister = -1;

        private byte _delayTimer;
        private byte _soundTimer;

        public Processor(Memory memory, Display display, Keyboard keyboard)
        {
            _memory = memory;
            _display = display;
            _keyboard = keyboard;
            _random = new();
            Reset();
        }

        public void Reset()
        {
            Array.Clear(_registers, 0, _registers.Length);
            _indexRegister = 0;

            Array.Clear(_stack, 0, _stack.Length);
            _stackPointer = 0;

            _programCounter = PROGRAM_START_ADDRESS;
            _waitingForKeyRegister = -1;

            _delayTimer = 0;
            _soundTimer = 0;
        }

        public void Step()
        {
            if (_waitingForKeyRegister >= 0)
            {
                TryCompleteKeyWait();
                return;
            }

            ushort opcode = Fetch();
            Execute(opcode);
        }

        public void TickTimers()
        {
            if (_delayTimer > 0)
            {
                _delayTimer--;
            }

            if (_soundTimer > 0)
            {
                _soundTimer--;
            }
        }

        public byte GetRegister(int index)
        {
            return _registers[index];
        }

        private ushort Fetch()
        {
            byte highByte = _memory.Read(_programCounter);
            byte lowByte = _memory.Read((ushort)(_programCounter + 1));

            ushort opcode = (ushort)(
                (highByte << 8) |
                lowByte
            );

            _programCounter += 2;

            return opcode;
        }

        private void Execute(ushort opcode)
        {
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    ExecuteSystemOperation(opcode);
                    break;

                case 0x1000:
                    ExecuteJumpOperation(opcode);
                    break;

                case 0x2000:
                    ExecuteCall(opcode);
                    break;

                case 0x3000:
                    ExecuteSkipIfEqualImmediate(opcode);
                    break;

                case 0x4000:
                    ExecuteSkipIfNotEqualImmediate(opcode);
                    break;

                case 0x5000:
                    if ((opcode & 0x000F) != 0)
                    {
                        throw new NotSupportedException($"Неизвестный opcode: 0x{opcode:X4}");
                    }
                    ExecuteSkipIfRegistersEqual(opcode);
                    break;

                case 0x6000:
                    ExecuteLoadImmediate(opcode);
                    break;

                case 0x7000:
                    ExecuteAddImmediate(opcode);
                    break;

                case 0x8000:
                    ExecuteRegisterOperation(opcode);
                    break;

                case 0x9000:
                    if ((opcode & 0x000F) != 0)
                    {
                        throw new NotSupportedException($"Неизвестный opcode: 0x{opcode:X4}");
                    }
                    ExecuteSkipIfNotRegistersEqual(opcode);
                    break;

                case 0xA000:
                    ExecuteLoadIndex(opcode);
                    break;

                case 0xB000:
                    ExecuteJumpWithOffset(opcode);
                    break;

                case 0xC000:
                    ExecuteRandom(opcode);
                    break;

                case 0xD000:
                    ExecuteDraw(opcode);
                    break;

                case 0xF000:
                    ExecuteMiscOperation(opcode);
                    break;

                case 0xE000:
                    ExecuteKeyOperation(opcode);
                    break;

                default:
                    throw new NotSupportedException($"Неизвестный opcode: 0x{opcode:X4}");
            }
        }

        private void ExecuteLoadImmediate(ushort opcode)
        {
            int register = (opcode & 0x0F00) >> 8;
            byte value = (byte)(opcode & 0x00FF);

            _registers[register] = value;
        }

        private void ExecuteAddImmediate(ushort opcode)
        {
            int register = (opcode & 0x0F00) >> 8;
            byte value = (byte)(opcode & 0x00FF);

            _registers[register] = unchecked((byte)(_registers[register] + value));
        }

        private void ExecuteRegisterOperation(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;
            int y = (opcode & 0x00F0) >> 4;
            switch (opcode & 0x000F)
            {
                case 0x0000:
                    _registers[x] = _registers[y];
                    break;

                case 0x0001:
                    _registers[x] |= _registers[y];
                    break;

                case 0x0002:
                    _registers[x] &= _registers[y];
                    break;

                case 0x0003:
                    _registers[x] ^= _registers[y];
                    break;

                case 0x0004:
                {
                    byte vx = _registers[x];
                    byte vy = _registers[y];
                    int result = vx + vy;
                    byte carry = (byte)(result > 0xFF ? 1 : 0);
                    _registers[x] = (byte)result;
                    _registers[0xF] = carry;
                    break;
                }
                case 0x0005:
                {
                    byte vx = _registers[x];
                    byte vy = _registers[y];
                    byte notBorrow = (byte)(vx >= vy ? 1 : 0);
                    byte result = unchecked((byte)(vx - vy));
                    _registers[x] = result;
                    _registers[0xF] = notBorrow;
                    break;
                }
                case 0x0006:
                {
                    byte vy = _registers[y];
                    byte shiftedBit = (byte)(vy & 0x01);
                    byte result = (byte)(vy >> 1);
                    _registers[x] = result;
                    _registers[0xF] = shiftedBit;
                    break;
                }
                case 0x0007:
                {
                    byte vx = _registers[x];
                    byte vy = _registers[y];
                    byte notBorrow = (byte)(vy >= vx ? 1 : 0);
                    byte result = unchecked((byte)(vy - vx));
                    _registers[x] = result;
                    _registers[0xF] = notBorrow;
                    break;
                }
                case 0x000E:
                {
                    byte vy = _registers[y];
                    byte shiftedBit = (byte)((vy & 0x80) >> 7);
                    byte result = unchecked((byte)(vy << 1));
                    _registers[x] = result;
                    _registers[0xF] = shiftedBit;
                    break;
                }
                default:
                    throw new NotSupportedException($"Неизвестный opcode: 0x{opcode:X4}");
            }
        }

        private void ExecuteJumpOperation(ushort opcode)
        {
            ushort address = (ushort)(opcode & 0x0FFF);
            _programCounter = address;
        }

        private void ExecuteSkipIfEqualImmediate(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;
            byte value = (byte)(opcode & 0x00FF);
            if (_registers[x] == value)
            {
                _programCounter += 2;
            }
        }

        private void ExecuteSkipIfNotEqualImmediate(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;
            byte value = (byte)(opcode & 0x00FF);
            if (_registers[x] != value)
            {
                _programCounter += 2;
            }
        }

        private void ExecuteSkipIfRegistersEqual(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;
            int y = (opcode & 0x00F0) >> 4;

            if (_registers[x] == _registers[y])
            {
                _programCounter += 2;
            }
        }

        private void ExecuteSkipIfNotRegistersEqual(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;
            int y = (opcode & 0x00F0) >> 4;

            if (_registers[x] != _registers[y])
            {
                _programCounter += 2;
            }
        }

        private void ExecuteLoadIndex(ushort opcode)
        {
            ushort address = (ushort)(opcode & 0x0FFF);
            _indexRegister = address;
        }

        private void ExecuteMiscOperation(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;

            switch (opcode & 0x00FF)
            {

                case 0x0007:
                    _registers[x] = _delayTimer;
                    break;

                case 0x000A:
                    ExecuteWaitForKey(x);
                    break;

                case 0x0015:
                    _delayTimer = _registers[x];
                    break;

                case 0x0018:
                    _soundTimer = _registers[x];
                    break;

                case 0x001E:
                    _indexRegister += _registers[x];
                    break;

                case 0x0029:
                    ExecuteLoadFontAddress(x);
                    break;

                case 0x0033:
                    ExecuteStoreBcd(x);
                    break;

                case 0x0055:
                    ExecuteStoreRegisters(x);
                    break;

                case 0x0065:
                    ExecuteLoadRegisters(x);
                    break;

                default:
                    throw new NotSupportedException($"Неизвестный opcode: 0x{opcode:X4}");
            }
        }

        private void ExecuteCall(ushort opcode)
        {
            if (_stackPointer >= _stack.Length)
            {
                throw new InvalidOperationException("Переполнение стека CHIP-8");
            }

            ushort address = (ushort)(opcode & 0x0FFF);
            _stack[_stackPointer] = _programCounter;
            _stackPointer++;

            _programCounter = address;
        }

        private void ExecuteSystemOperation(ushort opcode)
        {
            switch (opcode)
            {
                case 0x00E0:
                    _display.Clear();
                    break;

                case 0x00EE:
                    ExecuteReturn();
                    break;

                default:
                    throw new NotSupportedException($"Неизвестный opcode: 0x{opcode:X4}");
            }
        }

        private void ExecuteReturn()
        {
            if (_stackPointer == 0)
            {
                throw new InvalidOperationException("Попытка RETURN при пустом стеке CHIP-8");
            }

            _stackPointer--;
            _programCounter = _stack[_stackPointer];
        }

        private void ExecuteJumpWithOffset(ushort opcode)
        {
            ushort address = (ushort)(opcode & 0x0FFF);
            _programCounter = (ushort)(address + _registers[0]);
        }

        private void ExecuteRandom(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;
            byte mask = (byte)(opcode & 0x00FF);
            byte randomValue = (byte)_random.Next(0, 256);
            _registers[x] = (byte)(randomValue & mask);
        }

        private void ExecuteDraw(ushort opcode)
        {
            int xRegister = (opcode & 0x0F00) >> 8;
            int yRegister = (opcode & 0x00F0) >> 4;
            int height = opcode & 0x000F;

            int startX = _registers[xRegister] % Display.WIDTH;
            int startY = _registers[yRegister] % Display.HEIGHT;

            _registers[0xF] = 0;

            for (int row = 0; row < height; row++)
            {
                int screenY = startY + row;

                if (screenY >= Display.HEIGHT)
                {
                    break;
                }

                byte spriteByte = _memory.Read((ushort)(_indexRegister + row));

                for (int column = 0; column < 8; column++)
                {
                    int screenX = startX + column;

                    if (screenX >= Display.WIDTH)
                    {
                        break;
                    }

                    int mask = 0x80 >> column;

                    if ((spriteByte & mask) == 0)
                    {
                        continue;
                    }

                    bool collision = _display.TogglePixel(screenX, screenY);

                    if (collision)
                    {
                        _registers[0xF] = 1;
                    }
                }
            }
        }

        private void ExecuteStoreRegisters(int x)
        {
            for (int i = 0; i <= x; i++)
            {
                _memory.Write((ushort)(_indexRegister + i), _registers[i]);
            }

            _indexRegister += (ushort)(x + 1);
        }

        private void ExecuteLoadRegisters(int x)
        {
            for (int i = 0; i <= x; i++)
            {
                _registers[i] = _memory.Read((ushort)(_indexRegister + i));
            }

            _indexRegister += (ushort)(x + 1);
        }

        private void ExecuteStoreBcd(int x)
        {
            byte value = _registers[x];
            _memory.Write(_indexRegister, (byte)(value / 100));
            _memory.Write((ushort)(_indexRegister + 1), (byte)((value / 10) % 10));
            _memory.Write((ushort)(_indexRegister + 2), (byte)(value % 10));
        }

        private void ExecuteLoadFontAddress(int x)
        {
            byte digit = (byte)(_registers[x] & 0x0F);

            _indexRegister = (ushort)(Memory.FONT_START_ADDRESS + digit * 5);
        }

        private void ExecuteKeyOperation(ushort opcode)
        {
            int x = (opcode & 0x0F00) >> 8;

            switch (opcode & 0x00FF)
            {
                case 0x009E:
                    if (_keyboard.IsPressed(_registers[x]))
                    {
                        _programCounter += 2;
                    }
                    break;

                case 0x00A1:
                    if (!_keyboard.IsPressed(_registers[x]))
                    {
                        _programCounter += 2;
                    }
                    break;

                default:
                    throw new NotSupportedException($"Неизвестный opcode: 0x{opcode:X4}");
            }
        }

        private void ExecuteWaitForKey(int x)
        {
            _keyboard.ClearReleasedKeys();
            _waitingForKeyRegister = x;
        }

        private void TryCompleteKeyWait()
        {
            if (!_keyboard.TryGetReleasedKey(out byte key))
            {
                return;
            }

            _registers[_waitingForKeyRegister] = key;
            _waitingForKeyRegister = -1;
        }
    }
}