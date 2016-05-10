#include <specstrings.h>
#include "undefsal.h"

typedef unsigned short wchar_t;
typedef __w64 long __time32_t;
typedef __time32_t time_t;
typedef wchar_t WCHAR;
typedef WCHAR TCHAR, *PTCHAR;
typedef unsigned char UCHAR;
typedef UCHAR BOOLEAN;
typedef unsigned long       DWORD;
typedef int                 BOOL;

__declspec(dllimport) char * __cdecl ctime(const time_t *);
__declspec(dllimport) time_t __cdecl time(time_t *);

// note, OUT not IN!
template <int N> void safesprintf(__out_ecount(N) wchar_t (&buffer)[N], __in const wchar_t *pszFormat, ...)
{
    buffer[0]=0;
}

BOOL SaveJobInfo()
{
    TCHAR szUser[260];
    DWORD nSize = sizeof(szUser)/sizeof(TCHAR);
    TCHAR szTime[64];
    time_t now;
    time(&now);

    //safesprintf(szTime, L"%hs", ctime(&now));
    //safesprintf<64>(szTime, L"%hs", ctime(&now));
    safesprintf<64>(szTime, L"%hs", "12345");
 
    return 1;
}
