#include <specstrings.h>
#include "undefsal.h"

typedef unsigned int DWORD;

typedef __struct_bcount(cbStruct) struct tagBIND_OPTS {
    DWORD           cbStruct;
    DWORD           grfFlags;
    DWORD           grfMode;
    DWORD           dwTickCountDeadline;
} BIND_OPTS;


int Foo(__inout BIND_OPTS *pBO) { if (pBO) { pBO->grfMode = pBO->grfFlags; }  return 1; };

void Test1()
{
    BIND_OPTS bo = {sizeof(bo), 0};
    Foo(&bo);   // OK. Noting to be said about...
}

typedef struct {
    DWORD dw1;
    DWORD dw2;
} TEST_INFO;

void Test2(__in int *pInt)
{
    TEST_INFO to = {};
    pInt[to.dw2] = 5;   // Assuming pInt != nullptr, OK.
}

typedef struct tag_DERIVED_OPTS
{
    DWORD firstField;
    BIND_OPTS bo;
    DWORD moreFlags;
} DERIVED_OPTS;

void Test3()
{
    DERIVED_OPTS dopt = {5, sizeof(BIND_OPTS), 0, 0, 0, 0 };
    Foo(&dopt.bo);  // OK
}

void Test4(__in int *pInt)
{
    BIND_OPTS bo = {sizeof(bo)};
    pInt[bo.grfMode] = 5;
}

void Test5(__in int *pInt)
{
    DERIVED_OPTS dopt = {2};
    pInt[dopt.bo.grfMode] = 5;
}

void main()
{
    int a1[1];
    Test2(a1);
    Test4(a1);
    Test5(a1);
}
