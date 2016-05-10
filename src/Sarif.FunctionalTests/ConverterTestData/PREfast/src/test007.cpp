#include "specstrings.h"
#include "mymemory.h"

__writableTo(elementCount(count)) char *mallocChar(size_t count)
{
    return (char*)malloc(count * sizeof(char));        
}

char *myConcat(__writableTo(elementCount(sizeA)) char *bufferA, int sizeA,
               __writableTo(elementCount(sizeB)) char *bufferB, int sizeB)
{
    if (bufferA == nullptr || bufferB == nullptr)
        return nullptr;

    // Great!  EspX forced me to add this line.
    if (sizeA < 0 || sizeB < 0)
        return nullptr;

    int sizeResult = sizeA + sizeB;

    char *bufferResult = mallocChar(sizeResult);
    if (bufferResult == nullptr)
        return nullptr;

    for (int i = 0; i < sizeA; i ++)
    {
        bufferResult[i] = bufferA[i];
    }
    for (int j = 0; j < sizeB; j ++)
    {
        bufferResult[j + sizeA] = bufferB[j];   // [PFXFP] PREfix thinks bufferResult can overflow.
    }
    return bufferResult;
}

int main() { /* dummy */ }
