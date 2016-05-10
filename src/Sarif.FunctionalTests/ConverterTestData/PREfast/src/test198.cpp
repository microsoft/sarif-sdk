#include <specstrings.h>
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

//
// Test case for Esp:345
// EXPECT NO WARNINGS
// 
void f345(__readableTo(elementCount(5)) int *a);
void g345()
{
    char a[10];
    a[0] = 0;
    f345(0); // was giving spurious uninit pointer message.
}

//
// Test case for Esp:353
// EXPECT NO WARNINGS
// 
typedef void* HKEY;
#define HKEY_CLASSES_ROOT                   ((HKEY)(ULONG)((LONG)0x80000000)) 
void g353(_In_ HKEY hkey);
void h353()
{
	HKEY hkey;
	g353(HKEY_CLASSES_ROOT);
}

HRESULT test485(__deref_out LPWSTR* pValue) // fail, why?
{
    WCHAR *buffer = (WCHAR *)malloc(2);
    if (buffer[0] != L'\0') return -1;
    *pValue = (LPWSTR)buffer;
    return S_OK;
}

int aa578[3], bb578[10];
int g578();
void f578()
{
    unsigned long index;

    for (index = 0;  index < 3;  index++)
    {
        aa578[index] = g578();
        if (aa578[index] != 90)
        {
            while (index--) {
                bb578[-99] = 1;           // Should report underflow, previously did not
                aa578[index] = 1;
            }
            return;
        }
    }
    bb578[99] = 1; // should report overflow (26000)
}

typedef _Null_terminated_ WCHAR *myPWSTR;
typedef _Null_terminated_ myPWSTR *myPZPWSTR;
#define MAXDWORD 0xffffffff  

DWORD f609a(/* _In_ myPZPWSTR prgpFoo */ const myPZPWSTR prgpFoo)
{
    DWORD i = MAXDWORD;

    for (i = 0; NULL != prgpFoo[i]; i++) // should be no warning
        ;

    for (i = 0; NULL != prgpFoo[i]; i+=2) // should warn
        ;
    return i;
}

DWORD f609b(_In_ myPZPWSTR prgpFoo)
{
    DWORD i = MAXDWORD;

    for (i = 0; NULL != *prgpFoo; i++) // should be no warning
        prgpFoo++;

    return i;
}

DWORD f609c(_In_ myPZPWSTR prgpFoo)
{
    DWORD i = 0;

    if (NULL != prgpFoo[0]) // should be no warning
    {
        i++;
    }

    if (NULL != *prgpFoo) // should be no warning
    {
        i++;
    }

    return i;
}

DWORD f609d(_In_ myPZPWSTR prgpFoo)
{
    DWORD i = 0;

    if (NULL != prgpFoo[0]) // should be no warning
    {
        while (NULL != prgpFoo[i]) // should be no warning
            i++;
    }
    return i;
}

DWORD f609e(_In_ myPZPWSTR prgpFoo)
{
    DWORD i = 0;

    if (NULL != prgpFoo[0]) // should be no warning
    {
        i++;
        if (NULL != prgpFoo[1]) // should be no warning
        {
            i++;
        }
    }
    return i;
}

const unsigned char rg622[32] = {8,7,6,5,4,3,2,1, 13,12,11,10, 18,17,16,15,21,20, 23,22, 26,25, 28,27, 30,29, 32,31, 34,33, 36,35}; 

void f622()
{
    const unsigned char* pch = rg622;
    
    char x[10];
    size_t count = 0;
    while(count < sizeof(rg622) && *pch < 10 && *pch >= 0)
    {
        x[*pch++] = 'A';
        count++;
    }
}

typedef unsigned long UDWORD;
typedef unsigned short SQLTCHAR;

struct A
{
    UDWORD      len;
    SQLTCHAR*   data;
};

struct B
{
    UDWORD      len;
    SQLTCHAR*   data;
};

typedef struct TDSCOLFMT
{
    char    Type;
    char    UserType;
    A       ShilohTextType;
    B       TextImageType;
    USHORT  ShilohTextLen;
    USHORT  TextImageTextLen;
} *LPTDSCOLFMT;

#define LONG_MAX      2147483647L   /* maximum (signed) long value */

// should give no postcondition violation warning as the datatypes
// ensure overflow of the output value cannot occur
void f618(
    UINT coltype,
    LPTDSCOLFMT lpb,
    _Deref_out_range_(0, LONG_MAX)   
    UDWORD       *pcbUsed,
    BOOL        fCalledByGetColFormat)
{
    UDWORD   db = sizeof(lpb->UserType) + sizeof(lpb->Type) + sizeof(USHORT);

    *pcbUsed = 0;

    switch (coltype)
    {
 case 13:
      {
        UDWORD      cb;

        if (!fCalledByGetColFormat)
        {
            if (coltype!=12)
                cb = lpb->ShilohTextLen*sizeof(SQLTCHAR);
            else
                cb = lpb->TextImageTextLen*sizeof(SQLTCHAR);
        }
        else
            cb = lpb->TextImageTextLen;

        if (coltype!=12)
            db += cb + sizeof(lpb->ShilohTextType);
        else
            db += cb + sizeof(lpb->TextImageType);					

	break;
   } 
}  
char a[1]; // force EspX to look at the function
a[0] = 9;

 *pcbUsed = db;
}

typedef void* PVOID;

PVOID Pool663[] = {
    NULL,
    NULL,
    NULL,
};

PVOID Create663();
void Destroy663(PVOID p);

// This function used to cause a spurious 26001 to be issued
void f663()
{
    ULONG Index;
    for (Index = 0; Index < 3; Index++) {
        Pool663[Index] = Create663();

        if (Pool663[Index] == NULL) {
            while (Index--) {
                Destroy663(Pool663[Index]);
            }
            return;
        }
    }
}


//
// The following are still failing
//
#if 0
enum MyEnum{ one, two, three, four,  max = four };
extern int arr[max];
int f383(MyEnum e )
{
    return arr[e];
}
#endif
