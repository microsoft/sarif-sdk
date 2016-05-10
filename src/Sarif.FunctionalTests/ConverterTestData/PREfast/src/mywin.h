#include "specstrings.h"
#ifndef NULL
#define NULL 0
#endif
typedef unsigned long       DWORD;
typedef int                 BOOL;
typedef unsigned char       BYTE;
typedef unsigned short      WORD;
typedef float               FLOAT;
typedef FLOAT               *PFLOAT;
typedef BOOL            *PBOOL;
typedef BOOL             *LPBOOL;
typedef BYTE            *PBYTE;
typedef BYTE             *LPBYTE;
typedef int             *PINT;
typedef int              *LPINT;
typedef WORD            *PWORD;
typedef WORD             *LPWORD;
typedef long             *LPLONG;
typedef DWORD           *PDWORD;
typedef DWORD            *LPDWORD;
typedef void             *LPVOID;
typedef const void       *LPCVOID;

typedef int                 INT;
typedef unsigned int        UINT;
typedef unsigned int        *PUINT;

typedef unsigned long ULONG;
typedef ULONG *PULONG;
typedef unsigned short USHORT;
typedef USHORT *PUSHORT;
typedef unsigned char UCHAR;
typedef UCHAR *PUCHAR;
typedef char *PSZ;


typedef char CHAR;
typedef short SHORT;
typedef long LONG;

typedef unsigned short wchar_t;
typedef unsigned short WCHAR;

typedef _Return_type_success_(return >= 0) LONG HRESULT;
#define SUCCEEDED(hr)   (((HRESULT)(hr)) >= 0)
#define FAILED(hr)      (((HRESULT)(hr)) < 0)

typedef WCHAR *PWCHAR, *LPWCH, *PWCH;
typedef const WCHAR *LPCWCH, *PCWCH;
typedef  _Null_terminated_ WCHAR *NWPSTR, *LPWSTR, *PWSTR;
typedef  PWSTR *PZPWSTR;
typedef  const PWSTR *PCZPWSTR;
typedef  _Null_terminated_ WCHAR  *LPUWSTR, *PUWSTR;
typedef  _Null_terminated_ const WCHAR *LPCWSTR, *PCWSTR;
typedef  PCWSTR *PZPCWSTR;
typedef  _Null_terminated_ const WCHAR  *LPCUWSTR, *PCUWSTR;

typedef CHAR *PCHAR, *LPCH, *PCH;
typedef const CHAR *LPCCH, *PCCH;
typedef  _Null_terminated_ CHAR *NPSTR, *LPSTR, *PSTR;
typedef  PSTR *PZPSTR;
typedef  const PSTR *PCZPSTR;
typedef  _Null_terminated_ const CHAR *LPCSTR, *PCSTR;
typedef  PCSTR *PZPCSTR;


typedef WCHAR OLECHAR;

typedef  OLECHAR *LPOLESTR;

typedef  const OLECHAR *LPCOLESTR;

typedef _Null_terminated_ OLECHAR *BSTR;

typedef _Return_type_success_(return >= 0) long NTSTATUS;
typedef size_t SIZE_T;



#ifndef WINAPI
#define WINAPI __stdcall
#endif

#ifdef __cplusplus
extern "C"
{
#endif

__declspec(dllimport)
DWORD
__stdcall
GetLongPathNameA(
         LPCSTR lpszShortPath,
         LPSTR  lpszLongPath,
         DWORD cchBuffer
    );
__declspec(dllimport)
DWORD
__stdcall
GetLongPathNameW(
         LPCWSTR lpszShortPath,
         LPWSTR  lpszLongPath,
         DWORD cchBuffer
    );

#ifdef UNICODE
#define GetLongPathName  GetLongPathNameW
#else
#define GetLongPathName  GetLongPathNameA
#endif // !UNICODE

#define MAX_COMPUTERNAME_LENGTH 15
#define MAX_PATH          260
#define REMOTE_NAME_INFO_SIZE 12

#define S_OK 0
#define E_FAIL 0x80004005

int
WINAPI
lstrlenA(
    LPCSTR lpString
    );

int
WINAPI
lstrlenW(
    LPCWSTR lpString
    );

LPSTR
WINAPI
lstrcpynA(
    _Out_ LPSTR lpString1,
    LPCSTR lpString2,
    int iMaxLength
    );

LPWSTR
WINAPI
lstrcpynW(
    _Out_ LPWSTR lpString1,
    LPCWSTR lpString2,
    int iMaxLength
    );

LPSTR
WINAPI
lstrcpyA(
    _Out_ LPSTR lpString1,
    LPCSTR lpString2
    );

LPWSTR
WINAPI
lstrcpyW(
    _Out_ LPWSTR lpString1,
    LPCWSTR lpString2
    );

LPSTR
WINAPI
lstrcatA(
    LPSTR lpString1,
    LPCSTR lpString2
    );

LPWSTR
WINAPI
lstrcatW(
    LPWSTR lpString1,
    LPCWSTR lpString2
    );

_At_buffer_(_Dst, _Iter_, _Size, _Post_satisfies_(_Dst[_Iter_] == _Src[_Iter_]))  
void wcscpy_s(
    _Out_writes_z_(_Size) wchar_t *_Dst,
    _In_ size_t _Size,
    _In_z_ const wchar_t *_Src
    );

void wcscat_s(
    _Inout_updates_z_(_Size) wchar_t *_Dst,
    _In_ size_t _Size,
    _In_z_ const wchar_t *_Src
    );

_Post_equal_to_(_String_length_(_S))
size_t wcslen(
    _In_z_ const wchar_t *_S
    );

_Post_equal_to_(_String_length_(_S))
size_t strlen(
    _In_z_ const char *_S
    );

//_Ret_maybenull_
//_Post_writable_byte_size_(size)
//void *malloc(
//    size_t size
//    );
//
//
//void free(
//    _Post_ptr_invalid_ void* p
//    );
//
//#define CopyMemory memcpy
//
//_Post_equal_to_(_Dst)  
//_At_buffer_((unsigned char*)_Dst, _Iter_, _MaxCount, _Post_satisfies_(((unsigned char*)_Dst)[_Iter_] == ((unsigned char*)_Src)[_Iter_]))  
//void * memcpy(
//        _Out_writes_bytes_all_(_MaxCount) void * _Dst,
//        _In_reads_bytes_(_MaxCount) const void * _Src,
//        _In_ size_t _MaxCount
//        );  

#ifdef __cplusplus
}
#endif
