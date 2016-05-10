#include <specstrings.h>
#include "undefsal.h"

typedef unsigned short wchar_t;
typedef __nullterminated wchar_t *PWSTR;

extern "C" __bcount(size) void *malloc(unsigned int size);

extern "C"
_Post_equal_to_(dst)
_At_buffer_(dst, _I_, count, _Post_satisfies_(((unsigned char*)dst)[_I_] == ((unsigned char*)src)[_I_]))
void* memcpy(
    _Out_writes_bytes_all_(count) void* dst,
    _In_reads_bytes_(count) const void* src,
    _In_ size_t count
);

extern "C" __out_range(==, _String_length_(str)) unsigned int wcslen(_In_z_ const wchar_t *str);

_At_((PWSTR *)ppOut, __deref_out)
void f(
  __deref_out void **ppOut
)
{
  const wchar_t Source[] = L"Transmogrification";
  wchar_t *Destination;

  Destination = (wchar_t *)malloc(sizeof(Source));
  memcpy(Destination, Source, sizeof(Source));
  *ppOut = (void *)Destination; // No warning should be generated here.
}

void Test()
{
  wchar_t *String;
  size_t Length;

  f((void **)&String);

  Length = wcslen(String); // No warning should be generated here.
}
