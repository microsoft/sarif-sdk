#include <specstrings.h>
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

//
// This function caused EspX to crash due to the memcpy() expression.
// Due to potential aliasing, an IntegerExpr cannot be calculated for the first parameter to memcpy()
// This was not being expected and caused a null-pointer dereference.
// 
DWORD
RemoveString(
    __in                               PSTR          DelString,
    __deref_inout_bcount(*MultiSZSize) PBYTE           *MultiSZ,
    __deref_inout                      PDWORD          MultiSZSize
    )
{
    PSTR szIndex = NULL;
    PSTR szTail = NULL;
    PSTR szSrc = NULL;
    DWORD dwSize = 0;
    PSTR szTemp = NULL;
    PSTR szOldIndex = NULL;
    DWORD dwError = 0;

    szTail = szSrc = (PSTR)(*MultiSZ);
    dwSize = *MultiSZSize - ((strlen((LPSTR)szIndex) + 1) * sizeof(CHAR));

    szTemp = (PSTR)malloc(dwSize);
    if (szTemp) {

        if (dwSize > 1) {

            memcpy(szTemp, szSrc, (PBYTE)szIndex - (PBYTE)szSrc);

            szOldIndex = szIndex;

            szIndex += strlen(szIndex) + 1;

            memcpy(szTemp + (((PBYTE)szOldIndex - (PBYTE)szSrc) / sizeof(CHAR)), szIndex, (PBYTE)szTail - (PBYTE)szIndex);
            *(szTemp + (dwSize / sizeof(CHAR)) - 1) = '\0';

        }

        if (0 == dwError) {

            free(*MultiSZ);
            *MultiSZ = (PBYTE)szTemp;
            *MultiSZSize = dwSize;
        }
    }

    return dwError;
}

