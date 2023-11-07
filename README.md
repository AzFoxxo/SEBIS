# SEBIS
The Simplified Eight Bit Instruction Set

## Motivation and Goal
The motivation behind this project was take the failures of a previous project `QKTK` (another 8 bit machine) and solve them to the make the machine useful as a playground for coding experiments instead of being extremely limited to the point of only allowing 128 instructions like its predecessor.

The final aim of this project is not to produce a 100% accurate recreation of a fantasy virtual machine but to learn how to emulate a simplified system and work around its limitations.

## Technical specifications
For the technical specifications of the fantasy machine and instruction set, see [specifications](docs/specifications).

## Projects
- `SEBIS.Assembler` - A simple assembler which takes SEBIS Assembly and produces a rom file.
- `SEBIS.Shared` - Contains shared code between the assembler and virtual machine
- `SEBIS.VirtualMachine` - is a simple implementation of the fantasy machine which runs SEBIS rom files.

## Licence
This project is licenced under the [MIT](LICENSE).