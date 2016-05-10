#include <specstrings.h>
#include "specstrings_new.h"
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

#define STRSAFE_MAX_CCH     2147483647

HRESULT
StringCchLengthW(
    __in_z_opt wchar_t* psz,
    __in __in_range(1, STRSAFE_MAX_CCH) size_t cchMax,
    __out_opt __deref_out_range(<, cchMax) __deref_out_range(<=, _String_length_(psz)) size_t* pcchLength)
{
    if (!psz || !pcchLength || cchMax > STRSAFE_MAX_CCH)
        return (HRESULT)0x80070057L; // STRSAFE_E_INVALID_PARAMETER

    size_t len = wcslen(psz);
    if (len >= cchMax)
        return (HRESULT)0x80070057L; // STRSAFE_E_INVALID_PARAMETER

    *pcchLength = len;
    return 0;
}


//
// Repro case for bug813
//

struct MyStructA
{
    __nullterminated char m_string[100];
    int m_data;
};

struct MyStructW
{
    __nullterminated wchar_t m_string[100];
    int m_data;
};

void MyCopyA(
    __in MyStructA *pIn,
    __out MyStructA *pOut
    )
{
    memcpy(pOut->m_string, pIn->m_string, strlen(pIn->m_string) + 1);
    pOut->m_string[100] = 0;   // buffer overrun expected here, but not previous line. ESPX:26000 / [PFXFN] PREfix thinks this is OK. 
}

void MyCopyW(
    __in MyStructW *pIn,
    __out MyStructW *pOut
    )
{
    memcpy(pOut->m_string, pIn->m_string, (wcslen(pIn->m_string) + 1) * 2);
    pOut->m_string[100] = 0;   // buffer overrun expected here, but not previous line. ESPX:26000 / [PFXFN] PREfix thinks this is OK. pOut->m_string[102] will get caught.
}


//
// Repro case for bug841
//

extern void UseBuffer(
    __in_bcount(BufferLength) void *Buffer,
    __in size_t BufferLength
    )
{
    char ch = 'a';
    for (int i = 0; i < BufferLength; ++i)
        ch |= ((char*)Buffer)[i];
}

void UseString(
    __in PWSTR String
    )
{
    size_t StringLength;
    if (FAILED(StringCchLengthW(String, STRSAFE_MAX_CCH, &StringLength)))
    {
        return;
    }

    size_t BufferLength = (StringLength + 1) * sizeof(wchar_t);

    UseBuffer(String, BufferLength);
}


//
// Repro case for bug695
//

#define VALUE 0xa

void doStuff() { }

void
string_iterate(
    __in PWSTR pString
    )
{
    PWSTR pStart = pString;
    PWSTR pEnd = pStart;

    while (*pEnd)
    {
        pStart = pEnd + 1;

        if (*pStart == VALUE)
        {
            doStuff();
        }

        pEnd = pStart;
    }
}

void main()
{
    MyStructA aIn, aOut;
    for (int i = 0; i < 99; ++i)
        aIn.m_string[i] = 'a' + i % 26;
    aIn.m_string[99] = '\0';

    MyCopyA(&aIn, &aOut);   // [PFXFP] REfix reports warning 1 for aIn.m_string[1] - plain wrong. Ignore it for this test. 

    MyStructW wIn, wOut;
    for (int i = 0; i < 99; ++i)
        wIn.m_string[i] = L'a' + i % 26;
    wIn.m_string[99] = L'\0';

    MyCopyW(&wIn, &wOut);   // [PFXFP] REfix reports warning 1 for aIn.m_string[1] - plain wrong. Ignore it for this test. 

    wchar_t* myStr = L"this is my test string!";
    UseString(myStr);       // OK

    string_iterate(myStr);  // OK
}
