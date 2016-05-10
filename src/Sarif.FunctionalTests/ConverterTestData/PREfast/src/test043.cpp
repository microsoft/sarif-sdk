#include "specstrings.h"
#include "mymemory.h"
#include "mywinerror.h"
#include "undefsal.h"

int *glob;
__success(return > 10) int f(__post __deref __writableTo(elementCount(10)) int **p) {
    if (glob)
    {
        *p = glob;
        return 11;
    }
    return 0;   // __success does not tell what happens on failure.
}

void g()
{
    int *a;
    int succ = f(&a);
    a[succ] = 1;    // BAD. f returns 11 on success, greater than elem count of a.
                    // NOTE: PREfix also warns for unitialized memory access when glob is null.
}

__success(return == 0) int f1(__post __deref __writableTo(elementCount(10)) int **p) {
    if (glob)
    {
        *p = new int[9];    // BAD
        return 0;
    }
    *p = new int[10];
    return 1;
}

bool pred(int i)
{
    return ((char)i != '|'); 
}

__success(return == 1)
bool foo(__out_ecount_part(*size, *size) int * buf, int *size, __in __nullterminated char *s)
{
    int reqd = 0;
    int i = 0;
    while (pred(s[i]))      // BAD: Can overrun s depending on what pred says. [PFXFN] PREfix seems unable to detect this.
    {
        if (pred(s[i]))
            reqd++;
        i++;
    }
    if (reqd > *size)
    {
        *size = reqd;
        return 1;  // BAD: Cannot return success code. *size grew.
    }
    i = 0;
    int j = 0;
    while (pred(s[i]) && i < reqd)
    {
        if (pred(s[i]))
            buf[j++] = s[i];
        i++;
    }
    *size = reqd;   // NOTE: reqd could have been overflown in line 46 and became negative
    return 1;
}

char bar(char *s)
{
    char ch = '\0';

    int a[10];
    int n = 10;
    
    if (foo(a, &n, s))
    {
        char next = a[n];   // It can over or underflow the buffer. BAD. [PFXFN] PREfix does not detect underflow when n is set to negative. See line 62.
        ch = a[n-1];        // This can also over or underflow the buffer. BAD.
        if (next > ch)
            ch = next;
    }

    return ch;
}


// Test post condition error messages to see that they correctly mention that a __success annotation is present and satisified

HRESULT SuccessSatisfied(__deref_out_ecount(n) char **p, size_t n)
{
    *p = new char[10];
    return 0;   // BAD. p may not have n elements.
}

int NoSuccess(__deref_out_ecount(n) char **p, size_t n)
{
    *p = new char[10];  // BAD. 10 is not at all related to "n"
    return -1;          // No way we can say we failed - without annotation!
}

void ErrorCode(int ec, HRESULT* hr)
{
    *hr = __HRESULT_FROM_WIN32(ec);
}

HRESULT SuccessMaybeSatisified(__deref_out_ecount(n) char **p, size_t n)
{
    static int magic;
    *p = new char[10];
    HRESULT ec;
    ErrorCode(magic, &ec);
    return ec;
}

// || operator in success annotation

class Stream
{
    __field_ecount_full(m_size) char *m_data;
    size_t m_size;
public:
    __success(return >= 0 && return != 1)
    HRESULT GetData(__out_ecount_part(*size, *size) char *buf, __inout size_t *size);
};

__success(return >= 0 && return != 1)
HRESULT Stream::GetData(__out_ecount_part(*size, *size) char *buf, __inout size_t *size)
{
    if (m_size > *size)
    {
        *size = m_size;
        return 1;
    }
    *size = m_size;
    memcpy(buf, m_data, m_size);        
    return 0;
}    

HRESULT GetStreamData(Stream *pStream)
{
    char buf[256];
    size_t size = sizeof(buf);
    HRESULT hr = pStream->GetData(buf, &size);
    if (hr < 0)
        return hr;
    return pStream->GetData(buf, &size);
}

HRESULT GetStreamDataGood(Stream *pStream)
{
    char buf[256];
    size_t size = sizeof(buf);
    HRESULT hr = pStream->GetData(buf, &size);
    if (hr < 0 || hr == 1)
        return hr;
    return pStream->GetData(buf, &size);
}


// See if __success and __range on return value co-exist peacefully
#define TRUE 1

__success(TRUE) __range(<, 0) HRESULT Error()
{
    return -1;
}

HRESULT Test1(
    __out_ecount_part(maxCount,*actualCount) char* chars,
    __in unsigned long maxCount,
    __out unsigned long* actualCount)
{
    HRESULT hr;

    hr = Error();
    if (hr < 0)
    {
        return hr;
    }

    *actualCount = 0;

    return 0;
}

__success(TRUE) __range(hr, hr) HRESULT PassError(
    HRESULT hr
    )
{
    return hr;
}

extern "C" 
HRESULT DoStuff()
{
    static int rand;

    if ((int)&rand % 2 == 0)
    {
        return 0;
    }
    else
    {
        return -1;
    }
}

HRESULT Test2(
    __out_ecount_part(maxCount,*actualCount) char* chars,
    __in unsigned long maxCount,
    __out unsigned long* actualCount)
{
    HRESULT hr;

    hr = DoStuff();
    if (hr < 0)
    {
        return PassError(hr);
    }

    *actualCount = 0;

    return 0;
}

void main()
{
    int a[10];
    glob = a;
    g();                // BAD: g is buggy.

    int* p;
    if (f1(&p) == 0)    // i.e., if f1 succeeded
    {
        if (p != nullptr)
            p[9] = 1;       // BAD: f1 is supposed to return a 10-elem buffer, but returned a 9-elem buffer.
    }

    char* s = "This is longer than 10 but shorter than 50 chars.";
    bar(s);             // PREfix doesn't tell anything. Need to look deeper.

    char* ss;
    if (SuccessSatisfied(&ss, 20) == 0)
    {
        if (ss != nullptr)
            ss[19] = 'a';   // BAD: SuccessSatisfied is supposed to return a 20-elem buffer on success, but returned a 10-elem buffer.
    }

    NoSuccess(&ss, 20); // It's return value of -1 says nothing about success or failure. Let me ignore.
    if (ss != nullptr)
        ss[19] = 'b';   // BAD: NoSuccess is supposed to return a 20-elem buffer. It returned a 10-elem buffer.

    if (SuccessMaybeSatisified(&ss, 20) == 0)
    {
        if (ss != nullptr)
            ss[19] = 'b';   // BAD: SuccessMaybeSatisified is supposed to return a 20-elem buffer on success, but it returned a 10-elem buffer.
    }
}