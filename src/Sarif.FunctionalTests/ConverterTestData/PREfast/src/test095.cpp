#include <specstrings.h>

//
// Subtle bug
//
// There was a bug where checking postconditions caused one of the possible
// values of the __success expression to be assumed.  If the function is
// inlined, then the callee was forced to assume this one outcome, leading to
// missing messages for the other outcomes.
//
// This test
//

bool GoopHelper(int x, int *q)
{
    static int magic1;
    static int magic2;

    if (magic1++)
    {
        *q = x << 1;
    }
    else
    {
        *q = -x;
    }

    return magic2++;
}

bool
__analysis_hint(INLINE)
__success(return == true)
Goop(int x, __deref_out_range(0,10) int *q)
{
    return GoopHelper(x, q);
    // Postcondtion checking has two outcomes: return==true and return!=true
    // (Because GoopHelper is unannotated.)
}


void KK(int z)
{
    int aa[10], bb[10];
    int b;
    if (Goop(z, &b))
      aa[b] = 1;      // 26014,26011 here is evidence of permitting return==true
    else
      bb[b] = 1;      // 26015,26011 here is evidence of permitting return!=true
}

void main()
{
    KK(5);      // BAD. This can cause overflow or underflow.   [PFXFN]
    KK(-5);      // BAD. This can cause overflow or underflow.  [PFXFN]
}