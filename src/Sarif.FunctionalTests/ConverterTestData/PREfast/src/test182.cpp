#include <specstrings.h>

_Ret_writes_(n) char* Alloc(_In_ size_t n);

#include "undefsal.h"

void certain_overflow(_In_ size_t n)
{
    char* p = Alloc(n);
    
    // this null termination caused the line that follows it
    // to be reported as "may" rather than "must" overflow
    // (tracked as Esp:1071)
    p[n-1] = 0; 
    
    // should be reported as "must" overflow
    p[n] = 0;
}
