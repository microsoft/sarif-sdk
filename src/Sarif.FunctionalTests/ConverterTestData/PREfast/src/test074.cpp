#include "specstrings.h"
#include "mywin.h"
#include "undefsal.h"

HRESULT StrCchCopy(__out_ecount(size) PSTR dest, const char *src, size_t size)
{
    if (src == nullptr)
        return -1;

    for (size_t i = 0; i < size - 1 && *src; ++i)
        *dest++ = *src++;

    *dest = '\0';

    return 0;
}

void buggy(const char *src)
{
    char buf[100];

    StrCchCopy(buf, src, 100);      // BAD. Return not checked.

    for (char *p = buf; *p; p++)    // BAD. Potential overflow. PREfix reports as uninit memory access for *p.
	(*p) = 2;                   // BAD. Potential overflow. PREfix reports as uninit memory access for *p.
}

void correct(const char *src)
{
    char buf[100];

    if (StrCchCopy(buf, src, 100) > 0)
    {
        for (PSTR p = buf; *p; p++) // OK
	    (*p) = 2;               // OK
    }
}

void GetFolder(__out_ecount(MAX_PATH) PWSTR s)
{
    for (int i = 0; i < MAX_PATH - 1; ++i)
        *s++ = 'a' + i / 26;
    *s = '\0';
}

void Copy(__out_ecount(size) PWSTR dest, __in PWSTR src, size_t size)
{
    for (int i = 0; i < size - 1 && *src; ++i)
        *dest = *src;
    *dest = '\0';
}

void f1()
{
    WCHAR buf[MAX_PATH];
    WCHAR buf1[MAX_PATH];

    GetFolder(buf);
    Copy(buf1, buf, wcslen(buf) + 1);
}

void main()
{
    char* str = "This is my test string.";
    buggy(str);
    buggy(nullptr); // Will cause buggy to hit the bug. But PREfix already reported the error for function buggy.
    correct(str);
    correct(nullptr);
}

