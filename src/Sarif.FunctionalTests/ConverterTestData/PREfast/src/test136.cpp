#include "specstrings.h"
#include "specstrings_new.h"
#include "mymemory.h"
#include "undefsal.h"

#define PAGE_SIZE 1024

#define NULL 0

typedef void *PVOID;
typedef short CSHORT;
typedef unsigned long ULONG;



//
// Starting test case for bug824
//


typedef
    __struct_xcount(sizeof(struct _MyMDL) +
        (ByteOffset + ByteCount + PAGE_SIZE-1) / PAGE_SIZE * sizeof(PFN_NUMBER))
  struct _MyMDL {
    struct _MDL *Next;
    CSHORT Size;
    CSHORT MdlFlags;
    struct _EPROCESS *Process;
    PVOID MappedSystemVa;   /* see creators for field size annotations. */
    _At_(StartVa+ByteOffset,            // ESPX BUG HERE: doesn't recognize
                                        // the sum as annotating the location
        __field_bcount_opt(ByteCount))
    PVOID StartVa;
    ULONG ByteCount;
    ULONG ByteOffset;
} MyMDL, *MyPMDL;

typedef __inexpressible_readableTo(polymorphism) MyMDL *PMyMDLX;

typedef int MEMORY_CACHING_TYPE;

__bcount(MemoryDescriptorList->ByteCount)
_At_(
    MemoryDescriptorList->MappedSystemVa + MemoryDescriptorList->ByteOffset, // SAME ESPX BUG HERE
    __post __bcount(MemoryDescriptorList->ByteCount))
__checkReturn
__success(return != NULL)
PVOID
MyMmMapLockedPagesWithReservedMapping (
    __in    PVOID MappingAddress,
    __in    ULONG PoolTag,
    __inout PMyMDLX MemoryDescriptorList,
    __in    MEMORY_CACHING_TYPE CacheType
    )
{
    char *pBuf = nullptr;
    if (MemoryDescriptorList)
    {
        PVOID pSysVa = malloc(MemoryDescriptorList->ByteOffset + MemoryDescriptorList->ByteCount);
        if (pSysVa)
        {
            pBuf = (char*)malloc(MemoryDescriptorList->ByteCount);
            if (pBuf)
            {
                MemoryDescriptorList->MappedSystemVa = pSysVa;
            }
            else
            {
                free(pSysVa);
            }
        }
    }

    return pBuf;
}


// As of the creation of this test case, the expected output file does not
// contain all defects that it should.  EspX still has some false negatives,
// as detailed in the comments in this file.
void bar1(
     __inout PMyMDLX mdl,
     __in PVOID ma,
     __in MEMORY_CACHING_TYPE ct,
     __in ULONG p
     )
{
    PVOID bp;
    if (mdl == NULL)
        return;
    bp = MyMmMapLockedPagesWithReservedMapping(ma, 'abcd', mdl, ct);

    if (!bp) return;

    //mdl->ByteCount = 100; // [hwisungi] These seem redundant...
    //mdl->ByteOffset = 100;

    ((char*)mdl->MappedSystemVa)[0] = 0; //Geoffrey: 26010 when ByteCount could be 0; 26001 when ByteOffset may be > 0
    ((char*)mdl->MappedSystemVa)[mdl->ByteCount] = 0; //[hwisungi: ???] Geoffrey: 26001 when ByteCount may be < ByteOffset.  ESPX reports 26017. Seems FP [ESPXFP?] 

    ((char *)bp)[-1] = 0;  // BAD. Underflow. ESPX:26001 / PFX:24
    ((char *)bp)[0] = 0; // Geoffrey: 26010 when ByteCount could be 0
    ((char *)bp)[mdl->ByteCount] = 0; // [hwisungi: ???] 6386, 26017, 26045. BAD. Overflow. ESPX:26000 / PFX:23

    free(bp);
}


//
// Starting test case for bug672
//

void GetData(
    __deref_inout
    //__deref_out_bcount(*pdwSize)
    _When_(pdwSize != NULL, __deref_out_bcount(*pdwSize))
    _When_(pdwSize == NULL, __deref_out)
    char **pData,
    __out_opt unsigned int *pdwSize
    )
{
    if (!pData)
        throw "bad pointer";

    int* pBuf = nullptr;
    if (pdwSize)
    {
        if (*pdwSize <= 0)
            throw "bad size";

        *pData = (char*)malloc(*pdwSize);
        if (*pData)
            memset(*pData, 1, *pdwSize);
        else
            *pdwSize = 0;
    }
    else
    {
        *pData = (char*)malloc(1);
        if (*pData)
            **pData = 1;
    }
}

void UseData()
{
    unsigned char *data;
    unsigned int size = 2;

    GetData((char **)&data, &size);
    if (data)
    {
        if (size >= 2)
            data[1] += 1;  // no overrun
        free(data);
    }

    GetData((char **)&data, NULL);
    if (data)
    {
        data[0] += 1;   // OK. no overrun
        data[1] = 1;    // BAD. overrun. ESPX:26006 / PFX:23
        free(data);
    }
}

void main()
{
    MyMDL mdl;
    mdl.ByteOffset = 100;
    mdl.ByteCount = 100;
    bar1(&mdl, nullptr, (MEMORY_CACHING_TYPE)3, (ULONG)5);
}
