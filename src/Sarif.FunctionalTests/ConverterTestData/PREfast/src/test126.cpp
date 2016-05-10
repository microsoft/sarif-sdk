#include <string.h>

#define XML_SUFFIX ".xml"

bool Safe(__in_z const char *cp)
{
    if (cp[0] == '\0')
        return false;
    return cp[1] == XML_SUFFIX[1];
}

bool Unsafe1(__in_z const char *cp)
{
    if (cp[0] == '\0')
        return false;
    return cp[1] == XML_SUFFIX[5];  // BAD. ESPX:26000 / PFX:23
}

bool Unsafe2(__in_z const char *cp)
{
    if (cp[0] == '\0')
        return false;
    return cp[1] == XML_SUFFIX[-1];  // BAD. ESPX:26001 / PFX:24
}

void main() { /* dummy */ }