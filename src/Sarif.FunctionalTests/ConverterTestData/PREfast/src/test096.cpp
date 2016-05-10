#include "myspecstrings.h"

typedef __success(return >= 0) long HRESULT;

#define FACILITY_WIN32                   7

#define S_OK                                   ((HRESULT)0L)
#define ERROR_INSUFFICIENT_BUFFER        122L    // dderror

__forceinline
HRESULT 
HRESULT_FROM_WIN32(unsigned long x) { return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);}

typedef char CHAR;
#define CONST const
typedef CHAR *PCHAR, *LPCH, *PCH;
typedef CONST CHAR *LPCCH, *PCCH;

typedef __nullterminated CHAR *NPSTR, *LPSTR, *PSTR;

bool QQ()
{
    static int magic;
    return (magic++ & 1);
}

HRESULT MyFunction( __out_ecount(len) LPSTR psz, int len )
{
    int aa[10];
    HRESULT hr = S_OK;
    if ( QQ() )
    {
        *psz = '\0';
    }
    else
    {
        hr = HRESULT_FROM_WIN32( ERROR_INSUFFICIENT_BUFFER );  // << inline fn
    }
    return hr;     // 26036 when HRESULT_FROM_WIN32 returns S_OK. [PFXFN] PREfix does not enforce this.
}


HRESULT ConstantFoldingTest(unsigned long rc)
{
    int aa[10], bb[10], cc[10];
    HRESULT hr = S_OK;

    if (QQ())
    {
        hr = HRESULT_FROM_WIN32( ERROR_INSUFFICIENT_BUFFER );   // hr == 0x8007007a
        aa[hr] = 1;   // 26001, 26015 due to lack of folding. PREfix reports underflow.  
        return hr;
    }

    if (QQ())
    {
        hr = HRESULT_FROM_WIN32( 0xDEADBEEF );      // hr == 0xDEADBEEF
        bb[hr] = 1;   // 26001, 26015 due to lack of folding. PREfix reports underflow.
        return hr;
    }

    if (QQ())
    {
        hr = HRESULT_FROM_WIN32( 0 );   // hr == 0
        cc[hr-99] = 0;   // 26001, 26015 due to lack of folding. PREfix reports underflow.
    }

    return hr;
}

void MyFunction2(unsigned long rc)
{
    if (rc > 0 && rc < 999)
    {
        int aa[10];
        HRESULT hr = HRESULT_FROM_WIN32( rc );  // hr == 0x80070001 ~ 0x800703e6 
        aa[hr+1000]=1;  // 26000, 26011. [PFXFN] PREfix misses this.
    }
}

void MyFunction3(unsigned long rc)
{
    if (rc == 0)
    {
        int aa[10];
        HRESULT hr = HRESULT_FROM_WIN32( rc );  // hr == 0
        aa[hr+1000]=1;    // 26000, 26011.  PREfix reports overflow. 
    }
}

void MyFunction4(unsigned long rc)
{
    if (rc >= 0x80000000)
    {
        int aa[10];
        HRESULT hr = HRESULT_FROM_WIN32( rc );  // hr < 0
        aa[hr]=1;    // 26015, 26001. PREfix reports underflow.
    }
}

void MyFunction5(unsigned long rc)
{
    if (rc != 0)
    {
        int aa[10];
        HRESULT hr = HRESULT_FROM_WIN32( rc );
        aa[hr]=1;    // 26015, 26001
    }
}

void main()
{
    MyFunction2(1);     // BAD. Underflows. [PFXFN] PREfix misses this.
    MyFunction2(998);   // BAD. Underflows. [PFXFN] PREfix misses this.

    MyFunction5(0x80000023);    // BAD. Underflows. [PFXFN] PREfix misses this.
    MyFunction5(0x00000235);    // BAD. Underflows. [PFXFN] PREfix misses this.
}