#include "specstrings.h"
#include <memory.h>
#include "mymemory.h"

__writableTo(elementCount(count)) int *mallocInt(size_t count)
{
    return (int*)malloc(count * sizeof(int));
}

int main()
{
    int *a = mallocInt(10);
    if (a == nullptr)
        return 0;

    memset(a, 0, 10 * sizeof(int));

    int *b = a+3;
    int c1 = b[2];
    int c2 = b[7];  // BAD. Same as a[10]

    for (int j = 0; j < 5; j ++)
    {
        for (int i = 0; i <= 10; i ++)
        {
            a[i] = 1;   // BAD. Overflow (e.g., when j <= 2 && i == 10) [ESPXFN] ESPX does not report this.
            if (i + j == 12)
              goto end;
        }
    }
end:;
}
