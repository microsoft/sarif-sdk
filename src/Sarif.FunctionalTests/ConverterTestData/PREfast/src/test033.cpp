#include "specstrings.h"
#include "mymemory.h"

char a[8];
char f(unsigned int i)
{
	return a[i % 8];
}

char g(unsigned int i)
{
	unsigned int j = i & 0x7;
	return a[j];
}

char h(unsigned int i)
{
	i %= 8;
	return a[i];
	
}

char h1(unsigned int i)
{
	i &= 0x7;
	return a[i];
	
}

void ZeroMemory(__out_ecount_full(n) unsigned char *buf, int n)
{
    memset(buf, 0, n);
}

void Read(__in_bcount(n) unsigned char *bug, int n)
{
    unsigned char c = 'a';
    for (int i = 0; i < n; ++i)
        c |= bug[i];
}

#define CCHMAXBUF 128
unsigned char rgch[CCHMAXBUF];
void foo(int i, int iX)
{
    int iMaxBuf = CCHMAXBUF - CCHMAXBUF % i;    // 0 <= CCHMAXBUF % i <= CCHMAXBUF
    ZeroMemory(rgch, iX > CCHMAXBUF ? iMaxBuf : iX); // NOTE: 0 <= param2 <= CCHMAXBUF (iX < 0 causes access violation in memset)
    int iTmp = iX > iMaxBuf ? iMaxBuf : iX;
    Read(rgch, iTmp);       // [PFXFP] PREfix warns this will cause overrun
}

void bar(int i, int iX)
{
    int iMaxBuf = CCHMAXBUF;
    iMaxBuf %= i;   // 0 <= iMaxBuf <= CCHMAXBUF
    iMaxBuf = CCHMAXBUF - iMaxBuf;
    ZeroMemory(rgch, iX > CCHMAXBUF ? iMaxBuf : iX);
    int iTmp = iX > iMaxBuf ? iMaxBuf : iX;
    Read(rgch, iTmp);       // [PFXFP] PREfix warns this will cause overrun
}

// Test round up logic.
#define ROUNDUP(size, f)    ((size + f - 1) & ~(f-1))

void f(__out_ecount(size) char *buf, size_t size)
{
    size_t rsize = ROUNDUP(size, 8);
    if (rsize > 100)
        return;

    // 100 >= rsize >= size
    char arr[100];
    memcpy(arr, buf, size);
}

void main()
{
    char a[96] = {};
    f(a, 96);   // ROUNDUP to 96

    char b[97] = {};
    f(b, 97);   // ROUNDUP to 104

    char c[105] = {};
    f(c, 105);   // ROUNDUP to 112: [PFXFP] PREfix thinks this accesses arr from offset 104.
}