#include "specstrings.h"
#include "specstrings_new.h"
#include "mywin.h"
#include "mymemory.h"

void VariableWorker(
        int cmd,
        _When_(cmd != 1 && cmd != 2, __out)
        _When_(cmd == 1, __out_ecount(10))
        _When_(cmd == 2, __out_ecount(20))
        char *ptr
        )
{
    if (cmd == 1)
    {
        memset(ptr, 0, 15);  // buffer overflow. ESPX:26000 / PFX:25
    }
    else if (cmd == 2)
    {
        memset(ptr, 0, 20);  // ok
    }
    else
    {
        *ptr = 0;  // ok
    }
}

void Bad1()
{
    char myArray[15];
    VariableWorker(2, myArray); // BAD. Overflows. ESPX:26000/PFX:25.
}

void Bad2(int cmd)
{
    char myArray[15];
    VariableWorker(cmd, myArray);   // BAD. Can overflow if cmd == 2. ESPX:26000/PFX:25
}

void Good1()
{
    char myArray[15];
    VariableWorker(1, myArray); // OK
    VariableWorker(3, myArray); // OK
}

void RangeWhen1(
    int cmd,
    _When_(cmd == 1, __in_range(<, 100))
    _When_(cmd == 2, __in_range(<, 50))
        unsigned int size
    )
{
    char buff100[100];
    char buff50[50];

    if (cmd == 1)
    {
        buff100[size] = 0;
        buff50[size] = 0;   // buffer overflow here
    }
    if (cmd == 2)
    {
        buff50[size] = 0;
    }
}

_When_(cmd == 0, __range(==, 0))
unsigned int RangeWhen2(
    unsigned int cmd
    )
{
    return cmd + 1;   // should see range violation here
}

void Bad3()
{
    char buff[1];
    buff[RangeWhen2(0)] = 0;   // OK for ESPX per contract. BAD for PFX. Overflow. PFX:23
    buff[RangeWhen2(1)] = 0;   // buffer overflow. ESPX:26017 / PFX:23
}

void WinHttpTestDWORD(
    _When_(dwLength == (DWORD)-1, __in_z)
    _When_(dwLength != (DWORD)-1, __in_ecount(dwLength))
    LPCWSTR lpszHdrs,
    DWORD dwLength
    )
{
    WCHAR ch = '\0';
    DWORD len = dwLength;
    if (len == -1)
    {
        while (*lpszHdrs)
            ch |= *lpszHdrs++;
    }
    else
    {
        for (DWORD i = 0; i < len; ++i)
            ch |= *lpszHdrs++;
    }
}

void WinHttpTestINT(
    _When_(cchLength == -1, __in_z)
    _When_(cchLength != -1, __in_ecount(cchLength))
    const WCHAR *lpszHdrs,
    int cchLength
    )
{
    WCHAR ch = '\0';
    int len = cchLength;
    if (len == -1)
    {
        while (*lpszHdrs)
            ch |= *lpszHdrs++;
    }
    else
    {
        for (int i = 0; i < len; ++i)
            ch |= *lpszHdrs++;  // [ESPXFP] ESPX gives warning 26018, which is not expected. Compare this with WinHttpTestDWORD
    }
}

extern "C" WCHAR* wcscpy(__out_z WCHAR *, __in_z const WCHAR *);

// Let's avoid warning for wcscpy.
void myWcsCpy(__out_z WCHAR *dest, __in_z const WCHAR *src)
{
    wcscpy(dest, src);
}

void CallWinHttpDWORD(
    LPCWSTR myStr
    )
{
    WCHAR localString[100];
    myWcsCpy(localString, myStr);

    WinHttpTestDWORD(localString, (DWORD)-1);  // no overrun

    WinHttpTestDWORD(localString, 100); // no overrun

    WinHttpTestDWORD(localString, -100); // yes overrun.  ESPX:26000 / [PFXFN] PREfix misses this.
}

void CallWinHttpINT(
    LPCWSTR myStr
    )
{
    WCHAR localString[100];
    myWcsCpy(localString, myStr);

    WinHttpTestINT(localString, (int)-1);  // no overrun

    WinHttpTestINT(localString, 100); // no overrun

    WinHttpTestINT(localString, 101); // yes overrun. ESPX:26000 / [PFXFN] PREfix misses this.
}

void main()
{
    char ch;
    char buf10[10];
    char buf20[20];

    VariableWorker(0, &ch);     // OK
    VariableWorker(1, buf10);   // OK for ESPX per contract. BAD for PREfix as VariableWorker is buggy for this case. PFX:25.
    VariableWorker(2, buf20);   // OK

    RangeWhen1(1, 99);  // OK for ESPX per contract. BAD for PFX. Will hit overflow. PFX:27
    RangeWhen1(2, 49);  // OK

    WCHAR myStr[100];
    int i;
    for (i = 0; i < 99; ++i)
        myStr[i] = L'a' + i % 26;
    myStr[i] = '\0';
    CallWinHttpDWORD(myStr);    // Should get overflow warning. [PFXFN]
    CallWinHttpINT(myStr);      // Should get overflow warning. [PFXFN]
}
