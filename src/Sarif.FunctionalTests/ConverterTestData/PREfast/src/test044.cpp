//#include <windows.h>
#include "specstrings.h"

typedef __success(return >= 0) long HRESULT;
#define _HRESULT_TYPEDEF_(_sc) ((HRESULT)_sc)
#define E_FAIL                           _HRESULT_TYPEDEF_(0x80004005L)
#define S_OK                                   ((HRESULT)0x00000000L)
#define S_FALSE                                ((HRESULT)0x00000001L)


int glob;
HRESULT foo(__deref_out_ecount(n) int **pp, int n)
{
	if (glob)
	{
		*pp = new int[n];
		return S_OK;
	}
	else
	{
		*pp = new int[n];   // __success tells nothing about what happens on failure.
		return E_FAIL;
	}
}

void main()
{
    int* buf;

    glob = 1;

    HRESULT hr = foo(&buf, 10);
    if (hr == S_OK)
    {
        if (buf != nullptr)
            buf[9] = 1;
    }
}