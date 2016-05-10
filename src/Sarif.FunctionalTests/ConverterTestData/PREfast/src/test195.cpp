#include <specstrings.h>
#include "undefsal.h"

struct Struct1
{
    char a[100];
};

struct Struct2
{
    int b;
    Struct1 s1;
};

void foo(_In_ Struct2* s2)
{
    // this particular pattern of field access was causing the wrong offset
    // to be calculated for s2->s1.a and false positive messages about the
    // safe access to be reportd.
    s2->s1.a[99] = 0;
}

