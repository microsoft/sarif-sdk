#include "specstrings.h"

#define MAX_SIZE 100

void foo(int nSize, float f)
{
    int data[MAX_SIZE];

    if (nSize > MAX_SIZE)
        return;

    if (f > 1.0)
        data[0] = 1;

    for (int i = 1; i < nSize; i++)
        data[i] = 0;

    data[MAX_SIZE] = 101;  // expect a buffer overrun warning only here. ESPX:26000 / PFX:23
}

void main() { /* dummy */ }