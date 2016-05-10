#include "specstrings.h"
// #include "undefsal.h"

void TestPtrDiff1(__out_ecount(end - start) int *start, int *end)
{
    start[end - start] = 1;     // BAD. Overflow.
}

void Good()
{
    int a[10];
    TestPtrDiff1(a, a + 10);    // OK for ESPX per annotations. [PFXFN] PREfix should warn, but doesn't.
}

void Bad()
{
    int a[10];
    TestPtrDiff1(a, a + 11);    // BAD. [PFXFN] PREfix should warn, but doesn't.
}

void TestPtrDiffChar1(__out_ecount(end - start) char *start, __in char *end)
{
    unsigned n = end - start;
    if (n >= 2)
    {
        *start++ = 1;   // OK
        *start++ = 2;   // OK
    }
}

void TestPtrDiffChar2(__out_ecount(end - start) char *start, __in_ecount(0) char *end)
{
    *start++ = 1;
    *start++ = 2;   // BAD. Can overflow if end - start <= 1
    *start++ = 3;   // BAD. Can overflow if end - start <= 2
}

void TestPtrDiffInt(__out_ecount(end - start) int *start, __in_ecount(0) int* end)
{
    unsigned n = end - start;
    if (n >= 2)
    {
        *start++ = 1;
        *start++ = 2;
    }
}

void foo(__in_ecount(end - begin + 1) char *begin, /* __in */ __in char * const end)
{
    char ch = begin[end - begin]; // OK. Last element.
}

void Test2006(__in_ecount(end - begin + 1) char *begin, /* __in */ char * const end)
{
    foo(begin, end);
}

char NextChar()
{
    static char ch = 0xF0;
    return ch++;
}

void Read(__out_ecount(end - start) char *start, __in_ecount(0) char *end, __deref_out_ecount(end - *newstart) char **newstart)
{
    char *ptr = start;
    while (ptr <= end)  // Should be ptr < end
    {
        char c = NextChar();
        if (c == 0)
            break;
        *ptr++ = c; // BAD. Overflow when ptr reaches end        
    }
    *newstart = ptr;
}

void TestRead()
{
    char a[100];
    char *p = a;
    char *end = a + 100;    
    while (p < end)
        Read(p, end, &p);
}

void PtrDiffIntGood(__out_ecount(end - start) int *start, __in_ecount(0) int* end)
{
    for (int *p = start; p < end; p++)
        *p = 1;
}

void PtrDiffIntBad(__out_ecount(end - start) int *start, __in_ecount(0) int* end)
{
    for (int *p = start; p <= end; p++)
        *p = 1;
}

// Test pointer diff. in code.
void Fill(__out_ecount(size) short *p, size_t size)
{
    if (p == nullptr)
        return;

    for (size_t i = 1; i <= size; ++i)
        *p = i;
}

void f()
{
    short a[10];
    short *p = a;
    short *end = a + 10;
    p++;
    Fill(p, end - p);
}

void main()
{
#pragma warning(push)
#pragma warning(disable:26017,26052)
    char c2[2] = {'a', 'b'};
    TestPtrDiffChar1(c2, c2+2); // OK. Ignore ESPX warnings for c2 and c2 + 2 for this test.
    char c1[1] = {'a'};
    TestPtrDiffChar2(c1, c1+1); // BAD c1[1] and c1[2] will overflow.

    int i2[2];
    TestPtrDiffInt(i2, i2 + 2); // OK

    Test2006(c2, c2+1);         // OK
    Test2006(c2, c2+2);         // BAD. [PFXFN] PREfix does not warn this.

    PtrDiffIntGood(i2, i2+2);   // OK
    PtrDiffIntBad(i2, i2+2);    // BAD. [PFXFN] PREfix does not warn this.
#pragma warning(pop)
}
