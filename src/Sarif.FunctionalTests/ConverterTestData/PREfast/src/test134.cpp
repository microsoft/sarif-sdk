#include <specstrings.h>

typedef __success(return >= 0) long HRESULT;

extern "C"
__range(0, 9) int GetNumber(
    HRESULT hr
    )
{
    static int i;
    return i % 10;
}

extern "C"
HRESULT GetNumber2(
    __deref_out_range(0, 9) int *pInt
    )
{
    static int i;
    int r = *pInt = i % 20 - 5;
    if (0 <= r && r <= 9)
        return 0;
    else
        return -1;
}

void SafeTestStatusAsParam(HRESULT hr)  // Q: Do we need this param?
{
    int x[10];

    x[GetNumber(hr)] = 0;   // this is safe
}

void UnsafeTestStatus(HRESULT hr)       // Q: Do we need this param?
{
    int x[10];
    int index;

    GetNumber2(&index);

    x[index] = 0;    // this is unsafe and can overrun / underrun. ESPX:26015,26011 / PFX:23,24
}

void main() { /* dummy */}

