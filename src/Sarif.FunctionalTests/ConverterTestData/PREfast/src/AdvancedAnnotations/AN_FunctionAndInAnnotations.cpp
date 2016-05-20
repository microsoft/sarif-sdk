
#include <sal.h>

void UseValues(_In_ int * input);

void CallUseValues(bool flag)
{
    int source;
    if (flag)
        UseValues(&source);
}
