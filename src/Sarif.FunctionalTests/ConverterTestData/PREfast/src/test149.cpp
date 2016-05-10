#include <specstrings.h>

#define NULL 0

// It would technically be more accurate to have a _When_ on the return value's
// annotation to indicate that it returns zero on NULL.  However that's a more
// reasonable semantic to use than 'undefined', as EspX currently does.
_Post_equal_to_(_String_length_(str))
unsigned int mystrlen1(
    __in_z_opt const char *str
    );

_Post_equal_to_(_String_length_(str))
unsigned int mystrlen2(
    __in_z const char *str
    );

// Here the call to mystrlen2 is arguably wrong (or perhaps the SAL on
// mystrlen2 is wrong).  Given the SAL as is, it would be reasonable to have
// a NULL deref warning on the call.  It is not reasonable, however, to
// generate strange buffer overrun warnings because we treat
// _String_length_(NULL) as undefined.
void Test()
{
    char data[10];
    if (mystrlen1(NULL) != mystrlen2(NULL))
        data[10] = 0;
}

