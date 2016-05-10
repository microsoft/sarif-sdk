#include "specstrings.h"

// Test case for bug 802
typedef unsigned short wchar_t;
typedef wchar_t *PWSTR;

class TestClass
{
public:
    void Func1() { }
    bool Func2() { return false; }
    void Func3();

    TestClass(): buffer(0), size(0) { }
    ~TestClass() { delete buffer; }

    __field_bcount_opt(size) PWSTR buffer;
    size_t size;
};

void TestClass::Func3()
{
    bool ret = true;

    while (ret)     // In dev12, we used to get warning 26045. Addition of CFG_ENDSCOPE removes it. However, it is dev12 issue, not a regression. See ESP PS bug 1511
    {
        ret = Func2();

        if (ret)
        {
            Func1();
        }
    }
}


// Test case without field annotations, with a real buffer overrun
// seeded to ensure we're still tracking the buffer size.
extern __ecount(n) int *ReallocInts(int *pInt, int n);
extern unsigned int NewSize();
extern bool Cond();

void Test(int initVal)
{
    int n = initVal;
    int *p = ReallocInts(0, n);

    while (Cond())
    {
        if (Cond())
        {
            n = NewSize();
            p = ReallocInts(p, n);
        }
        if (Cond())
        {
            n = NewSize();
            p = ReallocInts(p, n);
        }
    }
    if (n > 0)
        p[n - 1] = 0;

    p[n] = 0;    // real buffer overrun to ensure we are still tracking the size
}

