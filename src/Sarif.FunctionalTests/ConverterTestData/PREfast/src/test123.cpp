#include "specstrings.h"
#include "undefsal.h"

typedef __nullterminated char *PSTR;

struct S {
    __field_ecount(c) PSTR p;
    int c;
};

void foo(__in PSTR s) { }


void bar(S *s)
{
    foo(s->p);   // OK. We used to get a spurious warning 26035 here
}

void main()
{
    S s;
    s.c = 10;
    s.p = new char[10];
    if (s.p)
    {
        s.p[0] = 'a';
        s.p[1] = 'a';
        s.p[2] = 'a';
        s.p[3] = 'a';
        s.p[4] = '\0';

        bar(&s);    // OK

        s.p[9] = '\0'; // OK. [PFXFP] 26018
        s.p[10] = '\0'; // BAD.
    }
}
