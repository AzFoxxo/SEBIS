# Simplified Eight Bit Instruction Set (aka. SEBIS)
SEBIS is a lightweight 8 bit instruction 

### CPU
The CPU is uses 16 bit memory addressing with a default instruction size of 8 bits operating at exactly 5MHz (single threaded only).

### Memory layout
The machine contains one single 64K memory bank which is used for both ROM and RAM, is is entirely read and writeable and contains memory mapped devices.

The first 63K is reserved for general use, this can be RAM or ROM and has no limitations on what it can be used for.

The last 1K of memory is reserved for memory mapped devices only, it is responsible for IO such as keyboard, console output, etc.

#### Memory mapped devices and their addresses

### Registers
The SEBIS spec outlines a number of registers as defined below:
<table>
    <tr>
        <th>Register</th>
        <th>A</th>
        <th>B</th>
        <th>C</th>
        <th>PC<th>
    </tr>
    <tr>
        <th>Size (bytes)</th>
        <td>1</td>
        <td>1</td>
        <td>1</td>
        <td>2<td>
    </tr>
    <tr>
        <th>Read</th>
        <td>True</td>
        <td>True</td>
        <td>True</td>
        <td>False<td>
    </tr>
    <tr>
        <th>True</th>
        <td>True</td>
        <td>False</td>
        <td>False</td>
        <td>False<td>
    </tr>
    <tr>
        <th>Use</th>
        <td>General purpose register</td>
        <td>General purpose register</td>
        <td>Results register</td>
        <td>Program pointer (in memory)<td>
    </tr>
</table>




### Instruction byte breakdown
The instruction set is highly simplified however due to the limitations of eight bit instruction sets, each instruction can be classified into different types (S, L and LL for 8 bit, 16 bit and 24 bit respectively).


#### Instruction space denotions
`S` - Short instruction space denotion (8 bit)
`L` - Long Instruction space denotion (16 bit)
`LL` - Long long Instruction space denotion (24 bit)

#### Read modes
- `0` - `NULL` - Instruction requires no read
- `1` - `REGISTER` - Instruction reads values from register
- `2` - `MEMORY` - Instruction reads values from a memory address
- `3` - `CONSTANT` - Read value from byte block of memory

`MEMORY` requires an additional two bytes to be sent after the instruction which data is read from for the instruction to be executed whereas `CONSTANT` only requires an additional byte to be provided. This allows a theoretical max size of 5 bytes per instruction.

#### Instruction bit breakdown
<table>
    <tr>
        <th>Bytes</th>
        <th>1-2</th>
        <th>3</th>
        <th>4-5</th>
        <th>6-8</th>
        <th>9-16</th>
        <th>17-24</th>
        <th>instruction+1-8</th>
        <th>instruction+9-16</th>
    </tr>
    <tr>
        <th>Use</th>
        <td>Instruction size (bytes)</td>
        <td>Flag</td>
        <td>Read mode</td>
        <td>S Opcode</td>
        <td>L Opcode</td>
        <td>LL Opcode</td>
        <td>Constant space/Memory address higher</td>
        <td>Memory address higher</td>
    </tr>
</table>