#include "specstrings.h"

int a[100];

void Irreducible(int j)
{
    // [ESPXFN] ESPX cannot analyze this loop with multiple entry points.
    int i = 1;
	
    if ( j > 5)
    {
	j--;
	goto insideloop;
    }
    else
    {

	j ++;
	while (i <= 200)
	{
	    a[i] = 1;
	    i *= 2;
insideloop:
	    j /= 2;
	}
    }
}

void main()
{
    // [PFXFN] PREfix fails to build a model for Irreducible, thus unable find bugs in it.
    Irreducible(10);
    Irreducible(4);
    Irreducible(0);
    Irreducible(-1);
}

