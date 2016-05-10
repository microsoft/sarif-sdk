// Vulnerability in NNTP Could Allow Remote Code Execution (MS04-036)

#include "specstrings.h"
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

BOOL WriteOutput(
__nullterminated char *pszText,    //input string
__deref_inout_ecount(*pcOutput) WCHAR **ppwszOutput, //ptr to output - advanced on output
__inout      short *pcOutput)    //available space in output string - updated on output
{
      
    short iText, iOutput;
 
    // copy, converting ASCII to Unicode
    for (iText = 0, iOutput = 0; pszText[iText] != 0; iText++, iOutput++)
    {
        if (iOutput > *pcOutput) return false; // NOTE: check should have been iOutput >= *pcOutput
        (*ppwszOutput)[iOutput] = (char) pszText[iText]; // BAD. It can write 2 bytes past boundary 
    }
     
    *pcOutput -= iOutput; //NOTE: due to incorrect check earlier, can become -1 (maybe OK) or underflow if it were a unsigned type (BAD). This can cause errors for callers.
    *ppwszOutput += iOutput;
 
    return true;
}

void main()
{
    char* str = "My test string.";
    short len = strlen(str);
    --len;  // Let's force to hit the second bug in WriteOutput. See above.
    WCHAR* wstr = (WCHAR*)malloc(len * sizeof(WCHAR));
    if (wstr != nullptr)
    {
        bool succeeded = WriteOutput(str, &wstr, &len);
        if (succeeded)
            WCHAR ch = wstr[len];   // Get the first character. BAD. Can overflow. Also underflows if len can become -1. 
                                    // [PFXFN] PREfix does not detect over/underflow.
                                    // [ESPXFN] PREfix does not detect underflow.
    }
}