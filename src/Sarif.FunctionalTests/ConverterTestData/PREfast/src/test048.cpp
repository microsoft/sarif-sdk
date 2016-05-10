#include "specstrings.h"

void *operator new[] (size_t size);

static void f(__out_ecount(n) int *p, size_t n)
{
    if (p == nullptr)
        return;
    while (n--)
       p[n] = 1;
}

static void g(__in_ecount(n) int *q, size_t n)
{
	q[n] = 1;   // BAD
	int *p = new int[10];
	f(p, 10 *sizeof(int));  // BAD: Second param should be elem count.
    if (p != nullptr)
        delete[] p;
}

void main()
{
    int a[10] = {};
    g(a, 10);
}