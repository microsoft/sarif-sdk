#include "specstrings.h"
#include "mymemory.h"
#include "undefsal.h"

typedef short BOOL;
void f(BOOL noExtra, int size)
{
	int len = size;
	if (!noExtra)
		len++;

	int *buf = new int[len];
    if (buf == nullptr)
        return;

	memset(buf, 0, size * sizeof(int));

	for (int i = 0; i < size; i++)
		buf[i]++;

	if (!noExtra)
		buf[size] = 1;

    delete[] buf;
}

typedef char TCHAR;
typedef __nullterminated char *LPTSTR;
typedef __nullterminated const char *LPCTSTR;
typedef unsigned long ULONG;
typedef unsigned int DWORD;

DWORD CreateLogString(__in LPCTSTR StringToFormat, __deref_out LPTSTR *LogReadyString)
{
    DWORD dwErr = 0L;
    LPCTSTR lpSrcCursor = 0;
    LPTSTR lpDstCursor = 0;
    LPTSTR lpFormattedString = 0;
    ULONG ulNewlinesToPatch = 0;
    ULONG ulStrToFormatLength = 0;
    ULONG ulLogReadyStrLength = 0;
    const TCHAR Newline = L'\n';
    const TCHAR CarriageReturn = L'\r';

     if ( !StringToFormat  || !LogReadyString )
    {
        dwErr = 87L;
        goto Cleanup;
    }
    
    lpSrcCursor = StringToFormat;
    if ( *StringToFormat == Newline )
    {
        ulStrToFormatLength++;
        ulNewlinesToPatch++;
        lpSrcCursor++;
    }
    
    while ( *lpSrcCursor )
    {
        ulStrToFormatLength++;

        if ( ( *lpSrcCursor == Newline ) && ( *(lpSrcCursor - 1) != CarriageReturn ) )
        {
            ulNewlinesToPatch++;
        }

        lpSrcCursor++;
    }
    
    ulLogReadyStrLength = ulStrToFormatLength + ulNewlinesToPatch;
    if ( ulLogReadyStrLength < ulStrToFormatLength )
    {
        dwErr = 534L;
        goto Cleanup;
    }

    lpFormattedString = (LPTSTR) malloc((ulLogReadyStrLength+1)*sizeof(TCHAR) );
    if (!lpFormattedString)
    {
        //dwErr = GetLastError();
        goto Cleanup;
    }

    lpSrcCursor = StringToFormat;
    lpDstCursor = lpFormattedString;
    if ( *StringToFormat == Newline )
    {
        *lpDstCursor++ = CarriageReturn;
        *lpDstCursor++ = Newline;
        lpSrcCursor++;
    }
    
    while ( *lpSrcCursor )
    {
        if ( ( *lpSrcCursor == Newline ) && ( *(lpSrcCursor - 1) != CarriageReturn ) )
        {
            *lpDstCursor++ = CarriageReturn;    // [ESPXFP] See below
            *lpDstCursor++ = Newline;           // [ESPXFP]
        }
        else
        {
            *lpDstCursor++ = *lpSrcCursor;      // [ESPXFP]
        }

        lpSrcCursor++;
    }

    *lpDstCursor = L'\0';
    *LogReadyString = lpFormattedString;

Cleanup:
    return dwErr;
}

void main()
{
    // [ESPXFP]
    // Warnings for the above function could be due to a known issue in ESPX
    // during loop analysis for lines 62 & 95.
    // PREfix doesn't find a bug in the above function.
}	