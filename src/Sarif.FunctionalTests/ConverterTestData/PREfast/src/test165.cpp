#include <specstrings.h>
#include "undefsal.h"

#define NULL 0

extern int Count();
extern int GetData(int);

void Test(
    __out_ecount_part(ulCount, *pcFetched) int *pData,
    unsigned int ulCount,
    __out unsigned int *pcFetched
    )
{
    int cur = 0;
    unsigned int cFetched;
    if (pcFetched == NULL)
        pcFetched = &cFetched;     // line 17
    *pcFetched = 0;
    while (cur < Count() && *pcFetched < ulCount)
    {
        *pData = GetData(cur);
        pData++;
        *pcFetched += 1;
        cur++;
    }
}
