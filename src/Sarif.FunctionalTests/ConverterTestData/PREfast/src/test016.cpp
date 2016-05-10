#include "specstrings.h"
// The loop in this test mimick the control flow of some code in 
// dcomss.  Avoiding false alarm here requires widening with the condition
// gathered after one pass over the loop.

void useBuffer(__readableTo(byteCount(count)) 
                      const int *buf,
                      size_t count)
{
    if (buf == nullptr)
        return;

    char val = 0;

    for (size_t i = 0; i < count; ++i)
    {
        val |= ((char*)buf)[i];
    }
}

struct A {};
struct C : virtual A
{
    int foo(int, int);
    int Good();
    int Bad1();
    int Bad2();
    int Bad3();
};

int C::foo(int, int)
{
    return 0;
}

int C::Good()
{
    const struct
    {
        int (C::*pfn)(int, int);
    }
    c_rgPropSources[] =
    {
        { &C::foo },
        { &C::foo },
        { &C::foo },
    };

    for (int i = 0;  i < 3;  ++i)
    {
        if (c_rgPropSources[i].pfn) // In range
        {
            return (this->*(c_rgPropSources[i].pfn))(1, 2);
        }
    }

    return 2;
}

int C::Bad1()
{
    const struct
    {
        int (C::*pfn)(int, int);
    }
    c_rgPropSources[] =
    {
        { &C::foo },
        { &C::foo },
        { &C::foo },
    };
    int j = 0;
    for (int i = 0; i <= 3; ++i) 
    {
        if (c_rgPropSources[i].pfn)  // BAD. Overflow.
        {
            j += (this->*(c_rgPropSources[i].pfn))(1, 2);   // Same as above
        }
    }

    return j;
}

int C::Bad2()
{
    const struct
    {
        int (C::*pfn)(int, int);
    }
    c_rgPropSources[] =
    {
        { &C::foo },
        { &C::foo },
        { &C::foo },
    };
    int j = 0;
    for (int i = 0;  i < 10;  ++i)
    {
        if (c_rgPropSources[i].pfn) // Out of range
        {
            j += (this->*(c_rgPropSources[i].pfn))(1, 2);
        }
    }

    return j;
}

int Bad3(C* p)
{
    int (C::*const c_rgPropSources[])(int,int) =
    {
        &C::foo,
        &C::foo,
        &C::foo,
    };
    int j = 0;
    for (int i = 0;  i < 10;  ++i)
    {
        if (c_rgPropSources[i]) // Out of range
        {
            j += (p->*(c_rgPropSources[i]))(1, 2);
        }
    }

    return j;
}

void main() { /* dummy */ }