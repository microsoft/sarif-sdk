#include <specstrings.h>
#include "mymemory.h"
#include "undefsal.h"

struct Common
{
    int m_c1;
    int m_c2;
};

struct Uncommon
{
    int m_c1;
    int m_c2;

    unsigned int m_size;
    __field_ecount(m_size) char m_data[1];
};

void UseCommon(__in Common *pC)
{
    if (pC)
    {
        pC->m_c1 = ++pC->m_c1;
        pC->m_c2 = ++pC->m_c2;
    }
}

void UseUncommon(__in Uncommon *pUC)
{
    if (pUC)
    {
        pUC->m_c1 = ++pUC->m_c1;
        pUC->m_c2 = ++pUC->m_c2;
        if (pUC->m_size > 0)
        {
            char ch = pUC->m_data[pUC->m_size - 1];
        }
    }
}

void TestUseCommon(__inout Uncommon *pUC)
{
    UseCommon((Common *)pUC);    // We used to get false positive here
}

void TestUseUncommon(__inout Uncommon *pUC)
{
    UseUncommon(pUC);    // correctly getting no warning here
}


// test case from Esp:714
class A {
public:
    unsigned int m;

    void add_m(void)
    {
        m = 0;
        return;
    };
};

class B : public A {
private:
    __field_range(0, 3) unsigned int n;

public:

    int safe(void)
    {
        int a[3];
        n = 3;

        for (unsigned int i = 0; i < n; ++i) {
            add_m();
            a[i] = 1;           // We used to get false positive 26014 here
        }
        return (0);
    };

    void safe1(void) {n = 3;};
};

void main()
{
    Uncommon UC;

    UC.m_c1 = 0;
    UC.m_c2 = 0;
    UC.m_size = 1;
    UC.m_data[0] = 1;
    TestUseCommon(&UC);     // OK

    UC.m_c1 = 0;
    UC.m_c2 = 0;
    UC.m_size = 1;
    UC.m_data[0] = 1;
    TestUseUncommon(&UC);   // OK

    B b1;
    b1.safe1();             // OK for PREfix. ESPX reports 26070.

    B b2;
    int rc = b2.safe();     // OK for PREfix. ESPX reports 26070.
}
