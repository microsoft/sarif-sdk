#include <specstrings.h>
#include "undefsal.h"

struct MyStruct { int one; int two; };

_Ret_writes_bytes_maybenull_(size) void* MemAlloc(size_t size);

void ValidAliasing(
    _Out_ unsigned long* pdwPeerInfoCount, // Count of peers returned in the array
    _Outptr_result_buffer_(*pdwPeerInfoCount) MyStruct** ppPeerInformation // pointer to the array    
    )
{
    *ppPeerInformation = 0;
    *pdwPeerInfoCount = 0;
    MyStruct* pPeerInfoList = (MyStruct*) MemAlloc(sizeof(MyStruct) * 10);

    *ppPeerInformation = pPeerInfoList; // alias
    for (int i = 0; i < 10; i++) {
        pPeerInfoList->one = 1;
        pPeerInfoList->two = 2;
        pPeerInfoList++; // this should break the alias with *ppPeerInformation
    }
    *pdwPeerInfoCount = 10;
}

void InvalidAliasing(
    _Out_ unsigned long* pdwPeerInfoCount, // Count of peers returned in the array
    _Outptr_result_buffer_(*pdwPeerInfoCount) MyStruct** ppPeerInformation // pointer to the array    
    )
{
    *ppPeerInformation = 0;
    *pdwPeerInfoCount = 0;
    MyStruct* pPeerInfoList = (MyStruct*) MemAlloc(sizeof(MyStruct) * 10);

    *ppPeerInformation = pPeerInfoList; // alias
    for (int i = 0; i < 10; i++) {
        pPeerInfoList->one = 1;
        pPeerInfoList->two = 2;
        pPeerInfoList++; // this should break the alias with *ppPeerInformation
        *ppPeerInformation = pPeerInfoList; // this sets up the alias again
    }
    *pdwPeerInfoCount = 10; // so by here, our pointer is not adhering to the postcondition    
}
