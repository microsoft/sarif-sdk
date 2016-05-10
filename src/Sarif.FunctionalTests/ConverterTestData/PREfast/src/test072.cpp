#include "specstrings.h"
#include "specstrings_new.h"
// #include "undefsal.h"

typedef unsigned short WCHAR;
typedef __nullterminated WCHAR *PWSTR;
typedef __nullterminated WCHAR const *PCWSTR;

extern "C" __out_range(_String_length_(s), _String_length_(s)) size_t wcslen(/*__in PWSTR */ PCWSTR s);
extern "C" PWSTR wcschr(_In_z_ const PWSTR _Str, WCHAR _Ch);

__out_range(==, _String_length_(s)) size_t mywcslen(/* __in PWSTR */ PCWSTR s)
{
    return wcslen(s);
}

void StrCopy(__out_ecount(count) PWSTR dest, /* __in */ PWSTR src, size_t count)
{
    while (--count > 0 && *src)
    {
        *dest++ = *src++;
        --count;
    }
    
    *dest = '\0';
}

__out_range(0, _String_length_(s)-1) unsigned find(__in PWSTR s, WCHAR c)
{
    WCHAR* p = wcschr(s, c);
    if (p == nullptr)
        return 0;   // Cannot return -1, etc., due to the annotation
    return (unsigned)(p - s);
}

void f(__out_ecount(20) PWSTR buf, __in PWSTR s)
{
    StrCopy(buf, s, 20);
    size_t index = find(buf, L'a');
    buf[index+1]++;
    buf[index+2]++; // BAD. Potential overflow here, past the null-terminator.
}

int foo(/* __in PWSTR */ PCWSTR s)
{
    return s[wcslen(s) + 1];  // BAD. Warning 26016 here
}

int bar(/* __in PWSTR */ PCWSTR s)
{
    return s[mywcslen(s) + 1];  // BAD. Warning 26016 here
}

void main()
{
    WCHAR istr[20];
    for (int i = 0; i < 18; ++i)
    {
        istr[i] = (WCHAR)(L'z' - i);
    }
    istr[18] = L'a';
    istr[19] = L'\0';
    WCHAR ostr[20];
    f(ostr, istr);  // BAD. ostr can overflow due to bug in f.
                    // [PFXFP] PREfix warns use of uninitialized memory ostr[1] and ostr[2] thinking istr[0] or istr[1] can be '\0'.
                    // [PFXFN] PREfix does not detect possible overflow in f.

    PWSTR istr2 = L"My test string # 2.";
    foo(istr2); // BAD. foo will overrun istr2
    bar(istr2); // BAD. bar will overrun istr2
                // NOTE. [PFXFN] If istr is passed in instead, PREfix won't be able to detect the overflow.
}

