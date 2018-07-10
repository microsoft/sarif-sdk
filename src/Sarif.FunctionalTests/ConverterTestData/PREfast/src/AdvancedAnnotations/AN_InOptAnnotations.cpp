
#include <sal.h>

void DereferenceParameter(_In_opt_ int * source, bool flag)
{
    if (flag)
        int result = *source;
}

