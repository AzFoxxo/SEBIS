# SEBIS Instruction Set

This document is the authoritative list of instructions supported in SEBIS.

Note: Some more complex instructions which both load data and operate on it will take multiple read modes thus must call sub-instructions taking up multiple clock cycles.

<table>
    <tr>
        <th>Opcode (ASM)</th>
        <th>Call code</th>
        <th>Space Denotions</th>
        <th>Operand</th>
        <th>Flag action</th>
        <th>Allowed read modes</th>
        <th>Note</th>
    </tr>
    <!-- NOP -->
    <tr>
        <td>NOP</td>
        <td>000</td>
        <td>S</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>Use an instruction cycle without changing value in registers</td>
    </tr>
    <!-- LDA -->
    <tr>
        <td>LDA</td>
        <td>001</td>
        <td>S</td>
        <td>MEMORY, CONSTANT</td>
        <td>NULL</td>
        <td>MEMORY, CONSTANT</td>
        <td>Load value from memory or inline constant to register A</td>
    </tr>
    <!-- LDB -->
    <tr>
        <td>LDB</td>
        <td>010</td>
        <td>S</td>
        <td>MEMORY, CONSTANT</td>
        <td>NULL</td>
        <td>MEMORY, CONSTANT</td>
        <td>Load value from memory or inline constant to register B</td>
    </tr>
    <!-- STA -->
    <tr>
        <td>STA</td>
        <td>011</td>
        <td>S</td>
        <td>MEMORY</td>
        <td>NULL</td>
        <td>REGISTER</td>
        <td>Store value of A register to memory address</td>
    </tr>
    <!-- STB -->
    <tr>
        <td>STB</td>
        <td>100</td>
        <td>S</td>
        <td>MEMORY</td>
        <td>NULL</td>
        <td>REGISTER</td>
        <td>Store value of B register to memory address</td>
    </tr>
    <!-- STC -->
    <tr>
        <td>STC</td>
        <td>101</td>
        <td>S</td>
        <td>MEMORY</td>
        <td>NULL</td>
        <td>REGISTER</td>
        <td>Store value of C register to memory address</td>
    </tr>
    <!-- SYSCALL -->
    <tr>
        <td>SYSCALL</td>
        <td>110</td>
        <td>S</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>REGISTER</td>
        <td>Performs predefined system calls (future-proofing only)</td>
    </tr>
    <!-- ABC -->
    <tr>
        <td>ABC</td>
        <td>111</td>
        <td>S</td>
        <td>NULL</td>
        <td>Perform subtraction (take B from A)</td>
        <td>REGISTER</td>
        <td>Add A to B and store results in C</td>
    </tr>
    <!-- MUL -->
    <tr>
        <td>MUL</td>
        <td>000 0000-0000</td>
        <td>L</td>
        <td>NULL</td>
        <td>Perform division (A divided by B)</td>
        <td>REGISTER</td>
        <td>Multiply A and B and store it in C</td>
    </tr>
    <!-- JMP -->
    <tr>
        <td>JMP</td>
        <td>000 0000-0001</td>
        <td>L</td>
        <td>MEMORY (label)</td>
        <td>NULL</td>
        <td>MEMORY</td>
        <td>Jump to memory address</td>
    </tr>
    <!-- JZ -->
    <tr>
        <td>JZ</td>
        <td>000 0000-0010</td>
        <td>L</td>
        <td>MEMORY (label)</td>
        <td>NULL</td>
        <td>MEMORY</td>
        <td>Jump if zero to memory address</td>
    </tr>
    <!-- JNZ -->
    <tr>
        <td>JNZ</td>
        <td>000 0000-0011</td>
        <td>L</td>
        <td>MEMORY (label)</td>
        <td>NULL</td>
        <td>MEMORY</td>
        <td>Jump if not zero to memory address</td>
    </tr>
    <!-- CMP -->
    <tr>
        <td>CMP</td>
        <td>000 0000-0100</td>
        <td>L</td>
        <td>NULL</td>
        <td>Perform not equals instead</td>
        <td>POINTER</td>
        <td>Compare if A and B are equal and store result in C</td>
    </tr>
    <!-- CPL -->
    <tr>
        <td>CPL</td>
        <td>000 0000-0101</td>
        <td>L</td>
        <td>NULL</td>
        <td>Perform more than instead</td>
        <td>POINTER</td>
        <td>Compare if A is less than B and store result in C</td>
    </tr>
    <!-- AND -->
    <tr>
        <td>AND</td>
        <td>000 0000-0111</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>POINTER</td>
        <td>Performs a bitwise AND operation between the values in registers A and B and stores the result in the C register</td>
    </tr>
    <!-- XOR -->
    <tr>
        <td>XOR</td>
        <td>000 0000-1000</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>POINTER</td>
        <td>Performs a bitwise XOR operation between the values in registers A and B and stores the result in the C register</td>
    </tr>
    <!-- OR -->
    <tr>
        <td>OR</td>
        <td>000 0000-1001</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>POINTER</td>
        <td>Performs a bitwise OR operation between the values in registers A and B and stores the result in the C register</td>
    </tr>
    <!-- HLT -->
    <tr>
        <td>HLT</td>
        <td>000 0000-1010</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>Halt execution (and jump to :end segment)</td>
    </tr>
    <!-- PANIC -->
    <tr>
        <td>PANIC</td>
        <td>000 0000-1011</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>Cause panic (and jump to :panic segment)</td>
    </tr>
    <!-- Memory addressing modes - MBNKROM -->
    <tr>
        <td>MBNKROM</td>
        <td>000 0000-1100</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>Address ROM memory bank directly (slow - changes where to read/not write)</td>
    </tr>
    <!-- Memory addressing modes - MBNKRAM -->
    <tr>
        <td>MBNKRAM</td>
        <td>000 0000-1101</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>Address RAM memory bank directly (fast - changes where to read/not write - if value isn't in RAM, falls back to RAM)</td>
    </tr>
    <!-- ZERO -->
    <tr>
        <td>ZERO</td>
        <td>000 0000-1111</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>Zero all memory in RAM</td>
    </tr>
    <!-- LDL -->
    <tr>
        <td>LDL</td>
        <td>000 0001-0000</td>
        <td>S</td>
        <td>MEMORY, CONSTANT</td>
        <td>NULL</td>
        <td>MEMORY, CONSTANT</td>
        <td>Load value from memory or inline constant to lower half of register L</td>
    </tr>
    <!-- LDH -->
    <tr>
        <td>LDH</td>
        <td>000 0001-0001</td>
        <td>S</td>
        <td>MEMORY, CONSTANT</td>
        <td>NULL</td>
        <td>MEMORY, CONSTANT</td>
        <td>Load value from memory or inline constant to top half of register L</td>
    </tr>
    <!-- CLD -->
    <tr>
        <td>CLD</td>
        <td>000 0001-0010</td>
        <td>L</td>
        <td>NULL</td>
        <td>NULL</td>
        <td>REGISTER</td>
        <td>Load bytes 16K into memory starting a address L (L&H) register from ROM (very slow)</td>
    </tr>
</table>
