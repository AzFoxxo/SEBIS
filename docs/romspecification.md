# ROM specification

The SEBIS ROM file format (.SEBIS) contains all the machine code of a program plus metadata telling the machine where to find the initialisation function, etc.

## Sections
- Header
  - Bytes 1-6 for `.SEBIS` in ASCII
  - Bytes7-9 for version (one byte per number in format x.x.x)
  - Bytes 10-11 for size of ROM in bytes
- ROM
  - SEBIS constants
    - Bytes 1-2 for init location in ROM
    - Bytes 3-4 for panic location in ROM
    - Bytes 5-6 for end location in ROM
  - Remaining data inside of 64K, general ROM
- Footer
  - Bytes 2-33 for SHA256 hash of ROM