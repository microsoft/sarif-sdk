#include <sal.h>


// These tests should not generate any warnings, and are basically identical,
// but need to ensure that we can handle both macros and const symbols.

const int SIDE = 4;
const int AREA = SIDE * SIDE;

void Foo_consts(_Out_writes_(AREA) int *p)
{
    for (int i = 0; i < AREA; i++)
        p[i] = 0;
}

#define SIDE_m 4
#define AREA_m (SIDE_m * SIDE_m)

void Foo_macros(_Out_writes_(AREA_m) int *p)
{
    for (int i = 0; i < AREA_m; i++)
        p[i] = 0;
}

