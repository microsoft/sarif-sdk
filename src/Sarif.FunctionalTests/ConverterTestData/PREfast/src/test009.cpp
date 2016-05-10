#if 0
#ifdef _PREFAST_
#define __null                  __declspec("SAL_null")
#define __notnull               __declspec("SAL_notnull")
#define __maybenull             __declspec("SAL_maybenull")
#define __readonly              __declspec("SAL_readonly")
#define __notreadonly           __declspec("SAL_notreadonly")
#define __maybereadonly         __declspec("SAL_maybereadonly")
#define __valid                 __declspec("SAL_valid")
#define __notvalid              __declspec("SAL_notvalid")
#define __maybevalid            __declspec("SAL_maybevalid")
#define __checkReturn           __declspec("SAL_checkReturn")
#define __readableTo(size)      __declspec("SAL_readableTo(" # size ")")
#define __writeableTo(size)     __declspec("SAL_writeableTo(" # size ")")
#define __writableTo(size)      __declspec("SAL_writableTo(" # size ")")
#define __typefix(ctype)        __declspec("SAL_typefix(" # ctype ")")
#define __deref                 __declspec("SAL_deref")
#define __pre                   __declspec("SAL_pre")
#define __post                  __declspec("SAL_post")
#define __excepthat             __declspec("SAL_except")
#else
#define __null
#define __notnull
#define __maybenull
#define __readonly
#define __notreadonly
#define __maybereadonly
#define __valid
#define __notvalid
#define __maybevalid
#define __checkReturn
#define __readableTo(size)
#define __writeableTo(size)
#define __writableTo(size)
#define __typefix(ctype)
#define __deref
#define __pre
#define __post
#define __excepthat
#endif
#endif

// #include <stdlib.h>
#include <memory.h>
#include "mymemory.h"

int main(int chunkSize)
{
    if (chunkSize <= 0)
      return 0;
    int *buf = (int *) malloc(sizeof(int) * (chunkSize * 3 + 1));
    int *buf1 = new int [chunkSize * 3 + 1];

    if (buf != nullptr && buf1 != nullptr)
    {
        int offset1 = chunkSize + 1;
        int offset2 = chunkSize * 2 + 1;
        buf[offset1] = buf[offset2];

        int *buf3 = (int *)realloc(buf, chunkSize * 5 *sizeof(int));
        memcpy(buf1, buf1 + chunkSize, chunkSize * sizeof(int));
        if (buf3 != nullptr)
            buf3[4];
    }
}

