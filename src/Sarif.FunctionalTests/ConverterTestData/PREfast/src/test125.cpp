#include <sal.h>
#include "mywin.h"
//
// Test case originally had attribute syntax, but problem repros with old-style
//#define _Deref_post_z_ __deref_out_z
//#define _In_z_ __in_z

void WriteString(__out_ecount_z(cch) char *wz, int cch)
{
    if (wz && cch > 0)
    {
        for (int i = 0; i < cch - 1; ++i)
            *(wz++) = 'a' + i % 26;
        *wz = '\0';
    }
}

template< int cchTo >
    void TGetString(
        _Outref_ _Post_z_ char (&wz)[ cchTo ]
        ) throw()
{
    if (cchTo < 2)
        wz[0] = '\0';
    else
        WriteString(wz, cchTo);
}

size_t StrLen( __in_z const char* wz ) throw()
{
    return strlen(wz);
}

size_t Safe() throw()
{
    char wz[32];
    TGetString( wz );
    return StrLen( wz ); // We used to get a bogus 26035 here
}

void main()
{
    Safe(); // OK
}
