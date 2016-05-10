#include "specstrings.h"
#include "undefsal.h"

struct S {
    int count;
    int a[100];
    int f;
};

struct Flex {
    int count;
    int arr[1];
};


void SimpleOverflow(__in S *s)
{
    s->a[100] = 1;  // BAD. Overflow. [PFXFN] PREfix treats 'a' just as a part of S.
}

void OffByOne(S *s)
{
    for (int i = 0; i <= 100; i++)
        s->a[i] = 1;    // BAD. Overflow. [PFXFN] PREfix treats 'a' just as a part of S.
}


void TestFlexArray(__in Flex *s)
{
    for (int i = 0; i < s->count; i++)
        s->arr[i]++;    // BAD. Can overflow if count > 1
}

void bar(__in S *s)
{
    for (int i = 0; i < s->count; i++)
        s->a[i]++;      // BAD. Can overflow. [PFXFN] PREfix treats 'a' just as a part of S.
}

void TestLocal()
{
    S s;
    s.a[100] = 1;       // BAD. Overflow. [PFXFN] PREfix treats 'a' just as a part of S.
}

struct S1 {
    S s;
    S *p;
    int g;
};

void TestPointedTo(S1 *p)
{
    p->s.a[100] = 1;    // BAD. Overflow. [PFXFN] PREfix treats 'a' just as a part of S.
    p->p->a[100] = 1;   // BAD. Overflow. [PFXFN] PREfix treats 'a' just as a part of S.
}

struct Complex {
    int count;
    S s[50];
    int f;
};

void TestNestedStructArrays(Complex *p)
{
    for (int i = 0; i < 50; i++)
        for (int j = 0; j < 101; j++)
            p->s[i].a[j] = 1;   // BAD. Overflow. [PFXFN] PREfix treats 'a' just as a part of S.
}

//Incorrect annotation
void TestArrayOfStructs1(__in S *p, int n) 
{
    for (int i = 0; i < n; i++)
        p[i].a[0] = 1;          // BAD. May overflow p.
}

//Overflowing struct array
void TestArrayOfStructs2(__in_ecount(n) S *p, int n) 
{
    for (int i = 0; i <= n; i++)
        p[i].a[0] = 1;          // BAD. overflows p.
}

//Overflowing field array.
void TestArrayOfStructs3(__in_ecount(n) S *p, int n)
{
    for (int i = 0; i <= n; i++)
        p[i].a[100] = 1;        // BAD. overflows p and p.a. [PFXFN] PREfix treats 'a' just as a part of S.
}

void main()
{
    S s;

    SimpleOverflow(&s); // BAD. [PFXFN] See SimpleOverflow
    OffByOne(&s);       // BAD. [PFXFN] See OffByOne

    Flex f;
    f.count = 2;
    f.arr[0] = 1;
    TestFlexArray(&f);  // BAD.

    s.count = 101;
    bar(&s);            // BAD. [PFXFN] See bar
    s.count = 102;
    bar(&s);            // BAD. PREfix catches this as it will access memory outside of s.

    Complex c;
    TestNestedStructArrays(&c); // BAD. [PFXFN] See TestNestedStructArrays

    S sa[10];
    TestArrayOfStructs1(sa, 11);    // BAD. Overflows sa.
    TestArrayOfStructs2(sa, 10);    // BAD. Overflows sa.
    TestArrayOfStructs2(sa, 10);    // BAD. Overflows sa and sa.a. Partial [PFXFN]. See TestArrayOfStructs2.
}