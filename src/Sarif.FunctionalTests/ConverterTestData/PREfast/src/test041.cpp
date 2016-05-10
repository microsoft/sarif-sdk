#include "specstrings.h"
#include "undefsal.h"

void f(__ecount(n + 1) int *a, int n)
{
    a[n] += 1;
}

void g(__ecount(n >> 1) int *b, int n)
{
    b[(n >> 1) - 1] += 1;
}

void h(__in __out int *p);

struct S {
    int size;
    char a[1];
};

void foo(__in_ecount(sizeof(S1)) void *p);

void bar(__in_ecount(p->size) S *p);

#define INTERNET_MAX_URL_LENGTH (32 + sizeof("://") + 2048)
void baz(__out_ecount(INTERNET_MAX_URL_LENGTH) char *p)
{
    p[INTERNET_MAX_URL_LENGTH] = 1;     // BAD. Overflow.
}

void
GetDeviceDefaultPrintTicketThunk(
    __in                                int hProvider,
    __out_bcount(pTicketLength)         char **ppTicketData,
    __out                               int   *pTicketLength
    );

// TODO: We do not report errors for method declarations currently
class C {
    int a;
public:
    virtual C f1(__out_bcount(n) char*buf, int *n);
    void f(__out_bcount(*n) char*buf, int *n);
    C();
};

// Tests for references
void ref1(__out_ecount(n) char *p, int &n)
{
    p[n] = 1;   // BAD. Overflow.    
}

void ref2(__out_ecount(n+1) char *p, int &n)
{
    p[n+1] = 1; // BAD. Overflow.    
}

void ref3(_Outref_result_buffer_(n) int *&buff, int &n)
{
    buff = new int[n];        
}

void ref4(_Outref_result_buffer_(n) int *&buff, int &n)
{
    buff = new int[n];        
}

void ref5(_Outref_result_buffer_(*n) int *&buff, int *&n)
{
    buff = new int[*n-1];        
}

typedef __bcount(size) void * (* FCall)(__out_ecount(size) int *buf, int size);

void g(__in_ecount(1) FCall pfn) // Should not complain about this
{
    int a[10];
    pfn(a, 11); // BAD. Overflow per line 72.
}

void foo1(__deref_inout_ecount(*size) char *buf, int *size);

void foo2(__in int a); // OK

// Using SAL keywords as parameter names - OK
void bar(__out_ecount(stringLength) char *buf, size_t stringLength)
{
}

void bar1(__out_bcount(byteCount) char *buf, size_t byteCount)
{
}

// Using return in preconditions
int bar2(__out_ecount(return) char *buf);

// Ensure that we process annotations on functions with return UDT correctly
C f(__in_ecount(n) int *p, int n, int *n1);

// Function typedefs
typedef void DRIVER_FUNCTION(__out_ecount(n) int *p, int n);

DRIVER_FUNCTION driverFn;
DRIVER_FUNCTION driverFn1;

void callDriver()
{
    int a[10];
    driverFn(a, 11);    // BAD. Overflow per line 100.
    driverFn1(a, 10);   // OK for ESPX per annotations. BAD for PREfix - Overflow. 
}

void driverFn(int *p, int n)
{
    p[n-1] = 1; // OK
}

void driverFn1(int *p, int n)
{
    p[n] = 1;   // BAD for ESPX. PREfix do not consider this as a bug until it sees a faulty caller.
}

struct S1 {
    int c;
    __field_ecount(c) char *p;
};

void useStruct(S1 *p)
{
}

//Test case from adsmith
#define MAX_ALLOCATION_NUMBER 100
typedef unsigned int UINT;

struct CStruct
{
    __range(<=, MAX_ALLOCATION_NUMBER) UINT Count;
};

class CClass
{
    void StartBuffer(
        __deref_out_ecount(*puRemaining) int **ppData,
        __deref_out_range(==, (this->CurrentRemaining)) UINT *puRemaining
        );
    void EndBuffer(
        __in_ecount(remaining) int *pData,
        __range(0, (this->CurrentBuffer->Count)) UINT remaining
        );

private:
    __range(<=, CurrentBuffer->Count) UINT CurrentRemaining;
    CStruct *CurrentBuffer;
};

// ESP bug 384: Large constants do not work correctly in annotations
typedef unsigned long DWORD;
#define ERR 0xFFFFFFFF

DWORD foo(void) { return 10; }

__success(return != ERR)
DWORD test(DWORD size, __out_bcount_part_opt(size, return) void * buffer)
{
    DWORD dw = foo();
    if (dw > size)
        return ERR;  // Warning 26045
    return dw;
}

// Test for forward declaration of struct
struct SS;
void TestMismatch(__in SS *s);

struct SS {
    __field_ecount(n) char *p;
    size_t n;
};

void TestMismatch(__in SS *s)
{
}

__bcount(size) void *pf(__out_ecount(size) int *p, int size)
{
    p[size - 1] = 1;   // OK.
    return (void*)(new char[size]);
}

void main()
{
    int len;
    char url[INTERNET_MAX_URL_LENGTH];    
    baz(url);   // BAD. Overflow.

    len = INTERNET_MAX_URL_LENGTH;
    ref1(url, len); // BAD. Overflow.

    len = INTERNET_MAX_URL_LENGTH - 1;
    ref2(url, len); // BAD. Overflow.

    int *buf;
    len = 30;
    ref3(buf, len);
    if (buf != nullptr)
    {
        buf[len - 1] = 1;   // OK
        buf[len] = 1;       // BAD. Overflow.
    }

    len = 40;
    ref4(buf, len);
    if (buf != nullptr)
    {
        buf[len - 1] = 1;   // OK
        buf[len] = 1;       // BAD. Overflow.
    }

    len = 50;
    int *plen = &len;
    ref5(buf, plen);
    if (buf != nullptr)
    {
        buf[*plen - 1] = 1;   // OK for ESPX per annotations. BAD for PREfix. Overflow.
    }

    g(&pf);     // BAD. Should cause overflow in pf. [PFXFN] PREfix uses default model for line 77 and misses this.
}


