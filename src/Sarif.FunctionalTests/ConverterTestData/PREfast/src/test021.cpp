#include <memory.h>

void f(char *b)
{
	char a[10];
	memcpy(a, b, 11);
}

void main() { /* dummy */ }