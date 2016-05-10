#include "specstrings.h"
#include "mywin.h"
#include "undefsal.h"

//ESP bug #404

struct UNICODE_STRING
{
    __field_bcount_part(MaximumLength, Length) WCHAR *Buffer;
    SIZE_T MaximumLength;
    SIZE_T Length;
};

struct RTL_UCSCHAR_ENCODER_RETURN_VALUE
{
    PUCHAR NewCursorValue;
    NTSTATUS FailureReason;
};

_Post_satisfies_(return.NewCursorValue >= begin && return.NewCursorValue <= end)
RTL_UCSCHAR_ENCODER_RETURN_VALUE RtlEncodeUtf16LE(UCHAR c, PUCHAR begin, PUCHAR end)
{
    RTL_UCSCHAR_ENCODER_RETURN_VALUE ret;
    ret.NewCursorValue = begin + (end - begin) / 2;
    return ret;
}

#define  STATUS_INTERNAL_ERROR -1
#define  STATUS_SUCCESS  0

NTSTATUS
Foo(
    __in ULONG Flags,
    __in SIZE_T cChars,
    __in_ecount(cChars) const UCHAR BufOfChars[],
    __inout UNICODE_STRING *StringInOut
    )
{
    const PUCHAR pFirst = reinterpret_cast<PUCHAR>(StringInOut->Buffer);
    const SIZE_T StringInOutLength = StringInOut->Length;
    const SIZE_T StringInOutMaximumLength = StringInOut->MaximumLength;

    PUCHAR pCursor = pFirst + StringInOutLength;

    const PUCHAR pLast = pFirst + StringInOutMaximumLength;

    for (SIZE_T i = 0; i < cChars; i++)
    {
    RTL_UCSCHAR_ENCODER_RETURN_VALUE rv = ::RtlEncodeUtf16LE(BufOfChars[i], pCursor, pLast);
    if (rv.FailureReason < 0)
        return rv.FailureReason;
    pCursor = rv.NewCursorValue;
    }

    if (pCursor < pFirst)
        return STATUS_INTERNAL_ERROR;

    if (pCursor > pLast)
        return STATUS_INTERNAL_ERROR;

    StringInOut->Length = pCursor - pFirst;

    return STATUS_SUCCESS;
}

void main()
{
    UCHAR str[10] = "123456789";
    UNICODE_STRING wStr;
    wStr.Buffer = new WCHAR[20];
    wStr.MaximumLength = 20;
    wStr.Length = 0;
    Foo(0xFF77, 10, str, &wStr);    // OK
}