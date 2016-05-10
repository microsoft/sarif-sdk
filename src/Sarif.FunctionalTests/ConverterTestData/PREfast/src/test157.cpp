#include <specstrings.h>
#include "specstrings_new.h"
#include "undefsal.h"

struct MyStruct
{
    int m_cData;
    __field_ecount(m_cData) int m_data[1];
};


void InitMyStruct(
    __out_bcount(cbData) MyStruct *pMS,
    unsigned int cbData
    );


void TestMyStructNoError()
{
    unsigned char buffer[sizeof(MyStruct) + 100 * sizeof(int)];
    MyStruct *pMS = (MyStruct *)&buffer;

    InitMyStruct(pMS, sizeof(buffer));
    pMS->m_data[pMS->m_cData - 1] = 0;
}

void TestMyStructNoError2()
{
    MyStruct ms;
    InitMyStruct(&ms, sizeof(ms));
    ms.m_data[ms.m_cData - 1] = 0;
}

void TestMyStructBufferOverrun()
{
    unsigned char buffer[sizeof(MyStruct) + 100 * sizeof(int)];
    MyStruct *pMS = (MyStruct *)&buffer;

    InitMyStruct(pMS, sizeof(buffer));
    pMS->m_data[pMS->m_cData] = 0;      // warning expected here
}

void TestMyStructBufferOverrun2()
{
    MyStruct ms;
    InitMyStruct(&ms, sizeof(ms));
    ms.m_data[ms.m_cData] = 0;      // warning expected here
}


void UseMyStruct(
    _In_ MyStruct *pMS
    );

_When_(flag == 3, _At_((MyStruct *)pBuffer, _Out_))
void ConditionalInit(
    _Out_writes_bytes_(cbBuffer) void *pBuffer,
    int cbBuffer,
    int flag
    );


void TestMyStructNoError3()
{
    unsigned char buffer[sizeof(MyStruct) + 100 * sizeof(int)];
    MyStruct *pMS = (MyStruct *)&buffer;

    InitMyStruct(pMS, sizeof(buffer));
    UseMyStruct(pMS);
}


void TestMyStructNoError4()
{
    unsigned char buffer[sizeof(MyStruct) + 100 * sizeof(int)];
    MyStruct *pMS = (MyStruct *)&buffer;

    ConditionalInit(pMS, sizeof(buffer), 3);
    UseMyStruct(pMS);     // no error should be reported here
}


