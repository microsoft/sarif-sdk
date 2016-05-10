#include <specstrings.h>

#define NULL 0

__nullterminated int rgpTest[] = { 1, 2, NULL};

void TestMeOriginal(int riid)
{
    int idx = 0;
    while (1) {
        if (riid == rgpTest[idx]) {
            return;
        }
        idx++;
        if (rgpTest[idx] == 0) {  // real buffer overrun possible here if null is first entry
            break;
        }
    }
}

void TestMeFixed(int riid)
{
    int idx = 0;
    if (rgpTest[0] == NULL)
        return;
    while (1) {
        if (riid == rgpTest[idx]) {
            return;
        }
        idx++;
        if (rgpTest[idx] == 0) {
            break;
        }
    }
}


_Null_terminated_ char MyGlobalString[] = "Hello world";

extern "C"
_Ret_range_(==, strlen(psz)) unsigned int strlen(_In_z_ const char *psz);

unsigned int TestPassToFunction()
{
    return strlen(MyGlobalString);
}

