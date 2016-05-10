#include <specstrings.h>

typedef unsigned int DWORD;

int NegDWord_Safe()
{
    int data[2] = {};
    DWORD dw = -1;

    ++dw;

    return data[dw];
}

int NegDWord_Unsafe()
{
    int data[2] = {};
    DWORD dw = -1;

    ++dw;   // 0
    ++dw;   // 1
    ++dw;   // 2

    return data[dw];    // BAD. ESPX:26017 / PFX:23
}

void main() { /* dummy */ }