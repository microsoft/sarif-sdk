#include "specstrings.h"
#include "undefsal.h"

void f(__in_ecount(*n) int *buf, __deref_out_range(pre(*n), pre(*n) + 1) int *n)
{
    (*n) += 2;  // BAD. Violates out range. OK for PREfix.
}

void g()
{
    int a[3];
    int n = 0;
    f(a, &n);   // n could become 1 per annotation from ESPX's view. n became 2 for PREfix.
    f(a, &n);   // n could become 2 per annotation. n became 4 for PREfix.
    a[n] = 0;
    f(a, &n);   // n could become 3 per annotation. n became 6 for PREfix.
    a[n] = 1;
}

__success(return == 0)
int
DoSomethingHelper(
    __out_ecount(*lpBufferSize) char* lpBuffer,
    __inout __deref_out_range( ==, pre(*lpBufferSize)) size_t *lpBufferSize
    )
{
    if (lpBuffer == nullptr || lpBufferSize == nullptr)
        return -2;

    for (size_t i = 0; i < *lpBufferSize; ++i)
        lpBuffer[i] = 1;

    int rc;
    static int magic;
    switch (magic % 3)
    {
    case 1:
        *lpBufferSize  += 1;    // Try to cause overflow
        rc = 1;
        break;
    case 2:
        *lpBufferSize  = -1;    // Try to cause underflow
        rc = -1;
        break;
    default:
        rc = 0;
        break;
    }

    return rc;
}

__success(return == 0)
int
DoSomethingHelperPart2(
    __out_ecount(*lpBufferSize) char* lpBuffer,
    __inout size_t *lpBufferSize
    )
{
    if (lpBuffer == nullptr || lpBufferSize == nullptr)
        return -2;

    for (size_t i = 0; i < *lpBufferSize; ++i)
        lpBuffer[i] = 1;

    int rc;
    static int magic;
    switch (magic % 3)
    {
    case 1:
        *lpBufferSize  += 1;    // Try to cause overflow
        rc = 1;
        break;
    case 2:
        *lpBufferSize  = -1;    // Try to cause underflow
        rc = -1;
        break;
    default:
        switch ((magic * 7) % 3)
        {
        case 1:
            *lpBufferSize  += 1;    // Try to cause overflow
            rc = 1;
            break;
        case 2:
            *lpBufferSize  = -1;    // Try to cause underflow
            rc = -1;
            break;
        default:
            rc = 0;
            break;
        }
        break;
    }

    return rc;
}

__success(return == 0)
int
DoSomethingGood(
    __out_ecount(*lpcchBuffer) char* lpBuffer,
    __inout size_t *lpcchBuffer
    )
{
    int status;
    status = DoSomethingHelper(lpBuffer, lpcchBuffer);

    if (status == 0)
    {
        status = DoSomethingHelperPart2(lpBuffer, lpcchBuffer);
    }

    return status;
}

__success(return == 0)
int
DoSomethingBad(
    __out_ecount(*lpcchBuffer) char* lpBuffer,
    __inout size_t *lpcchBuffer
    )
{
    DoSomethingHelper(lpBuffer, lpcchBuffer);
    // No check for success
    return DoSomethingHelperPart2(lpBuffer, lpcchBuffer);
}

_Ret_bytecap_(pre(*n)) void *foo(__inout int *n)
{
    (*n)--;
    char* buf = new char[*n]; // Bad. Violates post-condition. PREfix does not consider this as a bug (see below).

    static int magic;
    switch (magic % 3)
    {
    case 0:
        *n = -1;         // Cause caller to underflow if used as index
        break;
    case 1:
        *n += 2;         // Cause caller to overflow if used as index
        break;
    }

    return buf;
}

void bar()
{
    int n = 10;
    char *p = (char *)foo(&n);
    if (p == nullptr)
        return;
    p[9] = 1;   // OK for ESPX per annotation for foo. BAD for PREfix - overflows p (see above).
    p[n-1] = 0; // BAD. Post *n is not bound. Potential over / under flow. Underflow for PREfix.
    delete[] p;
}

void Fill(__out_ecount_part(*n, pre(*n)) int *buf, __inout int *n)
{
    static int magic;
    switch (magic % 3)
    {
    case 0:
        for (int i = 0; i < *n; ++i)
            buf[i] = i + 1;
        break;
    case 1:
        *n = 0;
        break;
    case 2:
        *n += 2;
        break;
    }
}

void baz(__out_ecount_full(size) int *buf, int size)
{
    Fill(buf, &size);
    buf[size-1] = 0;    // BAD. Depending on how Fill chages size, this can underflow / overflow.
}

void main()
{
    int iSize = 9;
    int iBuf[10];
    f(iBuf, &iSize);    // f says iSize can grow by 1.
    iBuf[iSize];    // BAD. Should be OK, but actually overflows because f is buggy.

    size_t cSize;
    char cBuf[10];

    cSize = sizeof(cBuf) / sizeof(cBuf[0]);
    DoSomethingGood(cBuf, &cSize);  // OK

    cSize = sizeof(cBuf) / sizeof(cBuf[0]);
    DoSomethingBad(cBuf, &cSize);   // BAD. It can over / underflow cBuf

    baz(iBuf, 10);       // Cause baz to be simulated, generating warnings. 
}
