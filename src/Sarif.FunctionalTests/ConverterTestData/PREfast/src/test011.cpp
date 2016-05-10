extern void CREATION();
extern void SIGNAL();
extern bool getBool(int);

#include <stdlib.h>
#include <memory.h>

typedef char DUALCHAR[2];

void dualchartest(int length, 
                 __writableTo(elementCount(length)) DUALCHAR *myBuffer)
{
    char *funny = myBuffer[0];
    for (int i = 0; i < sizeof(DUALCHAR) * length; i ++)
    {
        funny[i] = 'h';
    }
    funny[sizeof(DUALCHAR) * length] = '\0';
}

int main(int length)
{
    DUALCHAR *x = new DUALCHAR[20];
    if (x == nullptr)
        return -1;

    char *y = x[0];
    *y++ = 0;
    *y++ = 1;
    *y++ = 2;
    *y++ = 3;
    *y++ = 4;
    *y++ = 5;
    *y++ = 6;
    *y++ = 7;
    *(y+=30) = 8;
    *y++ = 38;
    *y++ = 39;
    *y++ = 40;  // BAD. Overflow.
    *y++ = 41;  // BAD. Overflow.
    *--y = 41;  // BAD. Overflow.
    *--y = 40;  // BAD. Overflow.
    *--y = 39;

    x[0][0] = 0;
    x[0][1] = 1;
    x[9][1] = 0;
    x[10][0] = 2;
    x[20][0] = 1;   // BAD. Overflow.

    DUALCHAR dc[20];
    dualchartest(20, dc);   // BAD. dualchartest overflows dc.
}

