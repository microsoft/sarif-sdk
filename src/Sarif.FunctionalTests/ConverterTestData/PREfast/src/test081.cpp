#include "specstrings.h"
#include "mywin.h"
#include "mymemory.h"

//Test case from johnham

//typedef long HRESULT;
#define SUCCEEDED(hr)    ((hr) >= 0)

HRESULT Emit(const char* s)
{ 
    // What we do here is not critical for our testing.
    return S_OK; 
}

const char* s_apwszOrdinalTens[10];
const char *s_apwszOrdinalDigits[20]; 

HRESULT EmitOrdinal(unsigned u)
{
    HRESULT hr = S_OK;

    if (u >= 1000)
    {
        return E_FAIL;
    }

    if (u >= 100)
    {
        unsigned u100 = u / 100;

        hr = Emit(s_apwszOrdinalDigits[u100]);  // OK

        if (SUCCEEDED(hr))
        {
            hr = Emit("hundred");
        }

        u -= u100 * 100;

        if (SUCCEEDED(hr) && u > 0)
        {
            hr = Emit("and");
        }
    }

    if (SUCCEEDED(hr) && u >= 20)
    {
        unsigned u10 = u / 10;
    hr = Emit(s_apwszOrdinalTens[u10]);         // OK
    u -= u10 * 10;
    }

    if (SUCCEEDED(hr) && u)
    {
        hr = Emit(s_apwszOrdinalDigits[u]);     // OK
    }

    return hr;
} 


void f(unsigned x, unsigned y)
{
    char a[10];
    if (10 * x <= y && y < 100)
    a[19 - x] = 1;      // BAD. Overflow. E.g., when x == 0 && y == 99
}


//Enable doubling in constraint solver. Needed to verify following code.
__success(return != 0)
    bool SafeAdd(unsigned long a, unsigned long b, __deref_out_range(==, a + b) unsigned long *result)
{
    if (result == nullptr)
        return false;

    *result = a + b;    // Not safe, but not critical for our testing...
    return true;
}

__success(return != 0)
bool SafeMult(unsigned long a, unsigned long b, __deref_out_range(==, a * b) unsigned long *result)
{
    if (result == nullptr)
        return false;

    *result = a * b;    // Not safe, but not critical for our testing...
    return true;
}

struct S {
    char *p;
};

void Fill(S *s)
{
    const char* msg = "Hellp, there";
    int len = strlen(msg);

    // What we do here is not critical for our testing.
    if (s == nullptr)
        return;

    s->p = (char*)malloc(len + 1);
    if (s->p == nullptr)
        return;

    memcpy(s->p, msg, len + 1);
}

void TestSafeInt(unsigned int n1, unsigned int n2)
{
    S glob;
    int *buf;
    unsigned long result1, result2, result3 = 0;
    Fill(&glob);

    if (glob.p)
    {
        if (SafeAdd(n1, n1, &result1) && SafeAdd(result1, strlen(glob.p), &result2) && SafeMult(result2, sizeof(int), &result3))
        {
            buf = (int *)malloc(result3);
            if (buf != nullptr)
            {
                memset(buf +  2 * n1, 0, strlen(glob.p));
            }
        }

        free(glob.p);
        glob.p = nullptr;

        if (buf != nullptr)
            free(buf);
    }
}

//
//  Limited range on 16-bit types
//

int ra[65536];

void R16_a(unsigned short i)
{
    ra[i] = 9;  // OK. 0 <= i <= 65535
}

int a66[66];
void R16_b(unsigned short i)
{
    a66[i/993] = 9;  // OK. 0 <= i/993 <= 65
}

void R16_c(unsigned short i)
{
    a66[i/992] = 9;  // BAD. Overflow. 0 <= i/992 <= 66. 
}

void R16_d(short i)
{
    ra[32768+i] = 9;  // OK. 0 <= 32768+i <= 65535
}

void R16_e(short i)
{
    a66[(32768+i)/993] = 9;  // OK. See above.
}

void R16_f(short i)
{
    a66[(32768+i)/992] = 9;  // BAD. Overflow. See above.
}

void main()
{
    EmitOrdinal(999);
    f(0, 99);   // BAD. Overflow. [PFXFN] Likely due to multi-param indexing.
    TestSafeInt(10, 0); // OK. NOTE: Second param is not used.
    R16_a(65535);   // OK
    R16_b(65535);   // OK
    R16_c(65535);   // BAD. Overflow.
    R16_d(-32768);  // OK
    R16_d(32767);   // OK
    R16_e(-32768);  // OK
    R16_e(32767);   // OK
    R16_f(-32768);  // OK
    R16_f(32767);   // BAD.
}