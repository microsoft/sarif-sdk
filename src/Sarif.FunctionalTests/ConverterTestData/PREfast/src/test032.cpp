// Expansion of valid  annotation to use the annotation on the type. In this case the annotation is on a typedef more than
// one level deep
#include "specstrings.h"

typedef unsigned int WCHAR;

void fill(__writableTo(elementCount(10)) WCHAR *a)
{
    if (a == nullptr)
        return;

    for (size_t i = 0; i < 10; ++i)
    {
        a[i] = 'a' + i;
    }
}

void f()
{
	WCHAR a[10];
	fill(a);
	WCHAR *psz = a;
	while (*psz) {  //Potential overflow here and ESPX warns this. [PFXFN] PREfix doesn't.
		psz++;
	}
}

void main() { /* Dummy */ }
