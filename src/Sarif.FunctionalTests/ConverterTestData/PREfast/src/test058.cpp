int acBits[16];
void f(int iMax)
{
    int cBits;
    if (iMax > 0xFFFF)
      iMax = 0xFFFF;        // 65000 runs NOTE: This is a bad assumption, e.g., for negative iMax

    if ( iMax & 0xF000 )
      cBits = acBits[(iMax >> 12) & 0x00FF] + 12;   // [ESPXFN] ESPX does not warn this. See iMax = 0x80011000 example below.
    else if (iMax & 0x0F00 )
      cBits = acBits[(iMax >>  8) & 0x00FF] +  8;   // [ESPXFP] ESPX ignores the condition for the first if to skip, and reports this as error.
    else if (iMax & 0x00F0)
      cBits = acBits[(iMax >>  4) & 0x00FF] +  4;   // [ESPXFP] ESPX ignores the conditions for the above if's to skip, and reports this as error.
    else
      cBits = acBits[iMax]; // BAD. e.g., iMax = 0x80010000
} 

void main()
{
    f(0x80011000);  // BAD. Causes acBits[(iMax >> 12) & 0x00FF] to overflow accessing offset 0x11.
    // I cannot think of numbers that can cause the next two acBits accesses to overflow...
    f(0x80010000);  // BAD. Causes acBits[iMax] to underflow.
}
