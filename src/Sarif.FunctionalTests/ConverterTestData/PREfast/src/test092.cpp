#include "specstrings.h"

//ESP bug #569

//  Tests on 64 bit arithmetic.  ESP#569 was due to truncating a 64 bit value
//  to 32 bits before division, leading to divide-by-zero.  The 'K' tests are
//  checkable with 32 bit arithmetic.  The 'T' tests have a loop stride that
//  has zero in the low 32 bits to detect truncation.  'X' has non-zero low
//  bits.  For the 'T' and 'X' tests to pass the loop analysis needs to use 64
//  bit values and the solver needs to guard against integer overflow during
//  computations.

const __int64 K = 1<<10;
const __int64 T = 1ui64 << 40;
const __int64 X = (1ui64 << 40)+256;


void test1t()
{
    int a[16];

    for (__int64 i = 0;  i < 16*T;  i += T)
    {
        a[i/T] = 1;
    }
}


void test1k()
{
    int a[16];

    for (__int64 i = 0;  i < 16*K;  i += K)
    {
        a[i/K] = 1;
    }
}


void test1x()
{
    int a[16];

    for (__int64 i = 0;  i < 16*X;  i += X)
    {
        a[i/X] = 1;
    }
}


void test2t()
{
    int a[15];

    for (__int64 i = 0;  i < 16*T;  i += T)
    {
        __int64 x = i>>40;
        a[x] = 1;          // 26014
    }
    a[-9]=1;               // 26001 to detect analysis understands loop
                           // termination (no truncation of increment)
}


void test2k()
{
    int a[15];

    for (__int64 i = 0;  i < 16*K;  i += K)
    {
        __int64 x = i>>10;
        a[x] = 1;          // 26014
    }
    a[-9]=1;               // 26001
}


void test2x()
{
    int a[15];

    for (__int64 i = 0;  i < 16*X;  i += X)
    {
        a[i/X] = 1;          // 26014
    }
    a[-9]=1;
}

void test2tu()
{
    int a[15];

    for (unsigned __int64 i = 0;  i < 16*T;  i += T)
    {
        unsigned __int64 x = i>>40;
        a[x] = 1;          // 26014
    }
    a[-9]=1;               // 26001 to detect analysis understands loop
                           // termination (no truncation of increment)
}


void test2ku()
{
    int a[15];

    for (unsigned __int64 i = 0;  i < 16*K;  i += K)
    {
        __int64 x = i>>10;
        a[x] = 1;          // 26014
    }
    a[-9]=1;               // 26001
}


void test2xu()
{
    int a[15];

    for (unsigned __int64 i = 0;  i < 16*X;  i += X)
    {
        a[i/X] = 1;          // 26014
    }
    a[-9]=1;                 // 26001
}

void main() { /* Dummy */ }