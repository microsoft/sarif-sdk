#include <specstrings.h>
#include "mymemory.h"
#include "undefsal.h"

#define FIELD_OFFSET(type, field)    ((unsigned long)(unsigned long *)&(((type *)0)->field))

typedef struct {
    unsigned nMult;
    unsigned nX;
    char data[1];
} MyBuffer;

void fill(
    unsigned x,
    unsigned y,
    __out_bcount(x * y) char *pBuffer
    )
{
    if (pBuffer && x * y > 0)
    {
        for (int i = 0; i < x * y; ++i) 
            *(pBuffer++) = 'a' + i % 26;
    }
}

__success(return != 0)
bool foo(
    __inout_bcount_part(*pSize, *pSize) MyBuffer *pBuffer,
    __inout unsigned *pSize
    )
{
    if (*pSize < FIELD_OFFSET(MyBuffer, data))
        return false;

    unsigned sizeData = pBuffer->nMult * pBuffer->nX;
    unsigned size = FIELD_OFFSET(MyBuffer, data) + sizeData;

    if (*pSize < size)
        return false;

    fill(pBuffer->nMult, pBuffer->nX, pBuffer->data);
    *pSize = size;
    return true;
}

void main()
{
    unsigned size = FIELD_OFFSET(MyBuffer, data) + 200; // Reserve 200 bytes for the data
    MyBuffer* pBuf = (MyBuffer*)malloc(size);
    if (pBuf)
    {
        pBuf->nMult = 1;
        pBuf->nX = 200;
        foo(pBuf, &size);
    }
}