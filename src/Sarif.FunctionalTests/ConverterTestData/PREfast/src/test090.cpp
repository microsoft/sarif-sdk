#include "myspecstrings.h"
#include "mymemory.h"

inline __analysis_inline int add1(int x)
{
    return x + 1;
}

void g(int x)
{
    int a[10];
    x = add1(9);
    a[x] = 1;   // BAD. Overflows.
}

__analysis_inline int add9(int x)
{
    x++;
    x++;
    x++;
    x++;
    x++;
    x++;
    x++;
    x++;
    x++;
    return x;
}

void calladd9()
{
    int a[10];
    a[add9(1)] = 1;     // BAD. Overflows.
}

__analysis_inline void add1deref(int *x)
{
    (*x)++;
}

void calladd1deref()
{
    int a[10];
    int x = 9;
    add1deref(&x);
    a[x] = 1;   // BAD. Overflows.
};

struct C {
    char *buf;
    size_t size;
    __analysis_inline char *GetBuffer() { return buf; }
    __analysis_inline size_t GetSize() { return size; }
};

void InLoop(C *p)
{
    int *a = new int[p->GetSize()];
    if (a == nullptr)
        return;

    for (int i = 0; i < p->GetSize(); i++)
    {
        a[i] = 1;   // OK
    }

    delete[] a;
}

void CallAccessor(C *p)
{
    char a[10];
    if (p->GetSize() <= 10)
    {
        memcpy(a, p->GetBuffer(), p->GetSize() + 1);    // BAD. May overflow if GetSize returns 10.
    }    
}

size_t glob;

__analysis_inline size_t returnGlob(int i)
{
    return i == 0 ? 0 : glob+i;
}

void foo()
{
    int a[10];
    a[returnGlob(0)] = 1;
    a[returnGlob(10)] = 1;  // BAD. May overflow. PREfix does not consider this as bug. It needs info on glob.
}

__analysis_inline void *alloc(size_t size)
{
    return new char[size];
}

void callAlloc()
{
    char a[10];
    a[1] = 1;
    char *p = (char *)alloc(10);
    if (!p)
        return;
    p[10] = 1;  // BAD. Overflows. [ESPXFN] Somehow, ESPX misses this. Why?
    free(p);
}

__analysis_inline int Recursive(int i)
{
    return i == 0 ? 1 : i + Recursive(i-1);
}

void callRecursive(int i)
{
    int a[10];
    a[Recursive(4)] = 1;    // BAD. Overflows. [PFXFN] PREfix cannot build model for Resursive, and misses this. [ESPXFP] This does not underflow.
}

__analysis_inline int add(int a, int b)
{
    return a + b;
}

void repeated()
{
    int a[10];
    int x = add(add(1, add(3, 4)), add(5, 3));
    a[x] = 1;   // BAD. Overflows.
}

void Access(__out_ecount(n) char *p, size_t n)
{
    if (p != nullptr)
        char ch = p[n-1];
}

void CallAccess(__out_ecount(n) char *p, size_t n)
{
    Access(p, n+1); // BAD. Overflows.    
}

__analysis_inline unsigned int min(int a, int b)
{
    return a < b ? a : b;
}

void callMin(unsigned int i)
{
    int a[10];
    a[min(i, 9)] = 1;   // OK
}

__analysis_inline unsigned int loop(unsigned int n)
{
    while (n > 0)
        n--;
    return n;
}

void callLoop(unsigned int n)
{
    int a[10];
    n = loop(10);
    a[n] = 1;   // OK. [ESPXFP] loop() returns 0 and it won't overflow.
}

//Test complicated parameter binding
__analysis_inline void add(int a, int b, int *c)
{
    *c = a + b;
}

struct S1 {
    int x;
};

struct S {
    int a;
    int b;
    int c;
    S1 *next;
};

void callAdd(S *p)
{
    int a[10];
    p->a = 1;
    p->b = 10;
    add(p->a, p->b, &p->c);
    a[p->c] = 1;    // BAD. Overflows. [PFXFN] PREfix misses this. Not sure why.
}

__analysis_inline void set(S1 *s1)
{
    s1->x = 10;
}

void callSet(S *s)
{
    int a[10];
    set(s->next);
    a[s->next->x] = 1;  // BAD. Overflows.
}

__analysis_inline __analysis_noinline int conflicting(int x)
{
    return x + 1;
}

void main()
{
    C c;
    c.buf = "123456789";
    c.size = 10;
    CallAccessor(&c);   // BAD. Overflows.

    char *str = "123456789";
    CallAccess(str, 10);    // BAD. Overflows.

    callMin(20);    // OK

    callLoop(20);   // OK

    S s;
    callAdd(&s);    // BAD. Overflows. [PFXFN] PREfix misses this. Not sure why.

    S1 s1;
    s.next = &s1;
    callSet(&s);    // BAD. Overflows. [PFXFN] PREfix misses this. Not sure why.
}