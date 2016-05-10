#include "specstrings.h"

const unsigned long MAX_ULONG = 0xffffffff;
void f(__out_ecount_full(Count) int *buf, unsigned long Count)
{
    // if (Count > MAX_ULONG/sizeof(unsigned long))
    //     return;
    for ( ;Count > 0; --Count)
    {
	*buf++ = 0;
    }
}

#define LB 536880912
#define MAX_INDEX 80
#define UB (LB + MAX_INDEX)
int arr[MAX_INDEX +1];

void f(int i)
{
    if (i < LB || i > UB)
	return;
    arr[i-LB] = 1;	
}

#include "mymemory.h"

void main()
{
    unsigned long count = MAX_ULONG / sizeof(unsigned long);
    int* buf = (int*)malloc(count * sizeof(int));   // This can cause warning on x86 arch.
    if (buf == nullptr)
        return;

    f(buf, count);
    int value = buf[count - 1];    // OK

    ++count;
    buf = (int*)malloc(count * sizeof(int));
    if (buf == nullptr)
        return;

    f(buf, count);
    value += buf[count - 1];    // OK
}