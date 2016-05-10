#include "specstrings.h"
#include "mymemory.h"
#include <memory.h>
#include "undefsal.h"

typedef __nullterminated char *PSTR;
typedef PSTR LPSTR;

void bar(char const * sz) // void bar(__in char *sz)
{
    while (*sz++)   // BAD. It can overflow.
        ;
}

void baz(__in LPSTR sz)
{
    while (*sz++)   // OK for ESPX per annotations. For PREfix, this can overflow, but PREfix does not warn.
        ;
}

void blah(__inout __typefix(LPSTR) char *sz)
{
    while (*sz++)   // OK for ESPX per annotations. For PREfix, this can overflow, but PREfix does not warn.
        ;
}

typedef struct S {
	int a;
	int b;
} SSS, *UFO;

struct T {
    int a; 
    int b;
    int c;
};

typedef struct T TTT;

typedef S* SP;
typedef __readableTo(elementCount(2)) S* SP1;

char g(__typefix(SP) __valid void *p)
{
	char ch = ((char *)p)[7];   // OK
	return ((char *)p)[8];      // BAD
}

void h(__typefix(LPSTR) __valid char *x)
{
    int len = 0;
    while (*x++)
        ++len;  // OK for ESPX per annotations. For PREfix, this can overflow, but PREfix does not warn.
}

void g1()
{
    char a[5] = {'a','b','c','d','e'};
    h(a);  // ESPX should give error here. [PFXFN] PREfix does not report overrun for this call.
}

void blah(__in_bcount(sizeof(SSS)+numOfT*sizeof(TTT)) S *p, size_t numOfT)
{
    if (p->a)
      return;

    T *q = (T*) (p + 1);    // If numOfT < 1, BAD!
    for (size_t i = 0; i <= numOfT; ++ i)
    {
        if (q[i].c)
            return;
    }
}

struct Header {
    double  xxx;  // forces alignment and size to be same of x86 and amd64 tests
	char *name;
};

typedef struct Header HEADER;

void foo(
    __out_bcount(sizeof(HEADER) + n * sizeof(unsigned int)) char *buf,
    unsigned int n)
{
	unsigned int *p = (unsigned int *)(buf + sizeof(Header));
    p[n-1] = 0; // OK
	p[n] = 0;   // BAD - 26000 must-overflow
}

void main()
{
    char s1[3] = {'a','b','c'};

    // PREfix does not report overrun for the following three calls
    bar(s1);    // [PFXFN]
    baz(s1);    // [PFXFN]
    blah(s1);   // [PFXFN]

    SSS s2;
    memset(&s2, 0, sizeof(SSS));
    g((void*)&s2);
    blah(&s2, 0);

    char* c1 = (char*)malloc(sizeof(HEADER) + 3 * sizeof(unsigned int));
    if (c1 != nullptr)
    {
        HEADER* h1 = (HEADER*)c1;
        h1->xxx = 1;
        h1->name = s1;
        foo(c1, 3);
    }
}