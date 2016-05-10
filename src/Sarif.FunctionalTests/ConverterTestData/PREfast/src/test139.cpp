#include <specstrings.h>
#include "specstrings_new.h"
#include "mymemory.h"
#include "undefsal.h"

bool g_random;
int RandomCond(__in_z const char *cp)
{
    return (int)g_random;
}

_On_failure_(__out_range(==, 0))
__success(return == 1)
int TestFailOnImpl(__in_z const char *cp)
{
    if (RandomCond(cp))
        return 0;    // ok: this is correct failure
    if (RandomCond(cp))
        return 1;    // ok: this is correct success
    return 3;        // error: not success or failure. ESPX:26061. PFX can detect this. See main.
}

void TestFailOnCaller()
{
    int buf[2];

    buf[TestFailOnImpl("Hi")] = 0;  // no buffer overrun here - return is 0 or 1 for ESPX. It can overflow for PREfix. PFX:23
    buf[TestFailOnImpl("Hi") - 1] = 0;  // buffer underrun here on failure. It can also overflow for PREfix. ESPX: 26001 / PFX:24,23
}

bool g_magic;

__success(*success)
__bcount(size * 2)
_On_failure_(__bcount(size))
void *WeirdAlloc(int size, __out bool *success)
{
    bool succeeded = g_magic;
    char* buf = (char*)malloc(succeeded ? size * 2 : size);
    if (!buf)
        throw "sorry";

    *success = succeeded;

    return (void*)buf;
}

void TestWeirdAlloc()
{
    bool success;
    char *buffer = (char *)WeirdAlloc(10, &success);

    if (buffer)
    {
        if (success)
        {
            buffer[19] = 0;   // no buffer overrun
        }
        else
        {
            buffer[9] = 0;    // no buffer overrun
            buffer[10] = 0;   // yes buffer overrun. ESPX:26000 / PFX:23
        }

        free(buffer);
    }
}

void main()
{
    int buf[2];
    int rc = TestFailOnImpl("Hi"); 
    buf[rc] = 0;    // BAD. Can overflow if we got 3. OK for ESPX per contract. PFX:23
}