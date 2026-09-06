CHIP-8 Emulator
A classic CHIP-8 emulator written from scratch in C# with Unity used as the front-end.
This project was built primarily as a learning exercise to understand how interpreters and emulators work at a low level: opcodes, registers, memory, stacks, timers, input, graphics, timing, and compatibility quirks.

Features
Classic CHIP-8 instruction set
4 KB memory
16 general-purpose registers (V0–VF)
I index register
Program Counter and 16-level stack
64×32 monochrome display
XOR sprite drawing with collision detection
16-key CHIP-8 keypad
Delay timer at 60 Hz
Sound timer at 60 Hz
Generated square-wave beep
ROM loading from Unity StreamingAssets
Original CHIP-8 compatibility behavior

Architecture
The emulator core is kept separate from Unity-specific rendering and audio logic.

The core classes do not need to know how Unity renders textures or plays audio. Unity is responsible for presenting the framebuffer, forwarding input, loading ROM files, and playing the sound generated while the CHIP-8 sound timer is active.

Instruction Set
The emulator implements the classic CHIP-8 instruction set, including:
00E0        CLS
00EE        RET
1NNN        JP addr
2NNN        CALL addr
3XNN        SE VX, byte
4XNN        SNE VX, byte
5XY0        SE VX, VY
6XNN        LD VX, byte
7XNN        ADD VX, byte
8XY0        LD VX, VY
8XY1        OR VX, VY
8XY2        AND VX, VY
8XY3        XOR VX, VY
8XY4        ADD VX, VY
8XY5        SUB VX, VY
8XY6        SHR VX, VY
8XY7        SUBN VX, VY
8XYE        SHL VX, VY
9XY0        SNE VX, VY
ANNN        LD I, addr
BNNN        JP V0, addr
CXNN        RND VX, byte
DXYN        DRW VX, VY, nibble
EX9E        SKP VX
EXA1        SKNP VX
FX07        LD VX, DT
FX0A        LD VX, K
FX15        LD DT, VX
FX18        LD ST, VX
FX1E        ADD I, VX
FX29        LD F, VX
FX33        LD B, VX
FX55        LD [I], V0..VX
FX65        LD V0..VX, [I]

The obsolete 0NNN SYS instruction is intentionally not implemented.
The emulator targets classic/original CHIP-8 behavior.

This is an educational emulator designed to provide a solid foundation for learning more complex systems, such as the Game Boy, NES, and other real-world hardware. If you decide to use it as the basis for your emulator, you'll need to add custom ROM loading functions, additional pause functions, and so on. You'll also need to refine the interface and refactor the code.
