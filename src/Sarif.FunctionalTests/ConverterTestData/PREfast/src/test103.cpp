#include "specstrings.h"
#include "undefsal.h"

struct US {
    __field_ecount_part(MaxLength, Length) char *buf;
    size_t Length;
    size_t MaxLength;
};

void Fill(/* __in US *us */ US const * us)
{
    // TODO
}

void Fill1(US *us)
{
    // TODO
}

void f()
{
    char a[100];
    US us{ a, 0, 100 };
    Fill(&us);
    char ch = us.buf[us.Length];    // Somehow, ESPX ignores this. Compare this with f1. Curious...
    us.buf[us.MaxLength] = 1;       // BAD. Overflows US::buf. ESPX:26000 / PFX:23 
    us.buf[100] = 1;                // BAD. Overflows US::buf. ESPX:26000 / PFX:23 
}

void f1()
{
    char a[100];
    US us{ a, 0, 100 };
    Fill1(&us);
    char ch = us.buf[us.Length];    // BAD. Read Overflows US::buf. ESPX:26000 / PFX:23 
    us.buf[us.MaxLength] = 1;       // BAD. Overflows US::buf. ESPX:26000 / PFX:23 
    us.buf[100] = 1;                // BAD. Overflows US::buf. ESPX:26000 / PFX:23 
}

void g(__in US *p)
{
    p->buf[p->Length] = 1;  // BAD. Optential overflow. ESPX:26014. See main for PFX warning.
}

void Make(US *s)
{
    if (!s)
        return;

    char a[100];
    s->buf = a;
    s->Length = 0;
    s->MaxLength = 100; 
}

void g()
{
    US s;
    Make(&s);
    s.buf[s.MaxLength];     // BAD. Overflows. ESPX:26000 [PFXFN] PREfix misses this.
}

char foo(__in US *p)
{
    p->buf[p->MaxLength - 2] = 1;  // should get error writing to __in param
    return p->buf[p->MaxLength - 1];    // OK?
}

void bar(US *p)
{
    p->buf[p->Length] = 1;  // BAD. Potential overflow. ESPX:26014
}

char gbuf[100];
void Init(__out US *p)
{
    p->buf = gbuf;
    p->Length = 100;
    p->MaxLength = 100; 
}

void good()
{
    US u;
    Init(&u);
    u.Length--;
    u.MaxLength--;
    u.buf++;
    foo(&u);    // OK
}

void bad()
{
    US u;
    Init(&u);
    u.Length--;
    u.buf++;
    bar(&u);    // BAD. Overflows. ESPX:26000 / PFX:25
}

void InferredIn(const US *p)
{
    p->buf[p->Length];  // BAD. Potential overflow. ESPX:26014
}

void TestInferredIn(US *p)
{
    p->Length++;
    InferredIn(p);  // p is not changed by InferredIn. Above p->Length++ can cause overflow in other functions. ESPX:26044,26014
}

//ESP bug #406 - should infer __in for 'this' parameter of const methods
struct S {
    __field_ecount_part(5, size) char *buf;
    __range(0, 5) size_t size;

    void foo() const {}
};

void f(__in S *s)
{
    s->foo();
    s->buf; // ???
}

struct Data
{
    unsigned int m_uMax;
    __field_range(<, m_uMax) unsigned int m_uCurrentMax;
    __field_ecount_part(m_uMax, m_uCurrentMax) char *m_data;
};

struct Wrap
{
    Wrap(__in Data *pData);

    Data *m_pData;
};

Wrap::Wrap(__in Data *pData)
{
    m_pData = pData;   // due to a previous optimization for __in params
               // there WAS a bogus postcondition buffer overflow warning here.
               // Also need some amount of alias analysis to ensure there are
               // no bogus postcondition violations.
}

void main()
{
    US us;
    char a[100];

    us.buf = a;
    us.Length = 100;
    us.MaxLength = 100;
    g(&us); // BAD. us.Length will cause us.buf to overflow through g. PFX:25

    char ch = foo(&us); // OK

    us.buf = a;
    us.Length = 100;
    us.MaxLength = 100;
    bar(&us);   // BAD. us.Length will cause us.buf to overflow through bar. PFX:25

    us.buf = a;
    us.Length = 100;
    us.MaxLength = 100;
    InferredIn(&us);   // BAD. us.Length will cause us.buf to overflow through InferredIn. PFX:25
    TestInferredIn(&us);    // BAD. us.Length will cause us.buf to overflow through InferredIn. PREfix seems not reporting this as dup of the above?
}