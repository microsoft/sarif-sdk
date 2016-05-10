#include <specstrings.h>
#include "mywin.h"
#include "mymemory.h"

__success(return != false)
bool Compress(LPCWSTR pchIn, __out_ecount(5) LPWSTR pchOut)
{
    unsigned int ch;
    unsigned int cchLimit = 3;
    ch = *pchIn++;
    while (ch != 0)
    {
        if (cchLimit == 0)    // generates bogus 26014
        //if (cchLimit <= 0)   // no 26014
            return false;

        cchLimit--;
        *pchOut = (WCHAR)ch;
        pchOut++;
    }
    *pchOut++ = 0;
    return true;
}

void TestFunc(__inout_ecount(10) int *items, int *out)
{
    unsigned int num = 0;
    unsigned int filter = 0;
    *out = 0;
    int *pRange = &items[ filter - 1];
    while (num-- > 0) {
        if (0 == memcmp(pRange, &num, sizeof(num))) {  
            memcpy(out, pRange, sizeof(int));
        }
        pRange ++;
    }
} 

// This section is repro case for Esp:738
int f()
{
    return 4;
}

void g(__in_ecount(2) unsigned int *Array)
{
    unsigned int count = 0;
    unsigned int onePastIndex = 0;

    unsigned int *arrayElement;

    if (f())
    {
        count = 1;
        onePastIndex = 1;
    }

    arrayElement = &Array[onePastIndex - 1];

    while (count-- > 0)
    {
        *arrayElement = 5;
    }
}

void main()
{
    LPCWSTR myStrL = L"My test string";
    LPCWSTR myStrS = L"Str";
    WCHAR pchOut[5];

    Compress(myStrS, pchOut);   // OK
    Compress(myStrL, pchOut);   // OK

    int items[10];
    int out;
    TestFunc(items, &out);      // OK

    unsigned int items2[2];
    g(items2);                  // OK
}
