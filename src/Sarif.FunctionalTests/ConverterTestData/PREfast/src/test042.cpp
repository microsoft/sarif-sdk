#include "specstrings.h"
#include "mymemory.h"
#include "undefsal.h"

__ecount(n) int *f(int n)
{
    int* p = new int[n-1];
    if (p != nullptr)
    {
        for (int i = 0; i < n-1; ++i)
            p[i] = 1;
    }
    return p;   // BAD for ESPX. p should have n elements.
}

void g(__deref_out_ecount(n) int **buf, int n)
{
    int* p = new int[n-1];
    if (p != nullptr)
    {
        for (int i = 0; i < n-1; ++i)
            p[i] = 1;
    }
    *buf = p;   // BAD for ESPX. p should have n elements.
}

void h(__deref_out_ecount(n) int **ppbuf, __in_ecount(n) int *pbuf, int n)
{
    if (ppbuf == nullptr || pbuf == nullptr || n <= 0)
        return;
    *ppbuf = pbuf + 1;  // BAD. *ppbuf should have n elements.
}	

__bcount(n) void *g1(int n)
{
    return new int[10]; // BAD. Should return n-byte array.
}

__bcount(n) int *g2(int n)
{
    static int a[10];
    return a - 1;       // BAD. Should return n-byte array.
}

void foo(__out_ecount_full(n) int *a, int n)
{
    if (a == nullptr)
        return;

    for (int i = 0; i < n; i++)
        a[i] = 1;

	n += 10;
}

void bar(__out_ecount_part(*n, *n) int *a, __inout int *n)
{
    if (a == nullptr || n == nullptr)
        return;

    for (int i = 0; i < *n; i++)
        a[i] = 1;

    *n += 10;   // BAD. *n cannot grow per annotation.
}

int Read()
{
    return 1; // Not ideal, but doesn't really change test results for ESPX / PREfix
}

void Next(__out_ecount_part(size, *fetched) int *a, size_t size, __out_opt size_t *fetched)
{
    if (fetched) {
        *fetched = 0;
    }

    short int i = 0;
    while (i < size) {  // This doesn't protect below i++ operation from overflow, causing underrun.
        int x = Read();
        if (x > 0)      // NOTE: If this is removed, ESPX does not warn potential underrun for next line.
            a[i++] = x; // BAD. Can underrun (if i overflows). [PFXFN] PREfix misses this.
    }
    if (fetched)
        *fetched = i;
}

void NextUnannotated(__out_ecount_part(size, *fetched) int *a, size_t size, size_t *fetched)
{
    if (fetched) {
        *fetched = 0;
    }

    short int i = 0;
    while (i < size) {  // This doesn't protect below i++ operation from overflow, causing underrun.
        int x = Read();
        if (x > 0)      // NOTE: If this is removed, ESPX does not warn potential underrun for next line.
            a[i++] = x; // BAD. Can underrun (if i overflows). [PFXFN] PREfix misses this.
    }
    if (fetched)
        *fetched = i;
}

// ESP bug #397: should not give warning 26045 in the case when the buffer is NULL.
void Foo( unsigned int dwBytesAllocated,
         __out_bcount_part_opt(dwBytesAllocated, *pdwBytesUsed) unsigned char *pBuffer, 
         __out unsigned int *pdwBytesUsed)
{
    if (pBuffer == 0)
    {
        *pdwBytesUsed = 4;   // There should be no warning in this case
    }
    else 
    {
        if (dwBytesAllocated >= 4)
        {
            *pdwBytesUsed = 4;
            *(unsigned int *)pBuffer = 0;
        }
        else
        {
            *pdwBytesUsed = 0;
        }
    }
}

void main()
{
    int* p;
    int size;
    int result;

    size = 10;
    p = f(size);
    if (p != nullptr)
        result = p[size - 1];   // BAD. Overflow.

    g(&p, size);
    if (p != nullptr)
        result = p[size - 1];   // BAD. Overflow.

    int a[20];
    size = 20;

    h(&p, a, size);
    p[size - 1] = 1;            // BAD. Overflow.

    p = (int*)g1(size * sizeof(int));
    if (p != nullptr)
        p[size - 1] = 1;        // BAD. Overflow

    p = g2(20 * sizeof(int));
    if (p != nullptr)
    {
        p[0] = 1;   // BAD. Underflow.
        p[19] = 1;  // BAD. Overflow. However, PREfix does not report this.
    }

    foo(a, size);
    result = a[size - 1];       // OK

    bar(a, &size);
    result = a[size - 1];       // BAD for PREfix. Overflow. OK for ESPX per annotation.

    //void Next(__out_ecount_part(size, *fetched) int *a, size_t size, __out_opt size_t *fetched)
    int buf[0x10000];
    size_t maxlen = 0x10000;
    size_t fetched;
    Next(buf, maxlen, &fetched);                // BAD. Should underflow buf. [PFXFN] PREfix does not report this...
    NextUnannotated(buf, maxlen, &fetched);     // BAD. Should underflow buf. [PFXFN] PREfix does not report this...
}