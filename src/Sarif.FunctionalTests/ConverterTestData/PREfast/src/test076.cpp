//#include <windows.h>
#include "specstrings.h"

void bad1(__out_ecount_full(size/2) int *p, int size)
{
    for (int i = 0; i < size; i++)  // BAD
    {
        p[i] = 1;
    }
}

void good1(__out_ecount(size) char *buf, size_t size)
{
    size_t size1 = size/2;

    size_t i;
    for (i = 0; i < size1; i++)
    {
        buf[i] = 0;
        buf[2*i] = 1;
    }
}

void good2(__out_ecount(size) char *buf, size_t size)
{
    size_t size1 = size >> 1;

    size_t i;
    for (i = 0; i < size1; i++)
    {
        buf[i] = 0;
        buf[i<<1] = 1;
    }
}

void good3(__out_bcount((size+1)/2) char *p, int size)
{
    int c = size/2;    int i = 0;
    for (i = 0; i < c; i++)
    {
        p[i] = 1;
    }
    if (size % 2 == 1)
        p[i] = 2;
}

void main()
{
    int i[5];
    bad1(i, 5 * 2); // BAD. I am following the contract, but it will cause bad1 to overflow i.
    char buf[5];
    good1(buf, 5);  // OK. Only 4 elememts will be initialized, but that is the contract.
    good2(buf, 5);  // OK. Only 4 elements will be initialized, but that is the contract.
    good3(buf, 5);  // OK. 3 elements will be initialized, and that is the contract.
}