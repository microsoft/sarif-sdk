#include "specstrings.h"
#include "undefsal.h"

int a[10];
int f(int x)
{
	int i;
	for (i = 0; i < 10; i++)
		if (x == a[i])
			break;
	if (i == 10)
		return -1;
	return a[i];
};

int f(int x, __in_ecount(n) int *b, int n)
{
	int i;
	for (i = 0; i < n; i++)
		if (x == b[i])
			break;
	if (i == n)
		return -1;
	return b[i];
};

int f1(int x, __in_ecount(n) int *b, int n)
{
	int *p = b;
	while (n > 0) {
		if (x == *p)
			break;
		p++;
		n--;
	}
	if (n == 0)
		return -1;
	return *p;
}

int f2(int x, __in_ecount(n) int *b, int n)
{
	int *p = b;
	int i = 0;
	while (i < n && x != *p) {
		p++;
		i++;		

	}
	if (i == n)
		return -1;
	return *p;
}

int ma[10][20];
int g(int x)
{
	int i, j;
	for (i = 0; i < 10; i++) {
		for (j = 0; j < 20; j++) {
			if (ma[i][j] == x)
				break;
		}
	}
	if (i == 10)
		return -1;
	return i + j;
}

void h(__out_ecount(n) char *p, __nullterminated __in  const char *q, int n)
{
	while (*q && n--)
		*p++ = *q++;
	*p++ = '\0';
}

void main()
{
    for (int i = 0; i < 10; ++i)
        a[i] = i;
    
    f(5);   // OK
    f(-1);  // OK

    f(5, a, 10);    // OK
    f(-1, a, 10);   // OK

    f1(5, a, 10);    // OK
    f1(-1, a, 10);   // OK

    f2(5, a, 10);    // OK
    f2(-1, a, 10);   // OK

    for (int i = 0; i < 10; ++i)
        for (int j = 0; j < 20; ++j)
            ma[i][j] = i * 10 + j;

    g(101); // OK
    g(-1);  // OK

    char* str = "this is a test string of length longer than 'size'";
    char str1[1];
    h(str1, str, 1);    // BAD. PREfix catches this!!
    char str2[2];
    h(str2, str, 2);    // BAD. [PFXFN] PREfix misses this...
}