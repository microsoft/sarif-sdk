#include "specstrings.h"
#include "mywin.h"
#include "undefsal.h"

extern "C" char* __cdecl strcpy(_Out_writes_z_(_String_length_(src) + 1) char* dst, _In_z_ const char* src);

void f(__in PSTR s)
{
    char buf[100];
    strcpy(buf, s); // BAD. Potential overflow.
}

void g(__in PSTR s)
{
    char buf[100];
    if (lstrlenA(s) <= 100)
	lstrcpyA(buf, s);   // BAD. Potential overflow. Above check should be < 100
}

void h(__in PWSTR s)
{
    WCHAR buf[100];
    if (lstrlenW(s) <= 100)
	lstrcpyW(buf, s);   // BAD. Potential overflow. Above check should be < 100
}

void foo(__in PWSTR s)
{
    WCHAR buf[100];
    if (wcslen(s) < 10)
    {
	lstrcpynW(buf, s, 100);
	lstrcpynW(buf, s, sizeof(buf)); // BAD. Should be sizeof(buf)/sizeof(buf[0])
    }
}

void foo1(__in PWSTR s)
{
    WCHAR buf[100];
    if (lstrlenW(s) < 10)
    {
        lstrcpynW(buf, s, 100);
        lstrcpynW(buf, s, sizeof(buf)); // BAD. Should be sizeof(buf)/sizeof(buf[0])
    }
}

void bar(__inout_ecount(100) PWSTR buf, __in PWSTR s)
{
    lstrcatW(buf, s);
}

void main()
{
    char* strA = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"; // strlen = 100
    f((PSTR)strA);  // BAD. Overflows local buffer in f.
    g((PSTR)strA);  // BAD. Overflows local buffer in g.

    wchar_t* strW = L"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"; // wcslen = 100
    h((PWSTR)strW); // BAD. Overflows local buffer in g.

    wchar_t* strW2 = L"123456789"; // wcslen < 10
    foo((PWSTR)strW2);  // BAD. Note that PREfix already reported bug in foo.
    foo1((PWSTR)strW2); // BAD. Note that PREfix already reported bug in foo1.

    wchar_t buf[100] = L"my test string.";
    bar((PWSTR)buf, (PWSTR)strW);   // BAD. Overflows buf.
}