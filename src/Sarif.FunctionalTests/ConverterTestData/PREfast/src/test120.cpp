#include <specstrings.h>
#include <stdlib.h>
#include "mymemory.h"
#include "undefsal.h"

typedef struct _StringStruct
{
    char GlobalString[100];
} StringStruct;

typedef struct _MyStruct
{
    __field_range(<=, sizeof(StringStruct)) unsigned int Size;
} MyStruct, *PMyStruct;

typedef struct _MyWrapper
{
    PMyStruct PMS;
} MyWrapper, *PMyWrapper;

void safe1(__out_bcount(100) char *pOut, __in PMyStruct pMS)
{
    memset(pOut, 0, pMS->Size); // OK. pMS->Size <= sizeof(StringStruct)
}

void safe2(__out_bcount(100) char *pOut, __in PMyWrapper pMW)
{
    memset(pOut, 0, pMW->PMS->Size);    // OK. pMW->pMS->Size <= sizeof(StringStruct)
}

void main()
{
    char buf[100];
    MyStruct ms;
    ms.Size = sizeof(StringStruct);
    safe1(buf, &ms);    // OK

    MyWrapper mw;
    mw.PMS = &ms;
    safe2(buf, &mw);    // OK
}

