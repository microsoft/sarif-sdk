#include <specstrings.h>
#include "undefsal.h"


extern "C" void *memset(__out_bcount(count) void *dest, int c, size_t count);

struct MyStruct
{
    int m_used;
    char m_data[100];
};

void Test1()
{
    MyStruct ms;
    unsigned int offset = sizeof(int); //((char *)&ms.m_data - (char *)&ms);

    ms.m_used = 0;
    memset((char *)&ms + offset, 0xff, sizeof(ms.m_data));
    ms.m_data[ms.m_used] = 0;     // no buffer overrun since m_used can't be
                                  // modified by memset call
}


//
// Below is repro case from Esp:662
//

typedef __struct_bcount(StructSize) struct _STRUCT
{
    __field_range(>=, sizeof(STRUCT))
    unsigned long StructSize;
} STRUCT, *PSTRUCT;

void UseStruct(
    __in PSTRUCT Struct
    );

int Test2()
{
    unsigned char Buffer[100];
    PSTRUCT Struct = (PSTRUCT)Buffer;

    Struct->StructSize = sizeof(Buffer);

    memset(Struct + 1, 0, sizeof(Buffer) - sizeof(STRUCT));

    UseStruct(Struct);

    return 0;
}


