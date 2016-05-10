#include <sal.h>

struct MyStruct
{
    int x_;
    int y_;
};

void GetBuffer(
    _Outptr_result_bytebuffer_(size) void **pBuffer,
    int size
    );

void UseStruct(
    _In_reads_(_Inexpressible_(varies)) MyStruct *pMS
    );

void Test(int n)
{
    MyStruct *pMS;

    GetBuffer((void **)&pMS, n);
    UseStruct(pMS);
}

// Test case from Esp:868
typedef _NullNull_terminated_ char *PZZSTR;

_Success_(return)
bool VerifyMultSz(
    _In_reads_(2) char *buf,
    _Outptr_ PZZSTR *pZZ
    )
{
    if (buf[0] == '\0' && buf[1] == '\0')
    {
        *pZZ = buf;
        return true;
    }

    return false;
}

