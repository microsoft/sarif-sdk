#include "mywin.h"

void f()
{
    char a[5] = {'a','b','c','d','e'};
    char b[10] = {'a','b','c','d','e','f','g','h','i','j'};
    GetLongPathName(a, b, 10);
}

void f1()
{
	char a[5] = { 'a','b','c','d','e' };
	char b[10] = { 'a','b','c','d','e','f','g','h','i','j' };
	GetLongPathName(b, a, 10);  // [ESPXFN] ESPX does not warn overrun as no annotation is available. By design.
}

void main() { /* dummy */ }