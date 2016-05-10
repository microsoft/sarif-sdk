#include "specstrings.h"
#include "mywin.h"
#define FACILITY_WIN32 7
#define ERROR_INSUFFICIENT_BUFFER 122L 

#define macro_MYHRESULT_FROM_WIN32(x) ((HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000))

__analysis_hint(INLINE) 
HRESULT analysis_inlined_MYHRESULT_FROM_WIN32(unsigned long x) {
 return macro_MYHRESULT_FROM_WIN32(x);
}

HRESULT func_MYHRESULT_FROM_WIN32(unsigned long x) {
 return macro_MYHRESULT_FROM_WIN32(x);
}


// no warning expected
HRESULT test1n(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    return macro_MYHRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER);
}

// warning expected
HRESULT test2p(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    return func_MYHRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER);    // BAD. ESPX does not know what the function version will return. ESPX:26030
}

// no warning expected
HRESULT test2n(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    return analysis_inlined_MYHRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER);
}

// no error if we are doing sign extension properly.
HRESULT test3n(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    unsigned long x = 0xFFFFFFFF;
    HRESULT ret = 0xFFFFFFFF + ((HRESULT)x);
    if(ret != ((HRESULT)-2))
    {
        return 0;   // WON'T REACH HERE
    }
    else
    {
        return -1;  // OK  
    }
}

// expect error if we are doing sign extension properly.
HRESULT test3p(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    unsigned long x = 0xFFFFFFFF;
    HRESULT ret = 0xFFFFFFFF + ((HRESULT)x);
    if(ret == ((HRESULT)-2))
    {
        return 0;   // BAD. Can cause overflow. ESPX 26030.
    }
    else
    {
        return -1;  // WON'T REACH HERE
    }
}

// expect no error if we are doing truncation properly.
HRESULT test4n(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    unsigned long x = 0xFFFFFFFF;
    unsigned __int8 y = x;
    if(0xFF != y)
    {
        return 0;
    }
    else
    {
        return -1;
    }
}

// expect error if we are doing truncation properly.
HRESULT test4p(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    unsigned long x = 0xFFFFFFFF;
    unsigned __int8 y = x;
    if(0xFF == y)
    {
        return 0;
    }
    else
    {
        return -1;
    }
}

// expect error if we are doing truncation and sign extension properly
HRESULT test5p(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    unsigned long x = 0xFFFFFFFF;
    __int8 y = x;
    if(y < 0)
    {
        return 0;
    }
    else
    {
        return -1;
    }
}

// expect no error if we are doing truncation and sign extension properly.
HRESULT test5n(__out_ecount_part(*pcb, *pcb) int *buf, int *pcb)
{
    (*pcb)++;
    unsigned long x = 0xFFFFFFFF;
    __int8 y = x;
    if(y >= 0)
    {
        return 0;
    }
    else
    {
        return -1;
    }
}

void main()
{
    int buf[100];
    int size;
    int val;

    HRESULT hr;

    size = 100;
    hr = test1n(buf, &size);
    if (hr >= 0)
        val = buf[size - 1];    // OK
    buf[size - 1] = 1;    // BAD. Overflows.

    size = 100;
    hr = test2p(buf, &size);
    if (hr >= 0)
        val = buf[size - 1];    // OK
    buf[size - 1] = 1;    // BAD. Overflows.

    size = 100;
    hr = test2n(buf, &size);
    if (hr >= 0)
        val = buf[size - 1];    // OK
    buf[size - 1] = 1;    // BAD. Overflows.

    size = 100;
    hr = test3n(buf, &size);
    if (hr >= 0)    // SUCCEEDED
        val = buf[size - 1];    // WON'T REACH HERE

    size = 100;
    hr = test3p(buf, &size);
    if (hr >= 0)    // SUCCEEDED
        val = buf[size - 1];    // BAD. Overflows. test3p is buggy.

    size = 100;
    hr = test4n(buf, &size);
    if (hr >= 0)    // SUCCEEDED
        val = buf[size - 1];    // WON'T REACH HERE

    size = 100;
    hr = test4p(buf, &size);
    if (hr >= 0)    // SUCCEEDED
        val = buf[size - 1];    // BAD. Overflows. test4p is buggy.

    size = 100;
    hr = test5n(buf, &size);
    if (hr >= 0)    // SUCCEEDED
        val = buf[size - 1];    // WON'T REACH HERE

    size = 100;
    hr = test5p(buf, &size);
    if (hr >= 0)    // SUCCEEDED
        val = buf[size - 1];    // BAD. Overflows. test5p is buggy.
}
