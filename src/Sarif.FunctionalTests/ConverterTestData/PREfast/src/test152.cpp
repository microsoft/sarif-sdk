#include <specstrings.h>
#include "undefsal.h"

__struct_bcount(nSize) struct MyStruct
{
    unsigned int nSize;
    int kind;
};


int Good1(__in MyStruct const *pMS)
{
    return pMS->kind;
}

int Good2(__in MyStruct *pMS)
{
    if (pMS->nSize < sizeof(MyStruct) + 1)
        return 0;

    char *pData = (char *)(pMS + 1);
    return *pData;
}

int Bad1(/* __in MyStruct *pMS */ MyStruct * pMS)
{
    char *pData = (char *)(pMS + 1);
    return *pData;
}

void AllocMyStruct(__deref_out MyStruct **ppMS);

int Good3()
{
    MyStruct *pMS;

    AllocMyStruct(&pMS);

    return pMS->kind;
}

