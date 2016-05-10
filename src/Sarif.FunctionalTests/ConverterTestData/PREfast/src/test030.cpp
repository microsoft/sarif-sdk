#include "specstrings.h"
#include <stdarg.h>

void ThisOneIsFine(int count, int y, ...)
{
    va_list Arglist;
    va_start(Arglist, y);
    for (int i = 0; i < count; i ++)
    {
        int funny = va_arg(Arglist, int);
    }
    va_end(Arglist);
}

#ifndef _WIN64
void ThisOneIsBad(int x, int y, int z)
{
    va_list Arglist;
    va_start(Arglist, y);
    int funny = va_arg(Arglist, int);   // BAD. [ESPXFN] ESPX does not report this.
    va_end(Arglist);
}
#endif

void main() { /* dummy */ }