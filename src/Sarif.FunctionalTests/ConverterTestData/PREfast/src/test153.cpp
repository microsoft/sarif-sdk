#include <specstrings.h>

typedef __nullterminated char *PSTR;

extern void GetXCount(__deref_out_xcount(Foo) void **p);

__post __valid PSTR Str();

__post __valid PSTR GetString()
{
    PSTR result = Str();

    GetXCount(reinterpret_cast<void **>(&result));

    return result;
}

