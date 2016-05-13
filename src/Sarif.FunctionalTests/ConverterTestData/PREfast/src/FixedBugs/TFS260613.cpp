
// Key Events for "sizeof(int)" 

int ComplexMismatch1()
{
    int source [] = {1, 2, 3, 4, 5, 6, 7};
    int * p = source;

    int offset = sizeof(int);
    p += offset;

    return *p;
}

int ComplexMismatch2()
{
    int source [] = {1, 2, 3, 4, 5, 6, 7};
    int * p = source;

    p += sizeof(int);

    return *p;
}
