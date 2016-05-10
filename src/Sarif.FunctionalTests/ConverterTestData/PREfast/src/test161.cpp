#include <sal.h>

#define NULL 0

_At_buffer_((char *)dest, _I_, count, _Post_equal_to_(c))
extern "C" void *memset(
   _Out_writes_bytes_(count) void *dest,
   int c,
   size_t count 
);

#define ZeroMemory(dest, size) memset((dest), 0, (size))

typedef unsigned int DWORD;
typedef unsigned char BYTE;
typedef BYTE *LPBYTE;

typedef struct _MyStruct
{
    DWORD dwSize;
    LPBYTE pBuffer;
    DWORD dwCount;
} MYSTRUCT;

 typedef struct _AnotherStruct {
     MYSTRUCT   MyHeader;
     BYTE    Buffer[16];
} ANOTHERSTRUCT;


int __cdecl wmain()
{
    ANOTHERSTRUCT AnotherStruct;
    MYSTRUCT* pMyStruct;

    ZeroMemory(&AnotherStruct, sizeof(AnotherStruct));

    pMyStruct = &(AnotherStruct.MyHeader);

    pMyStruct->dwSize = sizeof(AnotherStruct.MyHeader);
    pMyStruct->pBuffer = AnotherStruct.Buffer;
    pMyStruct->pBuffer[pMyStruct->dwCount++] = NULL;

}
