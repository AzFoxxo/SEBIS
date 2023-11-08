# SEBIS Assembly Specification

This document describes Standard SEBIS Assembly version `1.0.0`. It does not include any non-standard additions or implementation specific details. See also [Instruction Set](instruction-set.md)

## Comments
SEBIS assembly has support comments at the start of a line using the `;` (semi-colon) to denote one or post code comments e.g. `NOP ; comment`.

## Instruction Case Insensitivity
Each instruction in SEBIS should be case insensitive so `NOP`, `nop` or any variant between such as `NoP` should all be accepted as valid.

## Instruction Argumentation
Many instructions in SEBIS assembly require additional operand to be provided such as loading a value into a register, etc. Depending on the type of data, different syntax must be used to specify such data to ensure the assembler can produce the correct machine code.

Each instruction can take a max of only **1** instruction so more complex instructions such as `ABC` (add) take the values stored within `A` and `B` and add the results and store those within `C`.

### 16 bit memory address mode operands
For specifying a memory address as an operand, the syntax is `OPCODE $16_BIT_HEX_CODE`

### 8 bit inline-constant address mode operands
All 8 bit constants can be supplied as decimal by writing the number e.g. `OPCODE 39`, by hex value using `0x` e.g. `OPCODE 0x80` or by byte using `0b` e.g. `OPCODE 0x80`


## Labels
Labels can be created within code files using the syntax `:` followed by the name of the label which must be written with only letters `A`-`Z` and `_`. Labels unlike opcodes, are case sensitive to allow both snake_case and camelCase.

When labels are provided to the jump instructions, the label name is written after the opcode e.g. `JNZ my_label`.

### Required labels
SEBIS requires several labels to be present within your file, these cannot be excluded else the assembly process will fail. These sections are as follows:
- `:init` - this section contains start of your program, this section is where the machine will jump following the meta data written in the ROM header pointing to this address.
- `:panic` - this section should contain code that you wish to be executed if a critical error occurs and the machine panics, invalid code can be executed without causing a panic but in case of severe failure e.g. memory corruption, this code is jumped to.
- `:end` - this section is simple, it contains any code that must be executed when the machine has been told to halt - `HLT`.