//Code Red (MS01-033)

#include "mywin.h"
#include "specstrings.h"
#include "undefsal.h"

char *  __cdecl strchr(char const * _Str, int _Val)
{
    while (*_Str && *_Str != (char)_Val)
        _Str++;

    if (*_Str == (char)_Val)
        return (char*)_Str;

    return nullptr;
}

void DecodeURLEscapes( __in_bcount(*l*2) BYTE * pIn, __in ULONG * l, __out_ecount(*l) WCHAR * pOut )
{
    WCHAR * p2 = pOut;
    WCHAR c1, c2;
    for(ULONG l2 = *l; l2 > 0; --l2)
    {
        c1 = *pIn;
        *p2 = c1;
        pIn+=2;
        pOut+=2;
   }
}

void AddExtensionControlBlock(__in_bcount(cbBuffer) __nullterminated BYTE* pszBuffer, ULONG cbBuffer )
{
    CHAR *pszToken = (CHAR *)pszBuffer;
    while ( (0 != pszToken) && (0 != *pszToken) )
    {
        //
        //  Find the value on the right hand side of the equal sign.
        //
        CHAR *pszAttribute = pszToken;
        CHAR *pszValue     = strchr( pszAttribute, '=' );
        if ( 0 != pszValue )
        {
            ULONG cchAttribute = (ULONG)(pszValue - pszAttribute);
            pszValue++;
            //
            //  Point to the next attribute.
            //
            pszToken = strchr( pszToken, '&' );
            ULONG cchValue;
            if ( 0 != pszToken )
            {
                
                cchValue = (ULONG)(pszToken - pszValue);
                pszToken++;
            }
            else
            {
                cchValue = (ULONG)((CHAR *)&pszBuffer[cbBuffer] - pszValue);
            }
 
            WCHAR wcsAttribute[200];
            if ( cchAttribute >= sizeof wcsAttribute ) // BAD: fix is cchAttribute >= sizeof wcsAttribute/sizeof WCHAR
                return;
           
            DecodeURLEscapes((BYTE*)pszAttribute, &cchAttribute, wcsAttribute); // [PFXFN] PREfix does not warn this...
        }
    }
}


void main()
{
    CHAR* myattrib = "MyAttribute=Attribute value 1&TheirAttribute=Attribute value 2";
    AddExtensionControlBlock((BYTE*)myattrib, strlen(myattrib));
}