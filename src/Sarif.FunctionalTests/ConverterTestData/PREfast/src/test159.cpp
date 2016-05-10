#include <specstrings.h>

typedef unsigned char wchar_t;

extern "C" void memset(
    __out_bcount(cnt) _At_((unsigned char *)dst, __deref_out_range(==, c)) void *dst,
    int c,
    unsigned int cnt);

#define ZeroMemory(_ptr, _cnt) memset(_ptr, 0, _cnt)

struct Struct1
{
    __field_range(0, 32-1)
    int count;
    wchar_t buf[32];
};

struct Struct2
{
    __field_range(0, 32-1)
    int count;
    wchar_t buf[32];
};

bool Combine(Struct1 *pStruct1)
{
    if (!pStruct1)
    {
        return false;
    }

    //Adding this initializer instead of relying on ZeroMemory resovled the
    //false positive that we hit before Esp:716 was fixed.
    //Struct2 struct2 = { 0 };   <--- HERE
    Struct2 struct2;
    ZeroMemory(&struct2, sizeof(Struct2));

    for (int i = 0; i < pStruct1->count; ++i)
    {
        int j = 0;
        for ( ; j < struct2.count; ++j)
        {
/*
            if (pStruct1->buf[i] == struct2.buf[j])
            {
               break;
            }
*/
        }

        if (j == struct2.count)
        {
            struct2.buf[j] = pStruct1->buf[i];
            ++struct2.count;
        }
    }

    return true;
}

