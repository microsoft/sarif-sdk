#include "specstrings.h"
#include <string.h>
#include "undefsal.h"
void ZeroMem(__out_bcount(lcElements * stElemSize) void *pvArray, long lcElements, unsigned stElemSize)
{
    for (size_t i = 0; i < lcElements * stElemSize; ++i)
        ((char*)pvArray)[i] = 0;
}

void SetArray(__out_ecount(count) int *pArray, int val, long count)
{
    for (size_t i = 0; i < count; ++i)
        pArray[i] = val;
} 

void f()
{
    int a[10];
    ZeroMem(a, 11, sizeof(a[0]));   // [PFXFN] PREfix misses this
    SetArray(a, 1, 11);
}

void g(__out_ecount(size + 1) int *p, int size)
{
    p[size] = 1;
}

void h(__out_ecount(size) int *a, int size)
{
    g(a, size);
}

void foo(__out_ecount(sizeplus1-1) int *a, unsigned sizeplus1)
{
    a[sizeplus1-1] = 1;
}

void bar(__out_ecount(size) int *buf, unsigned short size)
{
    foo(buf + 1, size);     // ESPX does not see bug in foo through this... by design!
}

unsigned int StrLen(__post __readableTo(elementCount(return + 1)) const char *s)
{
    return strlen(s);
}

char Last(__pre __valid const char *s)
{
    unsigned len = StrLen(s);
    if (len > 0)
        return s[len-1];
    return 0;
}

char Complex(__pre __valid char *s)
{
    unsigned trailingLen = StrLen(s+3);
    for (int i = 0; i <= trailingLen + 3; i ++)
      s[i] = s[i]*2;
    return 0;
}

char BugLast(__pre __valid const char *s)
{
    unsigned len = StrLen(s);
    if (len > 0)
        return s[len+1];        // [PFXFN] PREfix seems to miss this.
    return 0;
}

void f1(__out_ecount((10 + 5)/2) int *buf)
{
    for (int i = 0; i < (10 + 5)/2; ++i)
        buf[i] = i;
}

void g1(__out_ecount(10 - 3 * 2) int *buf)
{
    f1(buf);
}

void Fill(__out_ecount(n) char *s, size_t n)
{
    for (int i = 0; i < n; ++i)
        s[i] = 'a' + n % 26;
}

char foo()
{
    char a[10];
    Fill(a, 10);
    size_t len = strlen(a);     // [PFXFN] PREfix does not catch that a is not null-terminated
    return a[len];
}

void bar(__inout_ecount(cx * cy) int *buf, size_t cx, size_t cy)
{
    size_t k = 0;
    for (size_t i = 0; i < cx; i++)
        for (size_t j = 0; j < cy; ++j)
            buf[k++] = 0;  // [ESPXFP] ESPX Should not warn here but it does; see Esp:638
}

namespace ShiftTest
{
#define SHIFT 2

void f(__out_ecount(n << SHIFT) int * buf, int n)
{
    for (int i = 0; i <= n << SHIFT; i++)
        buf[i] = 1;
}

void g(__out_ecount(n >> SHIFT) int *buf, int n, int s)
{
    int m = n >> SHIFT;
        for (int i = 0; i < m; i++)
            buf[i] = 1;
}

void foo()
{
    int a[10];
    f(a, 10);
}

void bar()
{
    int a[10];
    g (a, 100, 4);
}
}

void main()
{
    int buf[10];
    h(buf, 10);
    foo(buf, 11);
    bar(buf, 10);

    int buf1[10 - 3 * 2];
    g1(buf1);

    int buf2[8];
    ShiftTest::f(buf2, 8 >> SHIFT);
    ShiftTest::g(buf2, 8 << SHIFT, 0);  // OK
}







