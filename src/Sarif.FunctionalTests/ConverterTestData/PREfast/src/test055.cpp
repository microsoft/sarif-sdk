#include "specstrings.h"

//+=, -= bug
void f(int j)
{
    int a[10];
    int i = 10;
    i -= j/2;
    a[i] = 1;   // BAD. Possible over/underflow.

    int b[10];
    i = 5;
    i += i;
    b[i] = 10;  // BAD. Overflow.
}

// Bug: FIXED: Not tracking switch statements.
void foo(int b)
{
    enum AllocType {
        A1,
        A2
    } a;

    int *p = 0;
    if (b)
    {
        a = A1;
        p = new int[10];
    }
    else
    {
        a = A2;
        p = new int[20];
    }

    if (p == nullptr)
        return;

    switch (a)
    {
        case A1:
            p[9] = 1;   // OK
            break;
        case A2:
            p[20] = 1;  // BAD. Overflow.
            break;
    }

    delete[] p;
}

// Variable length struct
struct S {
    int a;
    int b;
    int elements[];
};

S s = {1, 2, {3, 4, 5}};

void bar()
{
    s.elements[1] = 1;
    s.elements[4] = 1;
}

// Bug: FIXED: not picking up built-in model for alloca
extern "C" void *          __cdecl _alloca(size_t);
#define alloca(n)   _alloca(n)
void testAlloca()
{
        char *p = (char *)_alloca(100);
        p[100] = 1;
        char *q = (char *)alloca(100);
        q[100] = 1;
}

// Bug: FIXED: not recognizing range for char values.

unsigned char g[10];

void testChar(__in_range(0, 9) int i)
{
    char a[255];
    a[g[i]] = 1;    // BAD. This can overflow of g[i] == 255 [PFXFN] PREfix does not report overflow.
}

// Bug: FIXED: __out_ecount_part_opt annotation should not have post-conditions assumed when buffer is NULL.
#define SUCCESS 0
__success(return == SUCCESS)
int RegGetValue(const char* key, __out_ecount_part_opt(*size, *size) char *buf, size_t *size)
{
    if (buf == nullptr)
    {
        *size = 10;
        return 0;
    }

    for (size_t i = 0; i < *size; ++i)
        buf[i] = 1;

    return 0;
}

void TestNull(const char *key)
{
    size_t size = 0;
    if (RegGetValue(key, 0, &size) == SUCCESS)
    {
        if (size > 0)
        {
            char *buf = new char[size];
            if (buf != nullptr)
            {
                buf[size] = 0;  // BAD. Overflow.
                delete[] buf;
            }
        }
    }
}

// Should report bug in dynamic initializer
const int squares[] = {0, 1, 4, 9, 16};
int s5 = squares[5];    // BAD. [PFXFN] PREfix does not report this. Compare this with line 139.

void main()
{
    f(0);   // BAD. Should cause overflow. [PFXBUG] PREfix cannot handle i = i +/- j, etc.
    f(22);  // BAD. Should cause underflow. [PFXBUG] PREfix cannot handle i = i +/- j, etc.

    foo(0);
    foo(1);

    g[9] = 255;
    testChar(9);    // BAD. Should overflow g. [PFXFN] PREfix does not report this...

    const int ints[] = {0, 1, 2, 3, 4};
    int val = ints[5];    // BAD.
}