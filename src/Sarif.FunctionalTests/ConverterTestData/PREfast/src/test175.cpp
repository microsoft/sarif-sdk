#include <specstrings.h>
#include "undefsal.h"

_Post_satisfies_(return == 0 || return == 1)
int Foo(_In_ int Select)
{
    switch(Select)
    {
    case 1:
        return 1;
    default:
        return 0;
    }
}

_Post_satisfies_(return == 0 || return == 1)
int
main(
    _In_    int     Argc,
    _In_    char**  Argv
    )
{
    return Foo(Argc);
 
    Argv;
}
