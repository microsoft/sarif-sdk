#include "myspecstrings.h"

int g;

__analysis_inline
int get_g()
{
    return g;
}

void main()
{
    int a[10];
    g = 9;
    a[get_g()] = 1; // OK. We used to get warnings 26015 and 26011 here. No warnings now.

    a[get_g()+10] = 1; // BAD. We used to get warnings 26015 and 26011 here. Get 26000 now. PFX:23.
}
