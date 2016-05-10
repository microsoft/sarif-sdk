#include "specstrings.h"

void f(__out_ecount(n) int *a, __in_ecount(m) int *b, int m, int n)
{
    for (int i = 0; i < m; i++)
    {
        a[i] = b[i];
    }
}

int a[10];
int g(int i)
{
    if (i > 10)
        return -1;
    return a[i];    // BAD. Can over/underflow.
}

void Fill(__out_ecount_part(size, *length) char *buf, int size, int *length)
{
    int filled = size - (int)buf % size;
    if (filled > size)
        filled = size;
    
    char ch = 0;
    for (int i = 0; i < filled; ++i)
        buf[i] = ++ch;
    
    if (length != nullptr)
        *length = filled;
}

void foo()
{
    char a[100];
    int length = 0;
    Fill(a, 100, &length);
    a[length] = '\0';   // BAD. Can overflow - length can still be equal to the size (100 in this example).
}

int bar()
{
    int j = 0;
    static int a[10];
    for (int i = 0; i < 10; i++)
    {
        if (j > i)
            a[10] = 1;  // BAD. Overflows. [ESPXFN] Maybe due to j changing inside the loop.
        if (a[i] == 1) {
            j++;
        }
    }

    return a[j-1];  // BAD. Underflow - j can be zero, causing underflow. [PFXFP] [PFXFN] PREfix reports this as overflow, which is wrong.
}

void baz()
{
    int a[10];
    for (int i = 0; i < 10; i+= 2)
        a[i] = a[i+1] = i;      // OK. i == 0, 2, 4, and 8
}


int elem2[] = { 10, 0};
void elem2noise()
{
    int i = 0;
    while (elem2[i] != 0)
    {
        elem2[i];
    }
}

int b[10];
int ineqnoise(int i)
{
    if (i == 10)
        ;
    else if (i > 10)
        ;
    else
        return b[i]; // BAD. Potential underflow. [PFXFN] PREfix misses this.
    return 0;
}

int Fill(__out_ecount_part(size, return) char *buf, int size)
{
    int filled = size - (unsigned int)buf % size;
    if (filled > size)
        filled = size;
    char ch = 0;
    for (int i = 0; i < filled; ++i)
        buf[i] = ++ch;
    
    return filled;
}

void Access(__in_ecount(size) char *buf, int size)
{
    char ch = '\0';
    for (int i = 0; i < size; ++i)
    {
        ch |= buf[i];
    }
}

void AccessAndFill(__inout_ecount(size) char *buf, int size)
{
    char ch = '\0';
    for (int i = 0; i < size; ++i)
    {
        ch |= buf[i];
        buf[i] = ch;
    }
}

void elem1noise()
{
    static char a[10];
    int filled = Fill(a, 10);
    Access(a + filled, 10 - filled);
    AccessAndFill(a + filled, 10 - filled);
}

void main()
{
    int a[3];
    int b[5] = {1,2,3,4,5};
    f(a, b, 5, 3);      // BAD. Will overflow a.

    int val = g(10);    // BAD. Will overflow global a.
    val = g(-1);        // BAD. Will underflow global a.

    ineqnoise(-1);      // BAD. Underflows.
}
