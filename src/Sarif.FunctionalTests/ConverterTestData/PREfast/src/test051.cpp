#include "specstrings.h"
#include "undefsal.h"

class I {
public:
    void Fill(__out_ecount_part_opt(*size, *size) int *buf, __inout unsigned *size);
};

void I::Fill(__out_ecount_part_opt(*size, *size) int *buf, __inout unsigned *size)
{
    const int default_size = 100;
    if (buf == nullptr)
    {
        *size = default_size;
        return;
    }

    for (unsigned i = 0; i < *size; ++i)
    {
        buf[i] = i + 1;
    }
}

void g(I *p)
{
    int a[10];
    unsigned size = 11;
    p->Fill(a, &size);  // BAD. size of a is 10, but we are passing 11.
}

void f(I *p)
{
    unsigned size;
    p->Fill(0, &size);
    int * buf = new int[size];
    if (buf != nullptr)
    {
	    for (int i = 0; i < size; i++)
	        buf[i] = 0;
	    p->Fill(buf, &size);
	    for (int i = 0; i < size; i++)
	    {
	        buf[i]++;   // OK    	
	    }

        delete[] buf;
    }
}

void main()
{
    I i;
    f(&i);
}