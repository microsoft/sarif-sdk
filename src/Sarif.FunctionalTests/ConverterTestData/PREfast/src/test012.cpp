// The loop in this test mimick the control flow of some code in 
// dcomss.  Avoiding false alarm here requires widening with the condition
// gathered after one pass over the loop.
#include "specstrings.h"
#include "mymemory.h"

__writableTo(elementCount(count)) int *mallocInt(size_t count)
{
    return (int*)malloc(count * sizeof(int));
}

int main()
{
    int *a = mallocInt(100);
    if (a == nullptr)
        return -1;

    int b[6] = {};

    int i = 0;
    for (int j = 0; j < 100; j ++)
    {
        a[j] = j;
        b[i] += a[j];
        if (++i > 5)
        {
            i = 0;
        }
    }
 end:;
}
