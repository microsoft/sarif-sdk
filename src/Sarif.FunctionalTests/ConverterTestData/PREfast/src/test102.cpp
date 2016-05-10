#include "specstrings.h"
#include "mymemory.h"
#pragma prefast(push)
#pragma prefast(disable: 26002 26003 26006 26007 26035 26036, "We are not interested in bugs in stdexcept")
#include <stdexcept>
#pragma prefast(pop)

#include "undefsal.h"

typedef __bcount(cb) __bcount(sizeof(int)) struct _VARSTR {
    int cb;
    int a;
    int b;
    int c;
    int d;
} VARSTR;

__notnull __valid VARSTR* good1()
{
    VARSTR *v = (VARSTR*)malloc(3*sizeof(int)); // BAD. 26026. [PFXFN] PREfix does not treat this unsafe
    if (v)
    {
        v->cb = 3*sizeof(int);
        v->a = 0;
        v->b = 0;
        return v;   // [ESPXFP?] v is OK according to the contract. Why is ESPX complaining?
    }
    else
    {
        throw std::exception();
    }
}

int good2(__in VARSTR *v)
{
    if (v && v->cb >= 4*sizeof(int))
        return v->c;
    return -1;
}

_Ret_cap_c_(1) _Ret_valid_ VARSTR* bad1()
{
    VARSTR *v = (VARSTR*)malloc(2*sizeof(int));  // BAD. 26026. [PFXFN] PREfix does not treat this unsafe
    if (v)
    {
        v->cb = 3*sizeof(int);
        v->a = 0;
    }
    return v;   // BAD. 26045. Returing memory smaller than v->cb bytes.
}

void main()
{
    VARSTR* vs;

    try
    {
        vs = good1();
        *(char*)((char*)vs + vs->cb - 1) = 0;  // OK
    }
    catch(const std::exception& ex)
    {
        // Ignore.
    }

    vs = bad1();
    if (vs)
    {
        *(char*)((char*)vs + vs->cb - 1) = 0;  // BAD. Overflows.
    }
}
