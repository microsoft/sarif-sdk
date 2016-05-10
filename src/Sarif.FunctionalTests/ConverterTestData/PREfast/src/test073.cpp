#include "specstrings.h"

__out_range(<, size) int find(__in_ecount(size) char *buf, int size)
{
    static bool magic;
    if (magic)
        return size - 1;
    else
        return -2;
}

void f(__in_ecount(size) char *buf, int size)
{
    int i = find(buf, size);
    buf[i];     // BAD. Can underflow if find returns < 0.
}

void f1(__in_ecount(size) char *buf, int size)
{
    int i = find(buf, size);
    buf[i+1];   // BAD. Can overflow if find returns size - 1. Can underflow if find returns < -1
}

__success(return == 1)
bool SafeAdd(short a, short b, __deref_out_range(==, a + b) short *c)
{
    if ((char)c % 2 == 0)
    {
        *c = a + b;
        return 1;
    }

    // Leave *c as is
    return 0;
}

void foo(__out_ecount_full(size) char *buf, short size)
{
    short size1 = 0;
    if (SafeAdd(size, 1, &size1))
	buf[size1] = 0;     // BAD. Overflow. Can also underflow if size + 1 overflows short.
}

void main()
{
    const short MAX_SHORT = 0x7FFF;
    char c[10] = {};
    f(c, 10);       // BAD. c can underflow.
    f1(c, 10);      // BAD. c can under/overflow.

    char c1[10];
    foo(c1, 10);    // BAD. Overflows c1.

    char c2[MAX_SHORT];
    foo(c2, MAX_SHORT); // BAD. Underflows c2 .
}

