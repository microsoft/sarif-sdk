#include "specstrings.h"

void f(__out_ecount(cb + 2 - ((cb + 2)/3) * 3 + 1) char *p, size_t cb)
{
    if (cb % 3 == 1)
        p[1] = 1;   // BAD. It can overflow if cb + 2 - ((cb + 2)/3) * 3 + 1 == 1. E.g., cb = 1.
}

void g(__out_ecount((cb + 2)/3 * 3) char *p, size_t cb)
{
    size_t i = 0;
    for (i = 0; i < cb; i++)
    {
        p[i] = 0;       // OK. cb <= ecount(p) for any cb > 0.
    }
    if (cb % 3 == 1)    // In these cases, ecount(p) == cb + 2
    {
        p[i++] = 0;     // OK.
        p[i++] = 0;     // OK.
    }
}

void h(unsigned x)
{
    int a[6];
    int r = (x & 0xff);

    if (x == 5)
        a[r] = 1;   // OK. r == 5
}

void main()
{
    char buf1[1];
    f(buf1, 1);      // BAD. Overflows buf1.

    char buf3[3];
    g(buf3, 1);     // OK

    h(5);   // OK
    h(6);   // OK. Won't access the array.
}