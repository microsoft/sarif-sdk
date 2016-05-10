#include "specstrings.h"

#define ARRAYSIZE(a)   (sizeof(a)/sizeof(a[0]))
#define BITS_INA_BYTE 8

/* Test of understanding of nested loops. All version are safe (although f and
 * g may be less efficient if destLength<portsLength*8).
 */


void orig(__in_bcount(portsLength) char* ports, size_t portsLength,
          __out_bcount(destLength) char* dest, size_t destLength)
{
    
    if (destLength > portsLength * BITS_INA_BYTE)
      destLength = portsLength * BITS_INA_BYTE;

    char* curSrc = ports;

    char* curDest = dest;
    char* endDest = dest+destLength;

    for (size_t mask = 1; curDest != endDest; curDest++) {
        *curDest = (*curSrc & mask) ? 1 : 0;               // BAD (potential)
        if (mask & 0x80) {
            mask = 0x1;
            curSrc++;
        }
        else
          mask <<= 1;
    }
}

void orig_mod(__in_bcount(portsLength) char* ports, size_t portsLength,
              __out_bcount(destLength) char* dest, size_t destLength)
{
    
    if (destLength > portsLength * BITS_INA_BYTE)
      destLength = portsLength * BITS_INA_BYTE;

    char* curSrc = ports;

    char* curDest = dest;
    char* endDest = dest+destLength;

    for (size_t mask = 1; curDest < endDest; curDest++) {
        *curDest = (*curSrc & mask) ? 1 : 0;               // BAD (potential)
        if (mask & 0x80) {
            mask = 0x1;
            curSrc++;
        }
        else
          mask <<= 1;
    }
}

void bestway(__in_bcount(portsLength) char* ports, size_t portsLength,
             __out_bcount(destLength) char* dest, size_t destLength)
{
    
    if (destLength > portsLength * BITS_INA_BYTE)
      destLength = portsLength * BITS_INA_BYTE;

    char* curSrc = ports;

    char* curDest = dest;
    char* endDest = dest+destLength;

    for (int i = 0; i<portsLength && curDest<endDest; i++, curSrc++)
    {
        size_t mask = 0x01;
        for (int j = 0; j<BITS_INA_BYTE && curDest<endDest; j++, mask <<= 1, curDest++)
        {
            *curDest = (*curSrc & mask) ? 1 : 0;             // OK
        }        
    }
}

void f(__in_bcount(portsLength) char* ports, size_t portsLength,
       __out_bcount(destLength) char* dest, size_t destLength)
{
    
    if (destLength > portsLength * BITS_INA_BYTE)
      destLength = portsLength * BITS_INA_BYTE;

    char* curSrc = ports;

    char* curDest = dest;
    char* endDest = dest+destLength;

    for (int i = 0; i<portsLength /*&& curDest!=endDest */; i++, curSrc++)
    {
        size_t mask = 0x01;
        for (int j = 0; j<BITS_INA_BYTE && curDest<endDest; j++, mask <<= 1, curDest++)
        {
            *curDest = (*curSrc & mask) ? 1 : 0;             // OK
        }        
    }
}


void g(__in_bcount(portsLength) char* ports, size_t portsLength,
       __out_bcount(destLength) char* dest, size_t destLength)
{
    
    if (destLength > portsLength * BITS_INA_BYTE)
      destLength = portsLength * BITS_INA_BYTE;

    char* curSrc = ports;

    char* curDest = dest;
    char* endDest = dest+destLength;

    for (int i = 0; i<portsLength /*&& curDest!=endDest */; i++, curSrc++)
    {
        size_t mask = 0x01;
        for (int j = 0; j<BITS_INA_BYTE && curDest!=endDest; j++, mask <<= 1, curDest++)
        {
            *curDest = (*curSrc & mask) ? 1 : 0;              // BAD
        }        
    }
}


void h(__in_bcount(portsLength) char* ports, size_t portsLength,
       __out_bcount(destLength) char* dest, size_t destLength)
{
    
    if (destLength > portsLength * BITS_INA_BYTE)
      destLength = portsLength * BITS_INA_BYTE;

    char* curSrc = ports;

    char* curDest = dest;
    char* endDest = dest+destLength;

    for (int i = 0; i<portsLength && curDest!=endDest; i++, curSrc++)
    {
        size_t mask = 0x01;
        for (int j = 0; j<BITS_INA_BYTE && curDest!=endDest; j++, mask <<= 1, curDest++)
        {
            *curDest = (*curSrc & mask) ? 1 : 0;               // BAD (potential)
        }        
    }
}




