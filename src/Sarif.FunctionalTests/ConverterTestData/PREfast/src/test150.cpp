// Test _At_buffer_ and _Satisfies_ parsing

#define NULL 0

#include <specstrings.h>
#include "specstrings_new.h"

_At_buffer_(pa, _I_, count, __notnull)
void foo (int ** pa, int count) {}

_At_buffer_(pa, _I_, count, _Satisfies_(pa[_I_] == pb[_I_]))
void foo2 (int ** pa, int **pb, int count) {}

// being perverse: same iterator variable for both (but should be 
// different symbols!)
_At_buffer_(pa, _I_, count, _Satisfies_(pa[_I_] == pb[_I_])
    _At_buffer_(*pa, _I_, count, _Satisfies_(*pa[_I_] == *pb[_I_])))
void foo3 (int *** pa, int ***pb, int count) {}

// Added superfluous buffer param to ensure analysis - see Esp:713
_Pre_satisfies_(n == m)
_Post_satisfies_(return == n + m)
int SatFunc(int n, int m, __in_bcount_opt(20) char *cp = NULL)
{
    return n + 1;
}

void TestSat1()
{
    int a[10];
    a[0] = SatFunc(1, 2);
}

// Expect "sure" buffer overrun warning here since we know that cp[y] is
// out of bounds due to the satisfies.
_Pre_satisfies_(x == y)
char TestPresatBad(int x, int y, __in_ecount(x) char *cp)
{
    return cp[y];
}

// The access here is 100% safe.
_Pre_satisfies_(x == y)
char TestPresatGood(int x, int y, __in_ecount(x) char *cp)
{
    if (x <= 0)
        return 0;
    return cp[y - 1];
}

