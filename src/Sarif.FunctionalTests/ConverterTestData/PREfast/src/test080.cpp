#include "specstrings.h"

void f(int x, int y)
{
    size_t n = 0;
    if ((x & 0x80) != 0)
        n++;
    if ((x >> y) != 0)
        n++;
    if (~x == y)
        n++;
    if (x % 3 == 0)
        n++;
    int *p = new int[n];
    if (p != nullptr)
    {
        // Below accesses are OK - same operations as above.
        n = 0;
        if ((0x80 & x) != 0)
            p[n++] = 1;
        if ((x >> y) != 0)
            p[n++] = 2;
        if (~x == y)
            p[n++] = 1;
        if (x % 3 == 0)
            p[n++] = 1;
       delete[] p;
    }
}

void g(int x, int y)
{
    int a[10];

    if (x * y == 9)
        a[y * x + 1] = 1;   // BAD. Overflows a.
}

void nested()
{
    int a[10];

    for (int i = 0; i < 10; i++)
    {
        for (int j = 0; j < 10; j++)
        {
            int x = i | j;
            a[x] = 1;   // BAD. It can overflow a. [PFXFN] Likely due to complex index. [ESPXFP] It won't underflow.
        }
    }
}

char call(__out_ecount(n) char *buf, size_t n, char m)
{
    return buf[n >> m]; // BAD. It can overflow buf if m == 0
}

char call1(__out_ecount(n) char *buf, size_t n, char m)
{
    size_t n1 = n >> m;
    return buf[n1]; // BAD. It can overflow buf if m == 0
}

//relational operators
void TestRelopBad(unsigned int i, unsigned int j)
{
    int a[10];
    int b1 = (j < 10);
    if (i < 10)
        a[i + b1] = 1;  // BAD. Overflows a if i == 9 && j < 10
}

void TestRelopGood(unsigned int i, unsigned int j)
{
    int a[10];
    int b1 = (j < 10);
    int b2 = (j >= 10);
    if (i < 9)
        a[i + b1 + b2] = 1;  // [ESPXFP] Not smart enough to prove this safe yet
}

void main()
{
    f(0x80, 0); // OK
    f(0x82, 1); // OK
    f(~0x7, 0x7);   // OK
    f(0x80 * 3, 0); // OK

    g(3, 3);     // BAD. [PFXFN] PREfix misses this. Likely because of multi-variable index.

    char a[10];
    call(a, 10, 0);     // BAD. Overflows a. [PFXFN] Likely because of shift operation for the index.
    call1(a, 10, 0);    // BAD. Overflows a. [PFXFN] Likely because of shift operation for the index.

    TestRelopBad(9, 9); // Bad.
    TestRelopGood(9,9);     // OK
    TestRelopGood(9,10);    // OK
}