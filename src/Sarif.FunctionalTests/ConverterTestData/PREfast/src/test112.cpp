// MSRC 6812
#include "specstrings.h"
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"
 
DWORD
Receive(
        __deref_inout_bcount_part(*lpdwBufferLength, *lpdwBytesReceived) LPVOID * lplpBuffer,
        __inout LPDWORD lpdwBufferLength,
        __inout LPDWORD lpdwBufferRemaining,
        __inout LPDWORD lpdwBytesReceived
        )
{
    DWORD rc = 0;
    if (!lplpBuffer || !lpdwBufferLength || !lpdwBufferRemaining || !lpdwBytesReceived)
        return -1;

    static int bytes;

    // Let's help PREfix to find bugs in careless callers.
    *lplpBuffer = malloc(bytes);
    *lpdwBufferLength = *lpdwBytesReceived = bytes;
    *lpdwBufferRemaining = 0;

    return bytes;
}
  
void main(void)
{
    LPSTR pchBuffer;
    DWORD bufferLength;
    DWORD bufferLeft;
    DWORD bytesReceived;
    DWORD bytesChecked;
    //
    // initialize variables (for SocketReceive())
    //
    pchBuffer = 0;
    bufferLength = 0;
    bufferLeft = 0;
    bytesReceived = 0;
    bytesChecked = 0;
    Receive((LPVOID *)&pchBuffer, &bufferLength, &bufferLeft, &bytesReceived);
    if (pchBuffer)
        pchBuffer[bytesReceived] = '\0';    // BAD. Potential overflow if bufferLength == bytesReceived. ESPX:26017 / PFX:23
}
