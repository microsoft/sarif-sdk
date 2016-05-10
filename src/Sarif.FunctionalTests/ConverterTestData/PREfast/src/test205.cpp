#include "specstrings.h"
#include <stdarg.h>
#include "mywin.h"

void foo(int x, int y, ...)
{
    va_list Arglist;
    va_start(Arglist, y);

    int sum;
    const int size = 10;
    int a[size] = { 1,2,3,4,5,6,7,8,9,10 };

    // 1. Use var arg as iteration variable
    int idx = va_arg(Arglist, int);
    if (idx < 0)
        idx = 0;

    sum = 0;
    for (; idx <= size; ++idx)
    {
        sum += a[idx];  // BAD. Overflows when idx == size
    }

    // 2. Use var arg as iteration bound
    int max = va_arg(Arglist, int);
    if (max < 0)
        max = 0;
    else if (max > 10)
        max = 10;

    for (idx = 0; idx <= max; ++idx)
    {
        sum += a[idx];  // BAD. Overflows when idx == max
    }

    va_end(Arglist);
}

void TestLog_Warn(
    __in LPCWSTR pszFunction,
    __in DWORD dwLine,
    __in LPCWSTR pszFormat,
    ...
    )
{
    va_list args;
    va_start(args, pszFormat);      // OK. We used to get 26018 (Esp Bug 1192)

    // ...

    va_end(args);
}

