#include "AF_LocalRemoteFunctions.h"

void LocalFunction(int* x);

void LocalFunction(int* x, int* y);

int UseLocalFunction(int * p)
{
	if (p == 0)
		int x = 5;
	LocalFunction(p);
    RemoteFunction(p, p);
	return *p;
}
