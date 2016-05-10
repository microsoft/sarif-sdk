// The loop in this test mimick the control flow of some code in 
// dcomss.  Avoiding false alarm here requires widening with the condition
// gathered after one pass over the loop.
#include "specstrings.h"

// test unsigned.
void indexing(unsigned int anIndex)
{
    int a[100], b[128];

    a[anIndex & 0x3f] = b[anIndex & 0x3f];  // OK. [PFXFP] PREfix thinks this can overflow.
    a[anIndex & 0x7f] = b[anIndex & 0x7f];  // BAD. Can overflow a but not b. [PFXFN] Above error in PREfix suppresses this warning.
    a[anIndex & 0xff] = b[anIndex & 0xff];  // BAD. Can overflow a and b.  [PFXFN] Above error in PREfix suppresses this warning.
}

void main()
{
    indexing(0x3f); // OK.
    indexing(0x7f); // BAD. Can overflow a. [PFXFP] for line 11 [PFXFN] for lines 12 and 13
    indexing(0xff); // BAD. Can overflow a and b. [PFXFP] for line 11 [PFXFN] for lines 12 and 13
}