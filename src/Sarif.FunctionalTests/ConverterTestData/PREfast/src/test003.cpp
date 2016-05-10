#include "specstrings.h"
#include <memory.h>

int useBuffer(__pre __writableTo(elementCount(count)) int *vec, int count)
{
    memset(vec, 0, count * sizeof(int));
    return 0;
}

int main()
{
    int a[10];
    int *b = a+7;
    useBuffer(a+3, 5);
    useBuffer(a+2, 9);
    useBuffer(b, 3);
    useBuffer(b, 5);

    return a[12] + (b-=1)[3];
}

