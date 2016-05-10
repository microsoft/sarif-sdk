#include "specstrings.h"

void qcolon(__out_ecount((flag > 0 ? size : 256) + (flag1 > 0 ? size : 256)) char *buf, size_t size, int flag, int flag1)
{
    if (flag > 0)
    {
        for (size_t i = 0; i < size + size; i++)
            buf[i] = 1;     // BAD. May overflow. Second 'size' is not related to flag, but flag1.
    }
    else
    {
        for (size_t i = 0; i < 512; i++)
            buf[i] = 1;     // BAD. May overflow. Second '256' is not related to flag, but flag1.
    }
}

void qcolon_good(__out_ecount((flag > 0 ? size : 256) + (flag1 > 0 ? size : 256)) char *buf, size_t size, int flag, int flag1)
{
    size_t sz = (flag > 0 ? size : 256) + (flag1 > 0 ? size : 256);
    for (size_t i = 0; i < sz; i++)
        buf[i] = 1;
}

void callqcolon()
{
    char a[10];
    qcolon(a, 10, 1, 1);  // bad    [PFXFN] PREfix misses this. Maybe due to loop count calculated from parameter value.

    char b[10];
    qcolon(a, 5, 1, 1); // good

    char c[100];
    qcolon(c, 100, 0, 1); // bad

    char d[512];
    qcolon(c, 100, 0, 0); // good
}

void TestAnd(__out_ecount(flag && flag1 ? size: 256) char *buf, size_t size, bool flag, bool flag1)
{
    if (flag)
    {
        for (size_t i = 0; i < size; i++)
            buf[i] = 1; // BAD. Can overflow. Buffer size depends both on flag and flag1
    }
}

void GoodTestAnd(__out_ecount(flag && flag1 ? size: 256) char *buf, size_t size, bool flag, bool flag1)
{
    if (flag && flag1)
    {
        for (size_t i = 0; i < size; i++)
            buf[i] = 1; // OK
    }
}

void TestOr(__out_ecount(flag || flag1 ? size: 256) char *buf, size_t size, bool flag, bool flag1)
{
    if (flag)
    {
        //good
        for (size_t i = 0; i < size; i++)
            buf[i] = 1;
    }

    if (flag1)
    {
        //good
        for (size_t i = 0; i < size; i++)
            buf[i] = 1;
    }

    //bad
    for (size_t i = 0; i < size; i++)
        buf[i] = 1;
}

void CallAnd()
{
    char a[10];
    TestAnd(a, 10, 1, 1);   // OK. Takes size, accesses size.   // OK for PREfix
    TestAnd(a, 10, 1, 0);   // BAD. Takes 256, accesses size.   // OK for PREfix
    TestAnd(a, 10, 0, 0);   // BAD. Takes 256, accesses none.   // OK for PREfix

    char b[256];
    TestAnd(b, 256, 1, 1);  // OK. Takes size, accesses size    // OK for PREfix
    TestAnd(b, 256, 0, 1);  // OK. Takes 256, accesses none.    // OK for PREfix

    // For PREfix
    TestAnd(b, 300, 1, 0);  // BAD. Overflows - Takes 256, accesses 300.    // OK for ESPX per annotations
}

void CallGoodAnd()
{
    char a[10];
    char b[256];

    GoodTestAnd(a, 10, 1, 1);   // OK. Takes size, accesses size.   // OK for PREfix
    GoodTestAnd(b, 10, 1, 0);   // OK. Takes 256, accesses none.    // OK for PREfix
    GoodTestAnd(b, 256, 1, 1);  // OK. Takes size, accesses size    // OK for PREfix
    GoodTestAnd(b, 256, 0, 1);  // OK. Takes 256, accesses none.    // OK for PREfix
}

void CallOr()
{
    char a[10];
    TestOr(a, 10, 1, 1);
    TestOr(a, 10, 1, 0);
    TestOr(a, 10, 0, 1);
    TestOr(a, 10, 0, 0);    // BAD. Takes 256. Accesses size.   // OK for PREfix

    char b[256];
    TestOr(b, 256, 1, 1);
    TestOr(b, 256, 1, 0);
    TestOr(b, 256, 0, 1);
    TestOr(b, 256, 0, 0);

    // For PREfix
    TestOr(b, 300, 0, 0);   // BAD. Takes 256. Accesses size.   // OK for ESPX per annotations
}

// Test for awcount
// awcount(flag, size) - size refers to byte count if flag is true, double-byte count if flag is false.

void Dual(__out_awcount(flag, size) char *buf, size_t size, bool flag)
{
    if (!flag)  // If double-byte count
        size *= 2;
    for (size_t i = 0; i < size; ++i)
        buf[i] = 0;
}

void CallDual()
{
    char a[10];
    Dual(a, 10, true);
    Dual(a, 10, false); // BAD. Causes Dual to overrun a.

    unsigned short b[10];
    Dual((char *)b, 10, false);
    Dual((char *)b, 10, true);  // BAD - passing 20-byte buffer, telling it is 10-byte. ESPX says OK. OK for PREfix.
    Dual((char *)b, 20, true);  // OK
 }

//Test for awcount that depends on a bit flag
#define UNICODE_FLAG   0x80
void DualBit(__out_awcount(!(flags & UNICODE_FLAG), size) char *buf, size_t size, unsigned int flags)
{
    if (flags & UNICODE_FLAG)   // If size is double-byte count
        size *= 2;
    for (size_t i = 0; i < size; ++i)
        buf[i] = 0;
}

void BadDualBit(__out_awcount(!(flags & UNICODE_FLAG), size) char *buf, size_t size, unsigned int flags)
{
    if (!(flags & UNICODE_FLAG))    // If size is byte count
        size *= 2;
    for (size_t i = 0; i < size; ++i)
        buf[i] = 0;     // BAD. May overflow. PREfix needs a caller to decide.
}

void CallDualBit()
{
    char a[10];
    DualBit(a, 10, 0);
    DualBit(a, 10, UNICODE_FLAG);   // BAD. Causes DualBit to overflow a.

    unsigned short b[10];
    DualBit((char *)b, 10, UNICODE_FLAG);
    DualBit((char *)b, 10, 0);      // BAD. Passsing 20-byte buffer, saying it is 10-byte. ESPX says OK. OK for PREfix. 
    DualBit((char *)b, 20, 0);      // OK
}

void CallBadDualBit()
{
    char a[10];
    BadDualBit(a, 10, 0);  // BAD. Causes BadDualBit to overflow. OK for ESPX per annotations.
}

typedef __nullterminated char *LPSTR;
typedef unsigned short WCHAR;
typedef __nullterminated WCHAR * LPWSTR;
typedef __success(return >= 0) long HRESULT;

#define HELPTEXTW      0x81
#define HELPTEXTA      0x01

void LoadStringW(__out_ecount(size) LPWSTR buf, size_t size)
{
    if (buf != nullptr)
    {
        int i;
        for (i = 0; i < size - 1; ++i)
            buf[i] = L'a' + i % 26;

        buf[i] = L'\0';
    }
}

HRESULT DualBitGood(__out_awcount(!(flags & UNICODE_FLAG), size) LPSTR buf, size_t size, unsigned int flags)
{
    if (flags == HELPTEXTW) // !(flags & UNICODE_FLAG) == false -> size is double-byte count.
    {
        LoadStringW((LPWSTR)buf, size); return 0;   // OK.
    }
    return -1;
}

HRESULT DualSwitch(__out_awcount(!(flags & UNICODE_FLAG), size) LPSTR buf, size_t size, unsigned int flags)
{
    switch (flags)
    {
        case HELPTEXTW:     // !(flags & UNICODE_FLAG) == false -> size is double-byte count.
            LoadStringW((LPWSTR)buf, size); return 0;   // OK.
    }
    return -1;
}

HRESULT DualFlag(__out_awcount(!flag, size) LPSTR buf, size_t size, unsigned int flag)
{
    size_t i;
    if (flag)
    {
        // size is double-byte count
        LPWSTR wbuf = (LPWSTR)buf;
        for (i = 0; i < size - 1; ++i)
            wbuf[i] = L'a' + i % 26;
        wbuf[i] = L'\0';
    }
    else
    {
        // size is single-byte count
        for (i = 0; i < size - 1; ++i)
            buf[i] = 'a' + i % 26;
        buf[i] = '\0';
    }
    return 0;
}

HRESULT DualBitCallingDualFlag(__out_awcount(!(flags & UNICODE_FLAG), size) LPSTR buf, size_t size, unsigned int flags)
{
    switch (flags)
    {
        case HELPTEXTW:
        case HELPTEXTA:
            return DualFlag(buf, size, flags == HELPTEXTW); return 0;
    }
    return -1;
}

void cleveraw(__out_ecount((!!bUnicode + 1) * cchBuffer) char *p, unsigned short bUnicode, unsigned int cchBuffer)
{
    // ecount((!!bUnicode + 1) * cchBuffer) is equivalent to awcount(!bUnicode, cchBuffer) 
    DualFlag(p, cchBuffer, bUnicode);
}

void callcleveraw()
{
    char a[10];
    cleveraw(a, 0, 10); // OK
    char b[10];
    cleveraw(b, 1, 10); // BAD. Overflows b.
}

void foo(__out_ecount(n + ((flags & 0x1) != 0)) int *p, int n, unsigned int flags)
{
    int i;
    for (i = 0; i < n; ++i)
        p[i] = i + 1;

    if ((flags & 0x1) != 0)
        p[i] = i + 1;
}

void callfoo()
{
    int a[10];
    foo(a, 10, 0x0); // OK
    int b[10];
    foo(a, 10, 0x1); // BAD. Overflows a. We are saying a has 10 + 1 elements.
}

void main()
{
    char c200[200];
    char c356[356];
    char c512[512];
    char c556[556];
    qcolon(c200, 100, 1, 1);    // OK. Taking size + size elements, and accesses size + size elements.
    qcolon(c556, 300, 1, 0);    // BAD. Taking size + 256 elements, but accesses size + size elements. [PFXFN] Maybe due to loop count calculated from parameter value.
    qcolon(c356, 100, 0, 1);    // BAD. Taking 256 + size elements, but accesses 256 + 256 elements.
    qcolon(c512, 300, 0, 0);    // OK. Taking 256 + 256 elements, and accesses 256 + 256 elements.

    qcolon_good(c200, 100, 1, 1);   // OK
    qcolon_good(c556, 300, 1, 0);   // OK
    qcolon_good(c356, 100, 0, 1);   // OK
    qcolon_good(c512, 300, 0, 0);   // OK

    char strA[100];
    WCHAR strW[100];
    DualBitCallingDualFlag(strA, 100, HELPTEXTA);   // OK
    DualBitCallingDualFlag((LPSTR)strW, 100, HELPTEXTW);   // OK
}