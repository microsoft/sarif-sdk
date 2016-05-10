#include <specstrings.h>
#include "undefsal.h"

#define     BMAX    16
//#define ARRAYSIZE(A) (sizeof(A)/sizeof(A[0]))

unsigned c[BMAX+1];         

void GoodNoLoop(__in_range(>, 0) int i, __in_ecount(i) unsigned int *p)
{
    __analysis_assume(*p < 17); // ARRAYSIZE(c)

    c[*p++]++;                      // assume all entries <= BMAX 
}

void GoodNoLoopDecr(__in_range(>, 0) int i, __in_ecount(i) unsigned int *p)
{
    __analysis_assume(*p == 16); // ARRAYSIZE(c)

    c[*p--]--;                      // assume all entries <= BMAX 
}

void Good1(__in_range(>, 0) int i, __in_ecount(i) unsigned int *p)
{
    int j = 0;
    do
    {
        __analysis_assume(*p < 17); // ARRAYSIZE(c)

        c[*p++]++;                      // assume all entries <= BMAX 
    } while (++j < i);
}

void Good2(__in_range(>, 0) int i, __in_ecount(i) unsigned int *p)
{
    do
    {
        __analysis_assume(*p < 17); // ARRAYSIZE(c)

        c[*p++]++;                      // assume all entries <= BMAX 
    } while (--i);
}

void Bad()
{
    char a[10];
    char *p = a + 10; 
    *p++ = 0; // bad
}

void Bad2()
{
    char a[10];
    char* p = a + 9; 
    char* q = p++; 
    *q++ = 0; // ok
    *q++ = 0; // bad
    *p = 0; // bad
}

void Bad3()
{
    char a[10];
    char* p = a + 9; 
    char* q = ++p;    
    *q++ = 0; // bad    
    *p = 0; // bad
}

struct Structure
{
    int count;
    unsigned* buf;
};

void GoodNested(Structure* p)
{
    __analysis_assume(p->count < 17 && p->count >= 0); // ARRAYSIZE(c)
    c[p->count++];
}

void Consumer(/* __in char* buf */ char const * buf)
{
    // *buf = 'a';
}

void BadCaller()
{
    char a[10];
    char* p = a + 10; 
    char* q = a + 9; 

    // see if pointers in buffers on the call get checked correctly
    Consumer(q++); // ok
    Consumer(p++); // bad
}
