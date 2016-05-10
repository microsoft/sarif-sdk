#include "specstrings.h"
#include "undefsal.h"

struct Header {
	int size;
};

typedef __inexpressible_readableTo(h->size) Header *PHEADER;

struct S1 {
	Header h;
	int a;
};

struct S2 {
	Header h;
	int a;
	int b;
};

int f(__in PHEADER h)
{
	if (h->size == sizeof(S1))
	{
		return ((S1*)h)->a;
	}
	else if (h->size == sizeof(S2))
	{
		return ((S2*)h)->b;
	}
	return 0;
}

void g(__out_xcount(n%10) int *a, int n)
{
    // Told not to guess things about n.
}

void h()
{
	int a[10];
	g(a, 10);
}

void main() { /* dummy */ }