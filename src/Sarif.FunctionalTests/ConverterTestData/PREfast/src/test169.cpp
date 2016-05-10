#include <sal.h>
#include "undefsal.h"

typedef _Return_type_success_(return >= 0) long HRESULT;

// No warning should be reported
HRESULT MyAPI1(_Out_ _On_failure_(_Post_satisfies_(*p == _Old_(*p))) int *p)
{
    return -1;
}

// No warning should be reported
HRESULT MyAPI2(_Out_ _On_failure_(_Deref_out_range_(==, _Old_(*p))) int *p)
{
    return -1;
}

// Should report warning
HRESULT MyAPI3(_Out_ _On_failure_(_Post_satisfies_(*p == _Old_(*p) + 1)) int *p)
{
    return -1;
}

// Should report warning
HRESULT MyAPI4(_Out_ _On_failure_(_Deref_out_range_(==, _Old_(*p) + 1)) int *p)
{
    return -1;
}

// No warning should be reported
HRESULT MyAPI5(_Outptr_ _On_failure_(_Post_satisfies_(*p == _Old_(*p))) int **p)
{
    return -1;
}

// No warning should be reported
HRESULT MyAPI6(_Outptr_ _On_failure_(_Deref_out_range_(==, _Old_(*p))) int **p)
{
    return -1;
}

