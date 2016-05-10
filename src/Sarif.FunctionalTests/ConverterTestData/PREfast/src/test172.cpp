#include <sal.h>

typedef unsigned short WCHAR;
typedef _Null_terminated_ char *PSTR;
typedef _Null_terminated_ WCHAR *PWSTR;
typedef _Null_terminated_ const WCHAR *PCWSTR;

extern "C"
_Post_equal_to_(_String_length_(str)) unsigned int wcslen(PCWSTR str);

#include "undefsal.h"

//
// Simplified test case
//

_Success_(return)
bool VerifySz(
   _In_reads_(2) char *buf,
   _Outptr_ PSTR *pStr
   )
{
    if (buf[1] == '\0')
    {
        *pStr = buf;
        return true;
    }

    return false;
}


//
// Test case for Esp:1039
//

bool
TestOACR_26035(
    __in                   size_t       cchSize,
    __in_ecount(cchSize)   WCHAR const* wsData
    )
{
    if (cchSize == 0) {
        return false;
    }

    if (wsData[cchSize - 1] != 0) {
        return false;
    }

    // **** FALSE DETECTION ****
    // warning 26035: Possible precondition violation due to failure to null terminate string 'wsData' 
    // Buffer wsData is a parameter to this function declared on line 196 
    // Annotation on function wcslen requires that {parameter 1} is null terminated 
    // where {parameter 1} is wsData [Annotation __nullterminated] 
    if ((wcslen(wsData) + 1) != cchSize) {
        return false; 
    }

    return true;
}


//
// Test case for Esp:895
//

typedef enum AstNodeKind
{
    AST_NULLSTMT,
    AST_LE,
    AST_LT,
    AST_EQUALS,
    AST_GE,
    AST_GT,
    AST_NE
};

AstNodeKind ParseRelop(
    _Inout_ PWSTR p
    )
{
    AstNodeKind kind = AST_NULLSTMT;;
    switch(*p++)
    {
    case '<':
        if (*p == '=')
        {
            p++;
            kind = AST_LE;
        }
        else
        {
            kind = AST_LT;
        }
        break;
    case '=':
        if (*p == '=')
        {
            p++;
            kind = AST_EQUALS;
        }
        break;
    case '>':
        if (*p == '=')
        {
            p++;
            kind = AST_GE;
        }
        else
        {
            kind = AST_GT;
        }
    case '!':
        if (*p == '=')
        {
            p++;
            kind = AST_NE;
        }
        break;
    default:
        break;
    }

    return kind;
}

//
// Now test to make sure we catch walking off the end of a nullterm buffer
//

unsigned int BadNullTerm(
    _In_ PSTR str
    )
{
    unsigned int result = 0;

    while (*str++)
        result++;

    result += *str;
    return result;
}

