#include <specstrings.h>

_Post_satisfies_(*n == 5)
void foo(int* n)
{
    char a[10];
    a[0] = 0;

    *n = 5;
    n+=2;
    *n = 6;
    return;
}