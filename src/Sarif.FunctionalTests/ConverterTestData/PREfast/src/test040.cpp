#include "specstrings.h"
#include "undefsal.h"

typedef unsigned short WCHAR;
typedef __nullterminated char *PSTR;

void Fill(__out_ecount_part(*size, *size+1) WCHAR *buf, __inout size_t *size)
{
    if (buf == nullptr || size == nullptr)
        return;

    size_t in_size = *size;
    *size -= *size % (size_t)size;
    if (*size + 1 > in_size)
        *size = in_size - 1;

    for (size_t i = 0; i < *size + 1; ++i)
    {
        buf[i] = L'a' + i % 26;
    }
}

void Assume(__out_ecount_full(length) int *p, size_t length)
{
    if (p == nullptr)
        return;

    for (size_t i = 0; i < length; ++i)
    {
        p[i] = 1;
    }
}

void StrFill(__out_ecount(size) PSTR s, size_t size)
{
    size_t max = (size_t)s % size;
    if (max > size - 2)
        max = size - 2;
    for (size_t i = 0; i < max; ++i)
    {
        s[i] = 'a' + i % 26;
    }

    s[max] = '\0';
}

void f()
{
	WCHAR buf[10];
	size_t size = 10;
	Fill(buf, &size);
	buf[size] = 0;
}

void g(__out_ecount_full(10) int *p, size_t length)
{
	Assume(p, length);
	p[10] = 1;	
}

void h(__out_ecount(size) PSTR s, size_t size)
{
    if (s == nullptr)
        return;

    StrFill(s, size);
    s[size] = '\0'; // This is plain wrong. ESPX gives a bit confusing warning details.
}

void main()
{
    int a[10];
    g(a, 10);

    int b[11];
    g(b, 11);
    int ans = b[10];

    char c[10];
    h(c, 10);
}