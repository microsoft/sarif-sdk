#include "specstrings.h"
typedef unsigned int DWORD;
typedef char *PUCHAR;
typedef size_t ULONG_PTR;

struct S {
    int a[10];
    int b;
};

struct S1 {
    char a[10];
    char b;
};

DWORD g(S1* buf)
{
    if (buf == nullptr)
        return 0;

    return (DWORD)buf->a[buf->b - 'a']; // Yes, obviously risky. For PREfix, this is not a bug, yet - needs a buggy caller.
}

void f(__in_bcount(size) S1 *buf, DWORD size)
{
    DWORD offset = g(buf);  // ESPX warns this as unbound use of buf. PREfix does not. -128 <= g(buf) <= 127
    S *s = (S *)((PUCHAR)buf + offset); // ESPX warns this as unsafe cast. PREfix does not.
    if (!((ULONG_PTR)s + sizeof(S) <= (ULONG_PTR)buf + size))
        return;
    s->b = 1;       // It may underflow buf if offset < -sizeof(S1)
}

#if 0
//TODO: make this work
void f(void* h)
{
    size_t contexts[10];
    if (h == 0)
	return;
    contexts[(unsigned)(unsigned *)h - 1] = 0;
}
#endif

void main()
{
    S1 buf;
    for (int i = 0; i < 10; ++i)
        buf.a[i] = 'a' + i;
    buf.b = 'a' + 10;   // OK
    f(&buf, 11);
    ++(buf.b);          // Cause g to access buf past buf.b
    f(&buf, 11);
    buf.b = 'a' - 1;
    f(&buf, 11);        // Cause g to underflow buf.a

    buf.b = 'a';
    buf.a[buf.b - 'a'] = (char)(0x7F);
    f(&buf, 11);    // BAD. Can underflow buf. See line 30.
}