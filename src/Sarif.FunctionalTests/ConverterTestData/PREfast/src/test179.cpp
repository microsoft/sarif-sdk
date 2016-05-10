#include <specstrings.h>
#include "undefsal.h"

typedef unsigned short USHORT;
typedef unsigned long ULONG;
typedef unsigned char BYTE;
typedef char CHAR;
typedef CHAR* PCHAR;

extern "C"
_Post_equal_to_(dst)
_At_buffer_(dst, _I_, count, _Post_satisfies_(((BYTE*)dst)[_I_] == ((BYTE*)src)[_I_]))
void* memcpy(
    _Out_writes_bytes_all_(count) void* dst,
    _In_reads_bytes_(count) const void* src,
    _In_ size_t count
);

enum MD5_AUTH_NAME
{
    MD5_AUTH_USERNAME = 0,
    MD5_AUTH_CNONCE,
    MD5_AUTH_NC,
    MD5_AUTH_HENTITY,
    MD5_AUTH_AUTHZID,           
    MD5_AUTH_LAST
};

typedef struct _STRING {
    USHORT Length;
    USHORT MaximumLength;
    _Field_size_bytes_part_(MaximumLength, Length) PCHAR Buffer;
} STRING;

typedef struct _DIGEST_PARAMETER
{
    USHORT usFlags;                              
    STRING refstrParam[MD5_AUTH_LAST];         
    USHORT usDirectiveCnt[MD5_AUTH_LAST];      
    ULONG UserId;   

} DIGEST_PARAMETER, *PDIGEST_PARAMETER;

void bar(_In_ PDIGEST_PARAMETER pDigest)
{
    if (pDigest->refstrParam[MD5_AUTH_NC].Length && (pDigest->refstrParam[MD5_AUTH_NC].Length <= 8))
    {
        CHAR  czNCBuf[8 + 1] = {0};
        // this was causing a false positive when CfgBuilder was rewriting the third parameter as:
        // (&pDigest->refstrParam[MD5_AUTH_NC])->Length
        memcpy(czNCBuf, pDigest->refstrParam[MD5_AUTH_NC].Buffer, pDigest->refstrParam[MD5_AUTH_NC].Length);
    }
}