#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

#include <Windows.h>

// Also look for indirect aliases e.g. when a property (not value) is
// passed from one variable to another

void h1c(wchar_t * p)
{
    int i = 6;
    int j = sizeof(p);

    i = i+j;

    int k = i + wcslen(p);
}
