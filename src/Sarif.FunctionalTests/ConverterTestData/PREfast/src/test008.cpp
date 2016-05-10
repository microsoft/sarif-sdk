#include "specstrings.h"
#include "mymemory.h"

__writableTo(elementCount(count)) char *mallocChar(size_t count)
{
    return (char*)malloc(count * sizeof(char));        
}

struct buffer
{
    char *content;
    int size;
};

buffer myConcat(__writableTo(elementCount(sizeA)) char *bufferA, int sizeA,
               __writableTo(elementCount(sizeB)) char *bufferB, int sizeB)
{
    buffer myBuffer;

    if (bufferA == nullptr || bufferB == nullptr)
    {
        myBuffer.content = nullptr;
        myBuffer.size = 0;
        return myBuffer;
    }

    myBuffer.size = sizeA + sizeB;
    myBuffer.content = mallocChar(myBuffer.size);
    if (myBuffer.content == nullptr)
    {
        myBuffer.size = 0;
        return myBuffer;
    }

    for (int i = 0; i < sizeA; i ++)
    {
        myBuffer.content[i] = bufferA[i];
    }

    for (int j = 0; j < sizeB; j ++)
    {
        myBuffer.content[j + 2] = bufferB[j];
    }

    return myBuffer;
}

void main()
{
    char a[1] = {'a'};
    char b[5] = {'b', 'c', 'd', 'e', 'f'};
    buffer buf = myConcat(a, 1, b, 5);
}