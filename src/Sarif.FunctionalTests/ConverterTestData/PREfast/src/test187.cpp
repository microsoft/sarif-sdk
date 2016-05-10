#include <specstrings.h>

typedef unsigned short wchar_t;
typedef _Null_terminated_ const wchar_t* PCTSTR;
typedef _Null_terminated_ wchar_t* PTSTR;
typedef wchar_t WCHAR;
typedef unsigned long ULONG;
typedef bool BOOL;
#define TRUE true
#define FALSE false

#include "undefsal.h"

_Success_(return != FALSE)
BOOL IsPatternSpecial(WCHAR c);

_Success_(return != FALSE)
BOOL
QuotePatternSpecials(__in_ecount(InChars) PCTSTR In,
                     __in ULONG InChars,
                     __out_ecount(OutChars) PTSTR Out,
                     __in ULONG OutChars)
{
    //
    // Make a pass over the input string and
    // produce an output string that will be interpreted
    // literally by the dbghelp pattern matcher
    // (i.e. all regex chars are quoted).
    //

    while (InChars--)
    {
        if (IsPatternSpecial(*In))
        {
            if (OutChars < 1)
            {
               // return FALSE;
               break;
            }
            *Out = '\\';  //Warning 26010 here
            Out++;    
            OutChars--;
        }

        if (OutChars < 1)
        {
            //return FALSE;
            break;
        }

        *Out = *In++;
        Out++;
        OutChars--;
    }

    if (OutChars < 1)
    {
        return FALSE;
    }

    *Out = 0;
    return TRUE;
}


