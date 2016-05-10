#include "specstrings.h"
#include "mymemory.h"
#include "undefsal.h"

void Add(__out_ecount_opt(argcount) int **args, size_t argcount, int *arg)
{
	if (arg) {
		if (args) {
			int **tmp = (int**)realloc(args, (argcount+1)*sizeof(int*));
			if (tmp)
				args = tmp;
			else {
				free(args);
				args = nullptr;
			}
			
		}
		else {
			argcount = 0;
			args = (int**)malloc(sizeof(int*));

		}
		if (args == nullptr)
			return;

		args[argcount++] = arg;
	}

    free(args);
}

_Ret_writes_maybenull_(*cnt) char ** Fill(__in char *a, __in char *b, __in char *c, __out size_t *cnt)
{
	size_t count = 0;
	if (a)
		count++;
	if (b)
		count++;
	if (c)
		count++;
	char **buf = new char*[count];
	count = 0;
	if (buf)
	{
		if (a)
			buf[count++] = a;
		if (b)
			buf[count++] = b;
		if (c)
			buf[count++] = c;
	}
	*cnt = count;
	return buf;
}

void f(int *p, int *q)
{
	int a[10];
	if (p == q)
		a[10] = 1;
	if (p == a)     // Both ESPX and PREfix assumes this to be false
		a[10] = 1;
	if (p == 0)
		a[10] = 1;
}

void main() { /* dummy */ }