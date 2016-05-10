#include "specstrings.h"
#include "undefsal.h"

int a[2000];

#define ASSERT(e) {int __assertBuffer[10]; if (!(e)) { __assertBuffer[10] = 0;} } //genereate a warning 2000 if condition fails

void f()
{
    for (int i = 0; i < 10; i++)
    {
        for (int j = 0; j < 20; j +=2)
        {
            ASSERT(j < 19);
            a[i + j]++;
        }
    }
}

void g(__in_ecount(n) int *b, int n)
{
    int *p = b;
    int j = 1;
    int k = 10;
    for (int i = 0; i < n; i++)
    {
        ASSERT(p - b == i);
        ASSERT(j <= i + 1);
        ASSERT(10 - k <= i);

        p++;


        if (b[i])
            j = j + 1;
        else
            k--;
    }
}

void h()
{
    int *p = a;
    int k = 0;
    int r = 100;
    int iter = 0;
    for (int i = 0; i < 10; i++)
    {
        ASSERT(k <= iter);
        ASSERT(k >= - 2 * iter);
        ASSERT(p <= a + 2 * iter);
        if (i > 5)
        {
            int j = 0;
            for (; j < 20; ++j)
            {
                if (a[i+j] == 0)
                    break;
                else if (a[i + j] == 1)
                    goto end;

                int r0 = r;
                int i0 = i;
                while (a[i+j] < 5)
                {
                    ASSERT(-(r - r0) == 3 * (i - i0));
                    i++;        // This can grow out of bound. [PFXFN] PREfix abandons loops with nested loop.
                    r -= 3;
                    if (a[i+j] == 3)
                        goto end;
                }
            }
            k -= 2;
            j++;
        }
        else
        {
            p = p + 2;
            k++;
        }
        iter++;
    }

end:
    return;
}

// From noise-todo\synchr-index-vars.cpp
typedef __nullterminated const char *PCSTR;

void synchronized(__out_ecount(size) char *buf, size_t size, __in PCSTR src)
{
    while (size > 0 && *src)
    {
        if (*src == ' ')
        {
            if (size > 2)
            {
                *buf++ = ' ';
                *buf++ = ' ';
            }
            else
                break;
            size -= 2;
        }
        else
        {
            *buf++ = *src;
            size--;
        }
        src++;
    }
}

void main()
{
    // NOTE: PREfix will assume the assertions can fail and generates warnings for each of them.
}