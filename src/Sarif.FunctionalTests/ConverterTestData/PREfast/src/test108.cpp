#include "mywin.h"
#include "undefsal.h"

DWORD CountCharsInMultiSzW1(/* __in LPWSTR */ _Null_terminated_ const WCHAR * mwszStrings) // ??? Why using LPCWSTR does not work ???
{
    DWORD cch = 0;

    if (mwszStrings != NULL)
        while (L'\0' != mwszStrings[cch])   // BAD. mwszStrings may not have the extra null-terminator. ESPX26016
            cch += lstrlenW(mwszStrings + cch) + 1;

    return cch + 1;
}

DWORD CountCharsInMultiSzW2(__in __nullnullterminated LPWSTR mwszStrings)
{
    DWORD cch = 0;

    if (mwszStrings != NULL)
        while (L'\0' != mwszStrings[cch])
            cch += lstrlenW(mwszStrings + cch) + 1; // OK

    return cch + 1;
}

void main()
{
    WCHAR* mySzStr1 = L"123456789";
    CountCharsInMultiSzW1(mySzStr1); // BAD. PFX25

    WCHAR* mySzStr2 = L"123456789\0";
    CountCharsInMultiSzW2(mySzStr2); // OK. 
}