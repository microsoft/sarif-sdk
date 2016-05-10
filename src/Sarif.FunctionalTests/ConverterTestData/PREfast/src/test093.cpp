#include "specstrings.h"
#include "undefsal.h"

struct S
{
    int a[10];
};


int foo1(__in S *p)
{
    int *q = p->a;

    int *end = q + 10;

    while (q < end)
    {   // Used to get 26001 here due, bug not doing array-to-pointer on 'p->a'
        if (q != p->a && q[-1] == q[0])
          q += 1;
        else
          q += (*q)&3;
    }

    return  q - p->a;
}

int foo2(__in S *p)
{
    int *q = p->a;

    int *end = q + 10;

    while (q < end)
    {   // This version never had a bogus warning
        if (q != &p->a[0] && q[-1] == q[0])
          q += 1;
        else
          q += (*q)&3;
    }

    return  q - p->a;
}

struct ss {
    char a[1024];
};

void foo3(__inout ss *pss, unsigned int d)
{
    char *p = pss->a + (d % 1024);

    if (p != pss->a &&
        *(p-1) == 4)  //  (was: bogus 26001)
    {
        *(p-1) = *p;  //  (no warnings)
        *(p-2) = 1;   //  26011 when {d%1024}==1
    }
}

void main()
{
    S s;
    for (int i = 0; i < 10; ++i)
        s.a[i] = i + 1;
    foo1(&s);   // OK. NOTE: PREfix fails to model foo1.
    foo2(&s);   // OK.
    ss s1;
    for (int i = 0; i < 1024; ++i)
        s1.a[i] = i + 1;
    foo3(&s1, 0);       // OK.  [PFXFP] PREfix think foo3 can underflow.
    foo3(&s1, 1);       // BAD. [PFXFP] PREfix think foo3 can underflow. [PFXFN] It is likely that PREfix does not detect the bug correctly.
    foo3(&s1, 1023);    // OK.  [PFXFP] PREfix think foo3 can underflow.
}