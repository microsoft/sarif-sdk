#include "specstrings.h"
#include "mymemory.h"
#include "undefsal.h"

struct S {
    int elts;
    int arr[0];    
};

void f(__in_bcount(sizeof(S) + s->elts * sizeof(int)) S *s)
{
    s->arr[s->elts] = 1;    // BAD. Overflow.
}

extern void g(__in_bcount(s->size1) char *p, S *s); // BAD annotation but not used.  No corresponding PREfix test.

extern void h(__in_bcount(s->arr) char *p, S *s); //  BAD annotation but not used.  No corresponding PREfix test.

extern void h1(__in_bcount(s.arr) char *p, int s); //  BAD annotation but not used.  No corresponding PREfix test.

struct Context
{
    size_t len1;
    size_t len2;
};

struct S1 {
    Context *ctx1;
    Context *ctx2;
};

#define min(a, b)      ((a) < (b) ? (a) : (b))

void foo(__in_ecount(s1.ctx1->len1) char *pData1, __in_ecount(s1.ctx1->len2) char *pData2,
     __out_ecount(s1.ctx2->len1) char *pData3, __out_ecount(s1.ctx2->len2) char *pData4,
    S1 s1
    )
{
    memcpy(pData3, pData1, s1.ctx1->len1);  // BAD.
    memcpy(pData4, pData2, min(s1.ctx1->len2, s1.ctx2->len2));  // OK.
}

void bar(__in_ecount(s1.ctx1->len1) char *pData1, __in_ecount(s1.ctx1->len2) char *pData2,
     __out_ecount(s1.ctx2->len1) char *pData3, __out_ecount(s1.ctx2->len2) char *pData4,
    const S1 &s1
    )
{
    memcpy(pData3, pData1, s1.ctx1->len1);  // BAD
}

class C {
    char x[100];
    int n;
public:
    C(int size)
    {
        for (int i = 0; i < 100; )
            x[i] = ++i;
        n = size;
    }

    void __stdcall f(__out_ecount(this->n) char *a);
};

void __stdcall C::f(__out_ecount(this->n) char *a)
{
    memcpy(a, x, n+1);  // BAD. Overflows a and potentially overflows x.
}

void fieldSameAsParam1(__out_ecount(p.elts) char *buf, __in S &p, int elts)
{
    buf[p.elts] = 1;    // BAD. Overflows buf.
}

void fieldSameAsParam2(__out_ecount(p->elts) char *buf, __in S *p, int elts)
{
    if (buf == nullptr || p == nullptr)
        return;

    for (int i = 0; i < p->elts; ++i)
        buf[i] = 1;    // OK.
}

void main()
{
#pragma warning(push)
#pragma warning(disable:26005)  // We are not testing zero-length array access
    S s;
    s.elts = 0;
    f(&s);  // BAD. Will overflow.

    char cA[20] = {1};
    char cB[20] = {2};
    char cC[10];
    char cD[10];

    S1 s1;
    Context ctx1;
    Context ctx2;
    s1.ctx1 = &ctx1;
    s1.ctx2 = &ctx2;

    s1.ctx1->len1 = 20;
    s1.ctx1->len2 = 20;
    s1.ctx2->len1 = 10;
    s1.ctx2->len2 = 10;
    foo(cA, cB, cC, cD, s1);    // BAD. foo overflows cC.

    s1.ctx1->len1 = 20;
    s1.ctx1->len2 = 20;
    s1.ctx2->len1 = 10;
    s1.ctx2->len2 = 10;
    bar(cA, cB, cC, cD, s1);    // BAD. foo overflows cC.

    C c1(100);
    char cC1[100];
    c1.f(cC1);          // BAD. Overflow. [PFXFN] PREfix does not warn read overflow of c1.x

    S sE;
    sE.elts = 10;
    int elts = 8;
    char cE[10];
    fieldSameAsParam1(cE, sE, elts);    // BAD. Overflows cE.
#pragma warning(pop)
}

