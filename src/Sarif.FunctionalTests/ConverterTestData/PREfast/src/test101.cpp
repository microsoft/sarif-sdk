#include "specstrings.h"
#include "mymemory.h"
#include "undefsal.h"

typedef struct _FLEXARRAY {
    unsigned int count;
    __ecount(count) int arr[1];
} FLEXARRAY;

void bad1(__in_ecount(cnt) int *buf, size_t cnt, __inout FLEXARRAY *f)
{
    memcpy(f->arr, buf, cnt * sizeof(int)); // BAD. 26015. Can overflow. f.arr can be smaller than cnt.
}

__valid FLEXARRAY* good(__in_ecount(cnt) int *buf, size_t cnt)
{
    FLEXARRAY *f = (FLEXARRAY*)malloc(sizeof(_FLEXARRAY) + ((cnt - 1) * sizeof(int)));
    if (f != nullptr)
    {
        f->count = cnt;
        memcpy(f->arr, buf, cnt * sizeof(int));
    }
    return f;   // [ESPXFP?] ESPX reports 26045. Not sure why...
}

__valid FLEXARRAY* bad2(size_t cnt)
{
    FLEXARRAY *f = (FLEXARRAY*)malloc(sizeof(_FLEXARRAY) + ((cnt - 1) * sizeof(int)));
    if (f != nullptr)
        f->count = cnt + 1;
    return f;   // BAD. Can cause overflow (f->count is one larger).
}

struct Foo
{
    void what(__inout FLEXARRAY *f, __in_ecount(cnt) int *buf, size_t cnt)
    {
        memcpy(f->arr, buf, cnt * sizeof(int));
    }
};

void main()
{
    int i10[10] = {};
    FLEXARRAY f1;
    f1.count = 1;
    f1.arr[0] = 1;  // OK
    bad1(i10, 10, &f1);  // BAD. Overflows f1.arr. PFX(25).

    FLEXARRAY* f2 = good(i10, 10);
    if (f2 != nullptr)
        f2->arr[f2->count - 1] = 1;   // OK

    FLEXARRAY* f3 = bad2(10);
    if (f3 != nullptr)
        f3->arr[f3->count - 1] = 1;   // BAD. Overflows. PFX(23)

    Foo f4;
    f4.what(&f1, i10, 10);  // BAD. Overflows. ESPX(26015). PFX(25).
}
