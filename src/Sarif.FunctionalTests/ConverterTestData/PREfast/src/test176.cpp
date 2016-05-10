#include "specstrings.h"
#include "undefsal.h"

#define NULL 0

// Note: no warnings should be generated in this file

_When_(ptr == NULL, _Pre_satisfies_(flag == 5))
_When_(ptr != NULL, _Pre_satisfies_(flag == 6))
void PtrInWhen(
    _In_opt_ int *ptr,
    int flag
    )
{
    int a[10], b[10], c[10];

    if (flag == 5)
    {
        if (ptr != NULL)
            a[10] = 0;
    }
    else if (flag == 6)
    {
        if (ptr == NULL)
            b[10] = 0;
    }
    else
    {
        c[10] = 5;
    }
}

_When_(mode == 0, _Pre_satisfies_(flag == 5))
_When_(mode != 0, _Pre_satisfies_(flag == 6))
void IntInWhen(
    _In_ int mode,
    int flag
    )
{
    int a[10], b[10], c[10];

    if (flag == 5)
    {
        if (mode != 0)
            a[10] = 0;
    }
    else if (flag == 6)
    {
        if (mode == 0)
            b[10] = 0;
    }
    else
    {
        c[10] = 5;
    }
}

_When_(ptr, _Pre_satisfies_(flag1 == 5))
_When_(!ptr, _Pre_satisfies_(flag1 == 6))
_When_(ptr, _Pre_satisfies_(flag2 == 5))
_When_(!ptr, _Pre_satisfies_(flag2 == 6))
void PtrInWhen(
    _In_opt_ int *ptr,
    int flag1,
    int flag2
    )
{
    int a[10], b[10], c[10];

    if (flag1 == 5)
    {
        if (flag2 != 5)
            a[10] = 0;
    }
    else if (flag1 == 6)
    {
        if (flag2 != 6)
            b[10] = 0;
    }
    else
    {
        c[10] = 5;
    }
}


// _In_reads_or_z_ uses _When_ under the covers
unsigned int mystrnlen(
    _In_reads_or_z_(max) const char *cp,
    _In_ unsigned int max
    );

unsigned int Foo(_In_reads_(100) const char *cp)
{
    return mystrnlen(cp, 100);
}


