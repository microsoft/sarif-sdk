#include <specstrings.h>

#include "undefsal.h"

// Adapted from __deref_in_ecount_iterator, but couldn't get that to work with the
// version of nmm checked into the esp depot.  However this works.
#define __deref_in_ecount_my(size)  __inout_ecount(1) __pre __deref __elem_readableTo(size)  // TODO: hwisungi: TEST TEST TEST: Is this complete?  __inout __pre __deref __elem_readableTo(size)


typedef unsigned short WORD;
typedef unsigned int DWORD;

WORD GetWord(
    __deref_in_ecount_my(sizeof(WORD)) __deref_out_range(==, pre(*pBuf) + sizeof(WORD)) char **pBuf // why do we use sizeof(WORD) for _my?
    )
{
    WORD result;
    if (pBuf) // TODO: hwisungi: TEST TEST TEST: if (pBuf && *pBuf && *pBuf + 1)
    {
        result = (*(*pBuf + 1) << (sizeof(WORD) / 2)) + **pBuf;
        *pBuf += sizeof(WORD);
        return result;
    }

    throw "sorry";
}

WORD BadGetWord1(
    __deref_in_ecount_my(sizeof(WORD)) __deref_out_range(== , pre(*pBuf) + sizeof(WORD)) char **pBuf
    )
{
    WORD result = GetWord(pBuf);
    return **(WORD **)pBuf;   // expect buffer overflow warning here. [ESPXFN] / PFX:25 
}

WORD BadGetWord2(
    __deref_in_ecount_my(sizeof(WORD)) __deref_out_range(==, pre(*pBuf) + sizeof(WORD)) char **pBuf
    )
{
    WORD result = **(WORD **)pBuf;
    *pBuf += sizeof(WORD);
    return **(WORD **)pBuf;   // expect buffer overflow warning here. ESPX:26000 / PFX:25
}

DWORD GetDWord(
    __deref_in_ecount_my(sizeof(DWORD)) __deref_out_range(==, pre(*pBuf) + sizeof(DWORD)) char **pBuf
    )
{
    DWORD result;

    result = GetWord(pBuf) << 16;
    result = result + GetWord(pBuf);

    return result;     // no warning expected here
}

void main()
{
    try
    {
        WORD *pBuf1;
        WORD buf1[1] = { 100 };
        WORD res1;

        pBuf1 = buf1;
        res1 = BadGetWord1((char**)&pBuf1);   // BAD.

        pBuf1 = buf1;
        res1 = BadGetWord2((char**)&pBuf1);   // BAD.

        DWORD *pBuf2;
        DWORD buf2[1] = { 100 };
        DWORD res2;

        pBuf2 = buf2;
        res2 = GetDWord((char**)&pBuf2);    // OK.
    }
    catch(...)
    {
    }
}
