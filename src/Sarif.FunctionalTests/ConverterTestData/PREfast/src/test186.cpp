#include <specstrings.h>

bool bar();

void obvious()
{
    size_t count = 0;
    char a[100];
    while (true)
    {
        a[count++] = 0; // shd warn
    }
}

void with_condition()
{
    size_t count = 0;
    char a[100];
    while (true)
    {
        if (bar())
        {
            a[count++] = 0; // sd warn
        }
    }
}

void with_related_increment_condition()
{
    const size_t max = 100;
    size_t count = 0;
    char a[max];
    while (true)
    {
        if (count < max)
        {
            a[count++] = 0; // shd not warn
        }
    }
}

void with_related_decrement_condition()
{
    const size_t max = 100;
    size_t count = max-1;
    char a[max];
    while (true)
    {
        if (count > 0)
        {
            a[count--] = 0; // shd not warn
        }
    }
}

void with_direct_safety_check_decr()
{
    const size_t max = 100;
    size_t count = max-1;
    char a[max];
    while (true)
    {
        if (count > 1)
        {
            a[count--] = 0; // shd not warn
        }
    }
}

//
// Simplified bug repro taken from Windows
//
typedef _Null_terminated_ char* PCSTR;
void foo(PCSTR pSource, _In_reads_(destLen) char* pOutput, size_t destLen)
{
    char* pEscaped = pOutput;

    size_t curLen(destLen);

    while (*pSource != 0)
    {
        switch (*pSource)
        {
            case '<':
                if (destLen > 4)
                {
                    *pOutput++ = 'a';   // shd not warn
                    *pOutput++ = 'b';   // shd not warn
                    *pOutput++ = 'c';   // shd not warn
                    *pOutput++ = 'd';   // shd not warn
                    destLen -= 4;
                }
                break;
                
        }
    }
}

