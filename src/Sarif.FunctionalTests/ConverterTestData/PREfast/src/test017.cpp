#include "specstrings.h"
#include "mymemory.h"
#include <memory.h>

extern __writableTo(elementCount(count)) int *mallocInt(size_t count)
{
    return (int*)malloc(count * sizeof(int));
}

void* operator new (size_t size, int *p)
{
    return malloc(size);
}

void* operator new[](size_t size, int *q)
{
    return 0;
}

struct Widget
{
	void *operator new(size_t size) { return malloc(size); }
	void *operator new(size_t size, int *p) { return malloc(size); }
	void *operator new[](size_t size, int *p) { return malloc(size); }
};

int main(int chunkSize)
{
    if (chunkSize <= 0)
      return 0;
    int *buf = (int *) malloc(sizeof(int) * (chunkSize * 3 + 1));
    if (buf != nullptr)
    {
        int *buf1 = new int [chunkSize * 3 + 1];
        if (buf1 != nullptr)
        {
            int offset1 = chunkSize + 1;
            int offset2 = chunkSize * 2 + 1;
            buf[offset1] = buf[offset2];

            int *buf3 = (int *)realloc(buf, chunkSize * 5 *sizeof(int));
            if (buf3 != nullptr)
            {
                memcpy(buf1, buf1 + chunkSize, chunkSize * sizeof(int));
                buf3[4] = 1;

                int *buf4 = new (buf) int;
                if (buf4 != nullptr)
                {
                    buf4[2] = 1;
                }

                Widget *widgets = new (buf) Widget[5];
                if (widgets != nullptr)
                {
                    memcpy(widgets, buf1, 10);
                }
            }
        }
    }
}

