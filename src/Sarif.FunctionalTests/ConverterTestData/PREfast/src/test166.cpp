#include <sal.h>
#include "undefsal.h"

#define NULL 0

typedef _Return_type_success_(return >= 0) long HRESULT;
typedef unsigned short wchar_t;
typedef _Null_terminated_ wchar_t *PWSTR;
typedef _Null_terminated_ const wchar_t *PCWSTR;

HRESULT GetText(_Outptr_ PWSTR *ppszText);
HRESULT GetTextCOM(_Outptr_ _On_failure_(_Deref_post_null_) PWSTR *ppszText);
HRESULT SetText(_In_ PCWSTR pszText);

extern "C" _Ret_range_(==, wcslen(psz)) unsigned int wcslen(_In_ PCWSTR psz);

void Test1()
{
    PWSTR pszText;

    GetText(&pszText);
    SetText(pszText);    // would be reasonable to warn here...
    pszText[wcslen(pszText)] = L'\0';  // or here
}

void Test2()
{
    PWSTR pszText;

    GetTextCOM(&pszText);
    SetText(pszText);   // not reasonable to warn here...
    pszText[wcslen(pszText)] = L'\0';  // or here
}

