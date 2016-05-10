#include <stdlib.h>
#include <memory.h>

struct windowoid
{
    int haha;
    int buffer[10];
};

void foo(int length)
{
    windowoid ww;
    ww.buffer[0] = 1;
    ww.buffer[12] = 2;  // BAD. Overflow.
    memset(ww.buffer, 0, length * sizeof(int));     // BAD. Can overflow if length > 10
    memset(ww.buffer + 2, 0, 9 * sizeof(int));      // BAD. Overflow.
}

void main()
{
    // Cause foo to overflow
    foo(11);
}