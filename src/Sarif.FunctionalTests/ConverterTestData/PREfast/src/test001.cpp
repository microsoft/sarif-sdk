#include <specstrings.h>
#include "mymemory.h"

int index(__pre __valid __pre __writableTo(elementCount(count)) int *vec, int count, int i)
{
    if (vec == nullptr)
        return -2;

    int a = vec[i]; // BAD. Potential overflow/underflow
    
    if (i >= 0 && i < count)
    {
        int b = vec[count - i]; // BAD. Overflows if i == 0. [PFXFN] PREfix does not report this.
        int c = vec[count - i - 1]; // OK. 0 <= count - i - 1 < count 
        int d = vec[i++]; // OK
        int e = vec[count - i]; // OK. Same as above count - i - 1
        int f = vec[count - i - 1]; // BAD. Potential underflow if i == count - 1 on input. [PFXFN] PREfix does not report this.
        vec--;
        vec[i]; // OK. vec-- and i++ above makes this equal to vec[i] without them.
        return vec[i+1]; // BAD. Potential overflow if i == count - 1 on input.
    }

    return -1;
}

__writableTo(elementCount(count)) int *mallocInt(size_t count)
{
    return (int*)malloc(count * sizeof(int));
}

int main()
{
    int *a = mallocInt(10);
    if (a != nullptr)
    {
        int *b = a+3;           // b points to 12th byte
        char *c = (char *) b;   
        (2+c)[25];              // Access the last byte at offset (12 + 2) + 25. 

        b[2] = 1;
        b[7] = 1;   // BAD. Overflow. Same as a[10].

        for (int i = 0; i < 10; i ++)
        {
            a[i] = 1;   // OK
        }
    }
    
    int length = 10;
    int *d = (int *) malloc(sizeof(int) * length);
    if (d != nullptr)
    {
        char *dInChars = (char*)d;
        (dInChars+5)[35] = 1;       // BAD. Overflow. Access one byte past the end (i.e., the 40th byte).
        (7+d)[44] = 1;              // BAD. Overflow. Tries to access 51st element.
    }

    // Test index function
    int buf[10] = {1,2,3,4,5,6,7,8,9,10};
    index(buf, 10, -1); // BAD. Underflows thru vec[i]
    index(buf, 10, 10); // BAD. Overflows thru vec[i]
    index(buf, 10, 0);  // BAD. Overflows thru vec[count - i]. [PFXFN] PREfix does not report this.
    index(buf, 10, 9);  // BAD. Overflows thru (--vec)[(++i) + 1]. Underflows thru vec[count - (++i) - 1]. [PFXFN] PREfix does not report underflow.
}
