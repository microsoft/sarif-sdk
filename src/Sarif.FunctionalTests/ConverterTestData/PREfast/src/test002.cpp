#include "specstrings.h"
#include "mymemory.h"

__writableTo(elementCount(count)) int *mallocInt(size_t count)
{
    return (int*)malloc(count * sizeof(int));
}

void fill(__pre __writableTo(elementCount(count1)) 
          int **matrix, int count1, 
          int count2, int i, int j)
{
    if (matrix == nullptr)
        return;

    if (0 <= i && i < count1)
    {
        matrix[i] = mallocInt(count2);
        if (matrix[i] == nullptr)
            return;

        if (0 <= j && j < count2)
          matrix[i][j] = 0;
    }
}

int funny(__pre __writableTo(elementCount(count1)) int *vector1, int count1)
{
    if (vector1 == nullptr)
        return 0;

    int i;
    for (i = 0; i < count1; ++i)
        vector1[i] = i;

    return i;
}

int main()
{
    int b = 10; // some unknown value
    int *a = mallocInt(b);
    if (a != nullptr)
    {
        funny(a, b);
        funny(++a, --b);
        funny(a, --b);
        funny(++a, ++b);
    }
}
