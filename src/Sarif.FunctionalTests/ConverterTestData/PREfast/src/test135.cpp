#include <specstrings.h>
#include "undefsal.h"

typedef unsigned char BYTE;
typedef BYTE *PBYTE;

typedef struct _SOCKET { int x; } SOCKET;

int MyRecv(
    __in            SOCKET s,
    __out_bcount_part(len, return) __out_data_source(NETWORK) char* buf,
    __in            int len,
    __in            int flags
    )
{
    static unsigned magic;
    int received = 0;
    if (buf && len > 0)
    {
        if (magic == 0)
            magic = 2;
        received = len - (unsigned)len / magic;
        for (int i = 0; i < received; ++i)
            buf[i] = 'a' + i % 26;
    }

    return received;
}

typedef int ( *_pfn_recv)(
    __in            SOCKET s,
    __out_bcount_part(len, return) __out_data_source(NETWORK) char* buf,
    __in            int len,
    __in            int flags
    );

_pfn_recv GetF() { return MyRecv; }

int
VfHookrecv( 
    __in          SOCKET s,
    __out_bcount_part(len, return) __out_data_source(NETWORK) char* buf,
    __in          int len,
    __in          int flags )
{
    typedef _pfn_recv FUNCTION_TYPE;

    FUNCTION_TYPE originalFunc = GetF();

    int ReturnValue;

    ReturnValue = (*originalFunc)(s, buf, len, flags);

    return ReturnValue; 
}


// Below is repro case for Esp:718

typedef void (*pfnGetData)(
    __out_bcount_part(*pcbDataLen, *pcbDataLen)
    PBYTE  pData,
    __inout  int   *pcbDataLen
    );

void WrapGetData(
    pfnGetData  pDataFunc,
    __out_bcount_part(*pcbDataLen, *pcbDataLen)
    PBYTE  pData,
    __inout     int   *pcbDataLen
    )
{
    pDataFunc(pData, pcbDataLen);   // false positive 26045 reported here
}

void WrapWrapGetData(
    pfnGetData  pDataFunc,
    __out_bcount_part(*pcbDataLen, *pcbDataLen)
    PBYTE  pData,
    __inout     int   *pcbDataLen
    )
{
    WrapGetData(pDataFunc, pData, pcbDataLen);   // no warning reported here
}

void MyGetData(
    __out_bcount_part(*pcbDataLen, *pcbDataLen)
    PBYTE  pData,
    __inout  int   *pcbDataLen
    )
{
    static unsigned magic;
    int received = 0;
    if (pData && pcbDataLen && *pcbDataLen > 0)
    {
        if (magic == 0)
            magic = 2;
        received = *pcbDataLen - (unsigned)*pcbDataLen / magic;
        for (int i = 0; i < received; ++i)
            pData[i] = 'a' + i % 26;

        *pcbDataLen = received;
    }
    else if (pcbDataLen)
    {
        *pcbDataLen = 0;
    }
}

void main()
{
    SOCKET s;
    s.x = 1;
    char buf[100];
    int received = VfHookrecv(s, buf, 100, 0x010101);   // OK
    if (received > 0)
    {
        char ch = buf[received - 1];    // OK
    }

    BYTE data[100];
    WrapWrapGetData(MyGetData, data, &received);    // OK
    if (received > 0)
        BYTE b = data[received - 1];    // OK
}