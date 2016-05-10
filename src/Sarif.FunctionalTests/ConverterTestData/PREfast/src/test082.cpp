#include "mywin.h"
#include "specstrings.h"

extern OLECHAR Base64ToChar[];
extern OLECHAR Pad64;

static DWORD EncodeBinary(__out_ecount((cbSrc+2)/3*4) LPOLESTR pchDest, __in_ecount(cbSrc) BYTE *pbSrc, DWORD cbSrc)
{
    OLECHAR *pchOut = pchDest;
    BYTE *pbIn = pbSrc;
    DWORD cChunks = cbSrc / 3; // count of 24-bit chunks
    DWORD cbRem = cbSrc % 3; // count of extra bytes
    DWORD cbRem2 = (cbSrc + 2) % 3; 

    // Encode each 6-bit octet as a printable character and convert to Unicode
    for (int i = 0; i < (int)cChunks; i++)
    {
        *pchOut++ = (OLECHAR)(Base64ToChar[pbIn[0] >> 2]);
        *pchOut++ = (OLECHAR)(Base64ToChar[((pbIn[0] << 4) & 0x30) | pbIn[1] >> 4]);
        *pchOut++ = (OLECHAR)(Base64ToChar[((pbIn[1] << 2) & 0x3c) | pbIn[2] >> 6]);
        *pchOut++ = (OLECHAR)(Base64ToChar[pbIn[2] & 0x3f]);
        pbIn += 3;
    }

    // Add padding if needed
    if (cbRem == 1)
    {
        *pchOut++ = (OLECHAR)(Base64ToChar[pbIn[0] >> 2]);
        *pchOut++ = (OLECHAR)(Base64ToChar[(pbIn[0] << 4) & 0x30]);
        *pchOut++ = (OLECHAR)Pad64;
        *pchOut++ = (OLECHAR)Pad64;
    }
    else if (cbRem == 2)
    {
        *pchOut++ = (OLECHAR)(Base64ToChar[pbIn[0] >> 2]);
        *pchOut++ = (OLECHAR)(Base64ToChar[((pbIn[0] << 4) & 0x30) | pbIn[1] >> 4]);
        *pchOut++ = (OLECHAR)(Base64ToChar[((pbIn[1] << 2) & 0x3c)]);
        *pchOut++ = (OLECHAR)Pad64;
    }
    return (DWORD)((BYTE *)pchOut - (BYTE *)pchDest);
}

static DWORD EncodeBinaryBad(__out_ecount((cbSrc+2)/3*4) LPOLESTR pchDest, __in_ecount(cbSrc) BYTE *pbSrc, DWORD cbSrc)
{
    OLECHAR *pchOut = pchDest;
    BYTE *pbIn = pbSrc;
    DWORD cChunks = cbSrc / 3; // count of 24-bit chunks
    DWORD cbRem = cbSrc % 3; // count of extra bytes
        DWORD cbRem2 = (cbSrc + 2) % 3; 

    // Encode each 6-bit octet as a printable character and convert to Unicode
    for (int i = 0; i < (int)cChunks; i++)
    {
        *pchOut++ = (OLECHAR)(Base64ToChar[pbIn[0] >> 2]);
        *pchOut++ = (OLECHAR)(Base64ToChar[((pbIn[0] << 4) & 0x30) | pbIn[1] >> 4]);
        *pchOut++ = (OLECHAR)(Base64ToChar[((pbIn[1] << 2) & 0x3c) | pbIn[2] >> 6]);
        *pchOut++ = (OLECHAR)(Base64ToChar[pbIn[2] & 0x3f]);
        *pchOut++ = 1;  //Bad. It will cause overflow of pchDest.
        pbIn += 3;
    }

    // Add padding if needed
    if (cbRem == 0) //Bad
    {
        *pchOut++ = (OLECHAR)(Base64ToChar[pbIn[0] >> 2]);
        *pchOut++ = (OLECHAR)(Base64ToChar[(pbIn[0] << 4) & 0x30]);
        *pchOut++ = (OLECHAR)Pad64;
        *pchOut++ = (OLECHAR)Pad64;
    }
    return (DWORD)((BYTE *)pchOut - (BYTE *)pchDest);
}

OLECHAR Base64ToChar[64];
OLECHAR Pad64;

void main()
{
    for (int i = 0; i < 64; ++i)
        Base64ToChar[i] = 'A' + i;  // Let's not worry about the details.
    Pad64 = '=';                    // Let's not worry about the details.

    const DWORD len = 100;
    BYTE src[len];
    for (int i = 0; i < len; ++i)
        src[i] = i;

    OLECHAR dst[(len + 2)/3*4];
    EncodeBinary(dst, src, len);
    EncodeBinaryBad(dst, src, len); // BAD. Causes overflow. [PFXFN] PREfix does not report overflow.
}