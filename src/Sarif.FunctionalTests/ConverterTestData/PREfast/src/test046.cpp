#include "mywin.h"
// #include "undefsal.h"

struct S {
    int a;
    int b;
};

struct S1 : S {
    int c;
};

void f(/* __in */ S const * const p)
{
    ((S1 *)p)->c;   // BAD. Unsafe. [PFXFN] PREfix does not warn this.
}

void f1(/*__in*/ char const * p) {
    while (*p++)
        ;
}

void f2(/*__in*/ wchar_t const *p) {
    while (*p++)
        ;
}

void f3(/*__in*/ WCHAR const *p) {
    while (*p++)
        ;
}

void f4(/*__in BSTR p */ _Null_terminated_ OLECHAR * p) {  // typedef _Null_terminated_ OLECHAR *BSTR;
    while (*p++)    // OK. null-terminated.
        ;
}

void f4(/*__in*/ BYTE const * p) {
    while (*p++)
        ;
}

void main()
{
    S s;
    S1 s1;

    s.a = s1.a = 1;
    s.b = s1.b = 2;
    s1.c = 3;

    f(&s);      // BAD: [PFXFN] PREfix does not warn...

    char a1[2] = {'a', 'b'};
    f1(a1);     // BAD: [PFXFN] PREfix does not warn...

    wchar_t a2[2] = {L'a', L'b'};
    f2(a2);     // BAD: [PFXFN] PREfix does not warn...

    // Obviously, PREfix doesn't seem to consider unsafe null-terminated string handling.
}