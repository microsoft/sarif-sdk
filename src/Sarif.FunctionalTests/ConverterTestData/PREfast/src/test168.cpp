#include <sal.h>
#include "undefsal.h"

#define NULL 0

extern void SetToNull(_Out_ _At_(*p, _Post_ _Null_) void **p);
extern void SetToZero(_Out_ _At_(*p, _Out_range_(==, 0)) void **p);

void TestSetToNull()
{
    int a[10];
    void *p;

    SetToNull(&p);
    if (p != NULL)
        a[11] = 0; // no warning expected here: infeasible path
    a[10] = 0;     // expect buffer overrun warning here
}

void TestSetToZero()
{
    int a[10];
    void *p;

    SetToZero(&p);
    if (p != NULL)
        a[11] = 0; // no warning expected here: infeasible path
    a[10] = 0;     // expect buffer overrun warning here
}

