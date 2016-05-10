#include "mywin.h"
#include "specstrings.h"
#include "undefsal.h"

HRESULT GetMachineName(
    __in __nullterminated WCHAR * pwszPath,
    __out_ecount(MAX_COMPUTERNAME_LENGTH+1) WCHAR   wszMachineName[MAX_COMPUTERNAME_LENGTH+1]
    )
{
    WCHAR * pwszServerName;
   
    // Skip the "\\".
    LPWSTR pwszTemp = pwszPath + 2;
 
    pwszServerName = wszMachineName;
    //fix:DWORD dwCount = 0;
    while (*pwszTemp && *pwszTemp != L'\\' ) //fix: && dwCount < MAX_COMPUTERNAME_LENGTH
    {
          *pwszServerName++ = *pwszTemp++;  
          //fix: dwCount++;
    }
    *pwszServerName = 0;
    return S_OK;
}

void main()
{
    WCHAR wMachineName[MAX_COMPUTERNAME_LENGTH+1];

    WCHAR* wstr = L"\\\\MyComputerNameIsLongerThanMax_COMUTERNAME_LENGTH\\share";
    GetMachineName(wstr, wMachineName); // BAD. Will cause overflow. [PFXFN] PREfix doesn't warn overflow - string search.

    WCHAR wstr1[10] = {L'\\', L'\\', L'C', L'O', L'M', L'P', L'U', L'T', L'E', L'R'};
    GetMachineName(wstr1, wMachineName); // BAD. Will cause overflow. [PFXFN] PREfix doesn't warn overflow - string search.
}