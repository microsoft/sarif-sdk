#include "specstrings.h"
#include "mymemory.h"

__writableTo(elementCount(count)) int *mallocInt(size_t count)
{
    return (int*)malloc(count * sizeof(int));
}

#define MAX_STACK_BUFFER_SIZE 200
int index(int count, int i)
{
    int *f = nullptr;
    int a[MAX_STACK_BUFFER_SIZE];
    int *b = a;
    if (count > MAX_STACK_BUFFER_SIZE)
    {
        b = mallocInt(count);
        if (b == nullptr)
            return -1;
        f = b;
    }
    else
    {
        b = a;
    }

    if (i >= 0 && i < count)
      b[i] = 1;
    else
      b[2] = 1;

    if (f != nullptr)
        free(f);

    int *p = a;
    int x = *p = 0;

    return 0;
}

void main() { /* dummy */ }