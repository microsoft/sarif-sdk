#include "specstrings.h"
#include "mymemory.h"

#define STR1 L"abcdef"
#define STR2  L"abcdefg"
#define STR1SIZE  sizeof(STR1)
#define STR2SIZE  sizeof(STR2)

typedef unsigned short WCHAR;
typedef __nullterminated WCHAR *PWSTR;
typedef __nullterminated const WCHAR *PCWSTR;

extern "C" unsigned wcslen(__in WCHAR *s);

#include "undefsal.h"

void f()
{
    WCHAR *p = STR1;
    p[10] = 1;  // BAD. Outside of STR1. Also trying to modify string literal. [ESPXFN]
}

void g()
{
    char a[5];
    a[wcslen(STR1)] = 1;    // BAD. a[6] overflows a.
}

void foo()
{
    WCHAR a[100];
    memcpy(a, STR1, 100);   // BAD. Overflows STR1.	
}

void bar(int f)
{
    WCHAR a[100];
    memcpy(a, f ? STR1 : STR2, STR1SIZE + sizeof(short));   // BAD. Can overflow STR1 if f != 0
}

void main() { /* Dummy */ }