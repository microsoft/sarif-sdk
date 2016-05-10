#include "mywin.h"

#define PART1 L"Blahblah\\"
#define PART2 L"Blah\\"

PWSTR Foo(_Inout_z_ PWSTR pPath)
{
    if (wcslen(pPath) < 30)
        return NULL;

    PWSTR pwch = pPath + 30;
    if (wcslen(pwch) == 5)
    {
        size_t len = wcslen(pwch);
        pwch[10] = '\0';   // buffer overrun warning expected here
    }
    return pPath;
}

