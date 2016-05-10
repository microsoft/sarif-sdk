#include "specstrings.h"
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

typedef unsigned short wchar_t;
void f(__out_ecount(n) char *p, size_t n, const char *q)
{
    while (n > 0 && *q)     // OK
    {
        if (*q != ' ')
        {
            *p++ = *q;
            n--;
        }
        q++;
    }
}

typedef unsigned short wchar_t;
void f1(__out_ecount(n) char *p, size_t n, const char *q)
{
    while (n >= 0 && *q)
    {
        if (*q != ' ')
        {
            *p++ = *q;  // BAD. Overflows p. [PFXFN] PREfix does not report this. 
            n--;
        }
        q++;
    }
}

void f2(__out_ecount(n) char *p, size_t n, const char *q)
{
    if (n < 10)
        return;
    while (*q)
    {
        if (*q != ' ')
        {
            *p++ = *q;  // BAD. p can overflow. [PFXFN] PREfix does not report this.
        }
        q++;
    }
}

void foo(__out_bcount(size) wchar_t *buf, size_t size)
{
    int sizeInChars = size/2;
    buf[sizeInChars-1] = 0;     // It can underflow, e.g., if size = 1.
}

void bar(__out_bcount(size) wchar_t *buf, size_t size)
{
    size_t sizeInChars = size/2;
    while (sizeInChars --)
        *buf++ = 0;
}

void ZeroMem(__out_ecount_full(cnt) char *dst, int cnt)
{
    while (cnt) {
        *dst = 0;
        dst++;
        cnt--;
    }
}

void GetComputerName(__out_ecount_part(*size, *size + 1) char *buf, size_t *size)
{
    // NOTE: This will add ESPX warning for the annotation.
    // No test case for PREfix for annotation error.
}

void Fill(__out_ecount(*size) char *buf, size_t *size)
{
    size_t original = *size - 1;
    GetComputerName(buf, &original);
    buf[original+1] = 0;  // No overflow per annotation for GetComputerName
}

struct RtlString {
    int maxsize;
    int len;
    char *buf;
};

void GetString(__inout RtlString *s)
{
    if (s == nullptr)
        return;

    int magic = (int)s % 3;
    if (magic == 0)
        s->len = 0;
    else if (magic == 1)
        s->len = s->maxsize + 1;
    else
        s->len = s->maxsize;
}

void TestFields(__out_ecount(size) char *buf, int size, int i)
{
    RtlString rtlStr;
    rtlStr.buf = buf;
    rtlStr.maxsize = size;
    GetString(&rtlStr);
    
    buf[rtlStr.len-1] = 0; // BAD. Over/underrun - either can happen. RtlString.len has no relationship with buf and maxsize.

    if (i < rtlStr.len)
        buf[i] = 0;
}

void f(__out_ecount(*pch) char *buf, size_t *pch)
{
    static char basic[10] = "abc";
    memcpy(buf, basic, sizeof(basic));
}

void FillBuff(short *buf, size_t *cb)
{
    // Let's cause Division to overflow buf
    *cb <<= 1;
}

void Division()
{
    short buf[100];

    size_t cb = 100;

    FillBuff(buf, &cb);

    buf[cb/2] = 1;  // BAD. It can overflow.
}

void main()
{
    // void f(__out_ecount(n) char *p, size_t n, const char *q)
    char* istr = "This is my test string...";
    size_t len = strlen(istr) - 1;  // Remove the last two '.'
    char *ostr = (char*)malloc(len * sizeof(char));
    if (ostr != nullptr)
    {
        f(ostr, len, istr);
        f1(ostr, len, istr);    // BAD. ostr can overflow. [PFXFN] PREfix does not report this.
        f2(ostr, len, istr);    // BAD. ostr can overflow. [PFXFN] PREfix does not report this.
        TestFields(ostr, len, len / 2); // BAD. Under/overflows ostr.
    }

    char str1[1];
    foo((wchar_t*)str1, 1);     // BAD. str will underflow.

    char str5[5];
    bar((wchar_t*)str5, 5);     // OK

    len = 5;
    ostr = (char*)malloc(len * sizeof(char));
    if (ostr != nullptr)
        f(ostr, &len);  // BAD. ostr will overflow.
}
