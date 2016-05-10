// The loop in this test mimick the control flow of some code in 
// dcomss.  Avoiding false alarm here requires widening with the condition
// gathered after one pass over the loop.
#include "specstrings.h"
#include "mymemory.h"

// test unsigned.
int indexing(unsigned int anIndex)
{
    int a[100];

    int index = (anIndex < 100) ? anIndex : 0;
    a[anIndex < 128 ? anIndex : 0] = 1;
    a[anIndex < 100 ? anIndex : 200] = 1;
    a[index] = (anIndex < 100)? 2 : 1;
    return a[index];
}

void *unannotatedMalloc(int size)
{
    return malloc(size);
}

int unannotated(int *x)
{
    int *y = x;
    int a = *x;
    int b = *x++;
    int c = *x;     // BAD. Can overflow if len(x) == 1. [ESPXFN] ESPX does not report this.
    int d = *y;
    int e = y[4];   // BAD. Can overflow if len(x) < 5. [ESPXFN] ESPX does not report this.
    int *z = &e;
    int f = *z;
    int *g = (int *)(unannotatedMalloc(400));
    if (g != nullptr)
    {
        g[2] = 1;
        free(g);
    }
    return 0;
}

void main()
{
    int a[1] = {1};
    unannotated(a);         // BAD. Can overflow.

    int b[4] = {1,2,3,4};   // BAD. Can overflwo.
    unannotated(b);
}