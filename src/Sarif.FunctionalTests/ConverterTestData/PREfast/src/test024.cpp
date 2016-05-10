#include "specstrings.h"
#include "undefsal.h"

void bar(__inout int(&arr)[2])
{
    arr[2] = 1;
}

void main()
{
    int a2[2];
    bar(a2);
}