

#include <sal.h>

extern void RequiresZ(_In_z_ char * c);

extern void DoesNotZ(_Out_ _Post_maybez_ char * b);

void BasicC6053(char * x)
{
    DoesNotZ(x);

    RequiresZ(x);
}


void BasicC6054()
{
    char a[5] = {'a'};

    RequiresZ(a);

    char b[5] = {'b', '\0'};

    RequiresZ(b); // No C6054 here
}

