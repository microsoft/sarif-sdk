//Stack buffer overrun in SMTP DNS handler (MS04-035)

#include "specstrings.h"
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

void FindARecord(
    __nullterminated LPWSTR wszStr,
    __out_ecount(*pcIpAddresses) DWORD *rgdwIpAddresses,
    __in ULONG *pcIpAddresses)
{
    ULONG i = 0;
    while(wszStr) {
        if(i > *pcIpAddresses) //check should have been i >= *pcIpAddresses
            break;
 
        rgdwIpAddresses[i] = (DWORD)wszStr; //writing past boundary
        i++;
        wszStr++;
    }
}

void main()
{
    LPWSTR wstr = L"21345";
    ULONG ulIn = wcslen(wstr);
    DWORD* dwOut = (DWORD*)malloc(ulIn * sizeof(DWORD));
    if (dwOut != nullptr)
        FindARecord(wstr, dwOut, &ulIn);
}