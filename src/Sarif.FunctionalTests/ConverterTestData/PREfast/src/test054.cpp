#include "specstrings.h"
#include "mymemory.h"
#include "undefsal.h"

int g[10];

void f(int i)
{
    g[i] = 1;
}

void f1(_In_range_(0, 9) int i)
{
    g[i] = 1;
}

__out_range(0, n) int find(__in_ecount(n) int *a, int i, int n)
{
    int val = a[n-1];

    static int magic;
    switch(magic % 3)
    {
    case 0:
        return n;   // It complies with the annotation.
    case 1:
        return magic % n;
    default:
        return -1;  // Post-condition violation
    }
}

__success(return >= 0)
__out_range(0, n) int find1(__in_ecount(n) int *a, int i, int n)
{
    int val = a[n-1];

    static int magic;
    switch(magic % 3)
    {
    case 0:
        return n;   // It complies with the annotation.
    case 1:
        return magic % n;
    default:
        return -1;  // OK. We failed.
    }
}

void h(__in_ecount(n) int *a, int n)
{
    int i = find1(a, 99, n);
    a[i] = 1;   // BAD: It can over/underflow
}

void h1(__in_ecount(n) int *a, int n)
{
    int i = find1(a, 99, n);
    if (i >= 0)
        a[i] = 1;   // BAD: It can still overflow
}

void search(__in_ecount(n) int *a, const int &n, __deref_out_range(0, n-1) int *index)
{
    *index = n; // Postcondition violation.
}

void search1(__in_ecount(n) int *a, const int &n, __deref_out_range(0, n-1) int *index)
{
    *index = n-1;
}

void foo(__in_ecount(n) int *a, int &n)
{
    int index;
    search(a, n, &index);
    a[index] = 1;    // Safe from ESPX's view. Bad from PREfix's view.
}

void foo1(__in_ecount(n) int *a, int &n)
{
    int index;
    search1(a, n, &index);
    a[index] = 1;    // OK for both ESPX and PREfix
}

void get(__in_ecount(n) int *a, int n, _In_range_(0, n-1) int m)
{
    if (a == nullptr || n <= 0)
        return;
    
    int x;
    for (int i = 0; i < n; ++i)
        x = a[i];

    x += a[m];  // OK: We assume 0 <= m <= n-1
}

void baz()
{
    int a[10] = {1,2,3,4,5,6,7,8,9,10};
    get(a, 10, 10); // Precondition violation. PREfix reports as overrun.
}


//Validation function
__success(return != 0)
bool Validate(__out_range(0, size - 1) int index, int size)
{
    return index >= 0 && index <= size; // Error in validation
}

void Access(__out_ecount(n) char *buf, int n, int i)
{
    if (Validate(i, n))
        buf[i] = 0; // ESPX feels OK, but PREfix doesn't.
}

//Validation function example from robinsp
void MoveMemory1(__out_bcount(size) void *dest, __in_bcount(size) void *src, size_t size)
{
    memmove(dest, src, size);
}

typedef unsigned short BOOL;
#define TRUE 1
#define FALSE 0

__success(return != 0) BOOL IsWriteRangeInBuffer
    (
    unsigned char* pStartOfBuffer, 
    unsigned char* pCurrentBufferPos, 
    int nBufferSizeInBytes, 
    __out_range(pStartOfBuffer - pCurrentBufferPos, lEndOfRange) long lStartOfRange, // Note that this value can be negative
    __out_range(lStartOfRange, (pStartOfBuffer + nBufferSizeInBytes) - pCurrentBufferPos) long lEndOfRange
    )
{
    unsigned char* pEndOfBuffer;
    unsigned char* pStartOfRange;
    unsigned char* pEndOfRange;

    if (0 == nBufferSizeInBytes)
    {
        return FALSE;
    }

    pEndOfBuffer = pStartOfBuffer + (nBufferSizeInBytes - 1);
    pStartOfRange = pCurrentBufferPos + lStartOfRange;
    pEndOfRange = pCurrentBufferPos + lEndOfRange;

    return (pStartOfBuffer <= pStartOfRange) && 
           (pStartOfRange < pEndOfRange) &&
           (pEndOfRange <= pEndOfBuffer);
}

void ButtHeadCopyBits (
  __in_bcount(lSourceLengthInBytes) unsigned char * pStartOfSourceBuffer,
  long lSourceLengthInBytes,
  long srcYStep,
  __out_bcount(lDestinationLengthInBytes) unsigned char * pStartOfDestinationBuffer,
  long lDestinationLengthInBytes,
  long dstYStep,
  unsigned short width,
  unsigned short height
)
{
    unsigned char * dst = pStartOfDestinationBuffer;
    unsigned char * src = pStartOfSourceBuffer;

    while ((height--) &&
            IsWriteRangeInBuffer(pStartOfSourceBuffer, src, lSourceLengthInBytes, 0, width) && 
            IsWriteRangeInBuffer(pStartOfDestinationBuffer, dst, lDestinationLengthInBytes, 0, width))
    {
        //__analysis_assume(src >= pStartOfSourceBuffer && src + width <= pStartOfSourceBuffer + lSourceLengthInBytes);
        //__analysis_assume(dst >= pStartOfDestinationBuffer && dst + width <= pStartOfDestinationBuffer + lDestinationLengthInBytes);
        MoveMemory1(dst, src, width);
        dst += dstYStep;
        src += srcYStep;
    }
}

//Range annotations on typedefs
#define MAXCOLOR  10

typedef __range(0, MAXCOLOR-1) int COLOR;

void UseColor(__in COLOR c)
{
    int a[MAXCOLOR];
    a[c] = 1;           // Seems there is no interesting PREfix test case for this...
}

// Does not work yet
struct SColor {
    COLOR c;
    int arr[MAXCOLOR];
};

void UseSColor(__in SColor *s)
{
    s->arr[s->c]++;
}

void main()
{
    for (int i = 0; i < 10; )
        g[i] = ++i;

    f(9);   // OK
    f(10);  // BAD. Overflow.
    f(-1);  // BAD: Underflow [PFXFN] But PREfix doesn't find this.

    int idx;
    int val;

    idx = find(g, 1, 10);
    val = g[idx];   // BAD. This can over/underflow depending on what find() returns.

    idx = find1(g, 1, 10);
    val = g[idx];       // BAD. This can over/underflow depending on what find() returns. Somehow PREfix does report this.

    h(g, 10);   // BAD: This can over/underflow as find1 can return -1

    h1(g, 10);  // BAD: It can overflow: PREfix doesn't find this.

    int size = 10;
    foo(g, size);   // While ESPX says foo is OK, PREfix doesn't like it.
    size = 10;      // Just make sure.
    foo1(g, size);  // OK

    get(g, 10, 9);  // OK - we comply with the annotations and function does not have bug.

    char ca[10] = {};
    Access(ca, 10, -1); // OK. Validate returns false
    Access(ca, 10, 9);  // OK
    Access(ca, 10, 10); // BAD. Validate has bug.

    // For now, postponing to add test coverage for PREfix
    // for IsWriteRangeInBuffer and ButtHeadCopyBits...
}
