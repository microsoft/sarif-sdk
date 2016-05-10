// The loop in this test mimick the control flow of some code in 
// dcomss.  Avoiding false alarm here requires widening with the condition
// gathered after one pass over the loop.

#include "specstrings.h"
#include <cstdlib>

void useBuffer(__readableTo(byteCount(count)) 
                      const int *buf,
                      size_t count)
{
    if (buf == nullptr)
        return;

    char val = 0;

    for (size_t i = 0; i < count; ++i)
    {
        val |= ((char*)buf)[i];
    }
}

void bar(int *x)
{
    if (x == nullptr)
        return;

    *x = (int)x & 1;
}

void baz(void *x)
{
    if (rand() % 2 == 1)
    {
        *((char*)x) = 1;
    }
}

// test element count vs. byte count
void indexing(__writableTo(byteCount(count))  int *buf, size_t count)
{
    int a[100] = {1,2,3,4,5,6,7,8,9,10};
    useBuffer(a, 100);
    useBuffer(a, 100 * sizeof(int));
    useBuffer(buf, count);
}

// test SimState
int foo()
{
    int a[100];

    struct ufo
    {
        int x;
    };

    int x = 0;
    bar(&x);
    if (x == 0)
      return 1;
    useBuffer(a, 100 * sizeof(int));

    int *j = new int;
    *j = 0;
    baz(j);
    if (*j == 0)
      return 1;
    useBuffer(a, 100 * sizeof(int));

    ufo *v = new ufo();
    v->x = 0;
    baz(v);
    if (v->x == 0)
      return 1;
    useBuffer(a, 100 * sizeof(int));
    return 0;
}

void main() { /* dummy */ }