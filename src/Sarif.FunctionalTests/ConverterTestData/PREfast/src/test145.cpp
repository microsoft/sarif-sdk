#include "specstrings.h"
#include "undefsal.h"

#define ERROR_SUCCESS 0

typedef void *HKEY;
#define HKEY_CURRENT_USER (HKEY)100
#define HKEY_LOCAL_MACHINE (HKEY)101

typedef unsigned char *LPBYTE;
typedef __nullterminated char * LPSTR;
typedef __nullterminated const char * LPCSTR;
typedef unsigned short WCHAR;
typedef __nullterminated WCHAR * LPWSTR;
typedef __nullterminated const WCHAR * LPCWSTR;
typedef long LONG;
typedef unsigned long DWORD;


__success(return == ERROR_SUCCESS)
LONG SHRegGetValueW(
    __in HKEY hkey,
    __in_opt LPCWSTR pszSubKey,
    __in_opt LPCWSTR pszValue,
    __out_opt DWORD *pdwType,
    __typefix(LPBYTE) __out_bcount_part_opt(*pcbData, *pcbData) void *pvData,
    __inout_opt DWORD *pcbData
    );

__success(return == ERROR_SUCCESS)
LONG __SHRegGetValueFromHKCUHKLM(
    __in LPCWSTR pwszKey,
    __in_opt LPCWSTR pwszValue,
    __out_opt DWORD* pdwType,
    __out_bcount_part_opt(*pcbData, *pcbData)
    __typefix(LPBYTE) void* pvData,
    __inout_opt DWORD * pcbData
    )
{
    if (!pcbData)
        return 1;
        
    DWORD cbData = pcbData ? *pcbData : 0;
    LONG lr = SHRegGetValueW(HKEY_CURRENT_USER, pwszKey, pwszValue, pdwType, pvData, pcbData);
    if (lr != ERROR_SUCCESS)
    {
        if (pcbData)
        {
            *pcbData = cbData; // just in case first call modified value (identified by prefast error)
        }
        lr = SHRegGetValueW(HKEY_LOCAL_MACHINE, pwszKey, pwszValue, pdwType, pvData, pcbData);  // bogus warning 2015 here.
    }
    return lr;
}

