#include <specstrings.h>
#include "specstrings_new.h"
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

__range(==, _String_length_(s)) unsigned int MyStrLen(__in_z char *s)
{
    return strlen(s);
}

#define TESTSTR "abc"

void TestStrLenPrimOp1()
{
    char buf[4];
    unsigned int n = MyStrLen(TESTSTR);
    buf[n+2] = 0;   // buffer overrun expected here. ESPX:26000 / PFX:23

    n = sizeof(TESTSTR);
    buf[n-1] = 0;   // no buffer overrun
}

void TestStrLenPrimOp2()
{
    char buf[4];
    unsigned int n = MyStrLen(TESTSTR);

    if (n + 1 != sizeof(TESTSTR))
        buf[10] = 0;   // no buffer overrun as this is unreachable
}


// Now test _String_length_ on WCHAR

typedef unsigned short WCHAR;

#define TESTWSTR L"abc"

__range(==, _String_length_(s)) unsigned int MyWcsLen(__in_z WCHAR *s)
{
    return wcslen(s);
}

void TestWcsLenPrimOp1()
{
    WCHAR buf[4];
    unsigned int n = MyWcsLen(TESTWSTR);
    buf[n+2] = 0;   // buffer overrun expected here. ESPX:26000 / PFX:23

    n = sizeof(TESTWSTR) / sizeof(WCHAR);
    buf[n-1] = 0;   // no buffer overrun
}

void TestWcsLenPrimOp2()
{
    WCHAR buf[4];
    unsigned int n = MyWcsLen(TESTWSTR);

    if (n + 1 != sizeof(TESTWSTR) / sizeof(WCHAR))
        buf[10] = 0;   // no buffer overrun as this is unreachable
}

// Test to make sure that a buffer which is sometimes used as single
// byte element and sometimes as two byte elements doesn't confuse things.
// This test generates a correct warning, but the description should only
// describe the size of an 89 element [2 bytes/element] buffer.  It was
// incorrectly (Esp:751) also describing that buffer as a
// 178 element [2 bytes/element] null terminated buffer.

struct CountedString
{
    unsigned int Length;
    __field_bcount(Length) WCHAR *Buffer;
};

_At_(pCS->Buffer, __out_range(==, source))
_At_(pCS->Length, __out_range(==, (_String_length_(source)+1) * sizeof(WCHAR)))
void InitCountedString(
    __out CountedString *pCS,
    __in_z WCHAR *source
    )
{
    if (pCS)
    {
        pCS->Buffer = source;
        pCS->Length = MyWcsLen(source) * sizeof(WCHAR);
    }

    throw "sorry";
}

void UseCountedString(
    __in CountedString *pCS
    )
{
    if (pCS)
    {
        WCHAR ch = pCS->Buffer[pCS->Length / sizeof(WCHAR) - 1];    // OK
    }
}

void TestCountedString(__in_ecount(100) WCHAR *inString)
{
    CountedString cs;
    WCHAR wBuf[100];
    unsigned int cb = 180 - sizeof(WCHAR);

    memcpy(wBuf, inString, cb);
    wBuf[cb/sizeof(WCHAR)] = 0;
    if (MyWcsLen(wBuf) != MyStrLen((char *)wBuf) * 2)
        wBuf[200] = 0;  // BAD. ESPX:26000 / PFX:23
    InitCountedString(&cs, wBuf);
    UseCountedString(&cs);
}

void main() { /* dummy */ }