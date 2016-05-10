#include <specstrings.h>
#include "undefsal.h"

struct MyStruct
{
    size_t elementCount;
};

void bar(
    _In_ char* s,
    // this next parameter caused EspX to crash, because it meant the location for actuals
    // would end up unbound (when it was defaulting to NULL), which EspX did not expect when
    // trying to calculate the byte size to safely read
    _In_reads_bytes_opt_(p->elementCount) MyStruct* p = 0 
    );

void foo(_In_ char* s)
{
    bar(s);
}
