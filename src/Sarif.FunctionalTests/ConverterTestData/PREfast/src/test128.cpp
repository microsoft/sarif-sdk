#include <specstrings.h>
#include "mymemory.h"
#include "undefsal.h"

typedef unsigned long ULONG;
typedef unsigned short WCHAR;

struct GUID
{
    int x;
    int y;
};

struct NT4SID
{
    int x;
    int y;
};

#define MAX_NT4_SID_SIZE 10
#define MAX_WCHAR_IN_DSNAME 10*1024*1024

typedef struct _DSNAME {
    ULONG structLen;
    __field_range(<=,MAX_NT4_SID_SIZE) ULONG SidLen;
    GUID Guid;
    NT4SID Sid;
    __field_range(<=,MAX_WCHAR_IN_DSNAME) ULONG NameLen;
    //WCHAR fill;
    __field_ecount(NameLen + 1) WCHAR StringName[1];
} DSNAME;

#define offsetof(s,m)   (size_t)&(((s *)0)->m)

__success(return != 0)
bool GetDSNAMEGood(
        ULONG bufSize,
        __out_bcount(bufSize) DSNAME *pName,
        ULONG nameSize,
        __in_ecount(nameSize) WCHAR *pRawName
        )
{
    if (bufSize < (offsetof(struct _DSNAME, StringName) + (nameSize + 1) * sizeof(WCHAR)))
        return false;
    if (nameSize > MAX_WCHAR_IN_DSNAME)
        return false;

    pName->structLen = bufSize;
    pName->SidLen = 5;
    pName->NameLen = nameSize;
    memcpy(pName->StringName, pRawName, nameSize * sizeof(WCHAR));
    pName->StringName[nameSize] = 0;

    return true;
}

__success(return != 0)
bool GetDSNAMEBad(
        ULONG bufSize,
        __out_bcount(bufSize) DSNAME *pName,
        ULONG nameSize,
        __in_ecount(nameSize) WCHAR *pRawName
        )
{
    if (bufSize < (offsetof(struct _DSNAME, StringName) + nameSize * sizeof(WCHAR)))
        return false;
    if (nameSize > MAX_WCHAR_IN_DSNAME)
        return false;

    pName->structLen = bufSize;
    pName->SidLen = 5;
    pName->NameLen = nameSize;
    memcpy(pName->StringName, pRawName, nameSize * sizeof(WCHAR));
    pName->StringName[nameSize] = 0;    // BAD. Overflows. ESPX:26019 / PFX:25

    return true;    // BAD. Returning bad buffer. ESPX:26044
}


// Avoid false positives in various scenarios with assumed annotations
// on pointers to variable-sized structs with field annotations.
void ReadDSNAME(const DSNAME *pDN)
{
    // dummy
}
void WriteDSNAME(/* __out */ DSNAME *pDN)
{
    // dummy
    // Q: why do i get warnings for __out annotation for pDN?
}

void
UpdateDSName_NoSAL(
    DSNAME *pDN
    )
{
    ReadDSNAME(pDN);
    WriteDSNAME(pDN);
}

void
UpdateDSName_WithSAL(
    __inout DSNAME *pDN
    )
{
    ReadDSNAME(pDN);
    WriteDSNAME(pDN);
}

void main()
{
    WCHAR pRawName[10] = {L"TEST NAME"};
    ULONG nameSize = 10;

    bool result;
    ULONG bufSize = offsetof(DSNAME, StringName) + (nameSize + 1) * sizeof(WCHAR);
    DSNAME* pDSNAME = (DSNAME*)malloc(bufSize);
    if (pDSNAME)
    {
        result = GetDSNAMEGood(bufSize, pDSNAME, nameSize, pRawName);    // OK
        if (result)
            pDSNAME->StringName[pDSNAME->NameLen] = L'\0';  // OK
        free(pDSNAME);
    }

    bufSize = offsetof(DSNAME, StringName) + nameSize * sizeof(WCHAR);
    pDSNAME = (DSNAME*)malloc(bufSize);
    if (pDSNAME)
    {
        result = GetDSNAMEBad(bufSize, pDSNAME, nameSize, pRawName);     // BAD. Overflow. PFX:25
        if (result)
            pDSNAME->StringName[pDSNAME->NameLen] = L'\0';  // BAD. Overflow. [PFXFN] PREfix does not report this.
        free(pDSNAME);
    }
}