#pragma warning(disable:4100)
#include <specstrings.h>
#include "undefsal.h"

//
// Test case for Esp:758 -
//     No warnings expected here.
//

//
// On success returns the number of characters written, excluding the trailing
// null.
//
__success(return)
bool
TackOn(
    __out_ecount_part(*Size, *Size + 1) char *Buffer,
    __inout /* __deref_out_range(<, _Old_(*Size)) */ unsigned int *Size
    );

__success(return)
bool
PutTogether(
    __out_ecount_part(*Size, *Size + 1) char *Buffer,
    __inout unsigned int *Size)
{
    unsigned int FirstPartLength = *Size;

    if (TackOn(Buffer, &FirstPartLength))
    {
        Buffer[FirstPartLength++] = '+';
        
        unsigned int SecondPartLength = *Size - FirstPartLength;

        if (TackOn(&Buffer[FirstPartLength], &SecondPartLength))
        {
            if (SecondPartLength == 1 &&
                Buffer[FirstPartLength] == '?')
            {
                Buffer[FirstPartLength] = '!';
            }

            *Size = FirstPartLength + SecondPartLength - 1;

            return true;
        }
    }

    return false;
}

