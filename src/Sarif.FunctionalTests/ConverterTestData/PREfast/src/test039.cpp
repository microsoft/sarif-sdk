#include "specstrings.h"
#include "undefsal.h"

// NOTE: Goal of this test case is to compare different error messages we will get
// for an errorneous caller that call same function with different annotations.

void Read1(char *buf, /* __in */ unsigned const *size)
{
    // *size should not change.
    return;        
}
void Read2(char *buf, unsigned *size)
{
    // Let it share same behavior with Read4.
    // *size can change. However, it should not grow.
    switch ((int)buf % 2)
    {
    case 0:
        return;
    case 1:
        *size -= 1;
        return;
    }
}
void Read3(char *buf, const unsigned *size)
{
    // *size cannot not change.
    return;
}
void Read4(__out_ecount_part(*size, *size) char *buf, __inout unsigned *size)
{
    switch ((int)buf % 2)
    {
    case 0:
        return;
    case 1:
        *size -= 1;
        return;
    }
}

void f1()
{
    char buf[100];
    unsigned size = 100;
    Read1(buf, &size);  // void Read1(char *buf, __in unsigned *size)
    buf[size] = 0;
}

void f2()
{
	char buf[100];
	unsigned size = 100;
	Read2(buf, &size);
	buf[size] = 0;
}

void f3()
{
	char buf[100];
	unsigned size = 100;
	Read3(buf, &size);
	buf[size] = 0;
}

void f4()
{
	char buf[100];
	unsigned size = 100;
	Read4(buf, &size);
	buf[size] = 0;
}

void main() { /* dummy */ }
 
