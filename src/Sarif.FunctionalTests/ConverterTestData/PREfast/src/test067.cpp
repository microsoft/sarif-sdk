#include "specstrings.h"
#include "mywin.h"

HRESULT StrCchCopy(__out_ecount(size) PSTR dest, const char *src, size_t size)
{
    if (dest == nullptr || src == nullptr)
        return -1;

    for (size_t i = 0; i < size - 1 && *src; ++i)
        *dest++ = *src++;

    *dest = '\0';

    return 0;
}

void foo()
{
    const char* src = "Test";
    char dst[10];
    StrCchCopy(dst, src, 10);

    char ch = dst[9];   // BAD. Accessing uninitialized portion of dst.  [ESPXFN] [PFXFN] Neither of the tools reports this...
}

void main() { /* dummy */ }