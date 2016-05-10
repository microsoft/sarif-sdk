#include <specstrings.h>
#include "undefsal.h"

typedef char BYTE;

extern "C"
_Post_equal_to_(_String_length_(p))
size_t mystrlen(
    _In_z_ const char *p
);

extern "C"
_Post_equal_to_(dst)
_At_buffer_(dst, _I_, count, _Post_satisfies_(((BYTE*)dst)[_I_] == ((BYTE*)src)[_I_]))
void* mymemcpy(
    _Out_writes_bytes_all_(count) void* dst,
    _In_reads_bytes_(count) const void* src,
    _In_ size_t count
);

void CopyNullTerminatedString(_In_z_ const char* input)
{
    if (input == 0) return;   
    
    size_t chars = mystrlen(input);
    if (chars >= 1023) return;
    
    char a[1024];
    mymemcpy(a, input, chars + 1);
    // should see no warning about calling mystrlen() as a is now null-terminated
    mystrlen(a);    
}

void CopyNullTerminatedBuffer()
{
    char a[10];
    char b[10];
    
    a[3] = 0;
    
    mymemcpy(b, a, 10);
    // should see no warning about calling mystrlen() as b is now null-terminated
    mystrlen(b);

}

void CopyNullTerminatedBufferToOffset()
{
    char a[10];
    char b[12];

    b[0] = 'a';
    b[1] = 'b';
    a[3] = 0;
    
    mymemcpy(b + 2, a, 10);
    // should see no warning about calling mystrlen() as b is now null-terminated
    // however they are not the same length. Even if analysis can't prove they
    // are different lengths, it certainly shouldn't prove they are the same,
    // so we should generate a warning on the a[11] access.
    if (mystrlen(a) != mystrlen(b))
	a[11] = 0;
}

