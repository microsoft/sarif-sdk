#pragma once

#include "mycrtdefs.h"

#ifdef __cplusplus
extern "C"
{
#endif

_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_Count*_Size) _CRTIMP _CRT_JIT_INTRINSIC _CRTNOALIAS _CRTRESTRICT    void * __cdecl calloc(_In_ _CRT_GUARDOVERFLOW size_t _Count, _In_ _CRT_GUARDOVERFLOW size_t _Size);
_CRTIMP _CRTNOALIAS void __cdecl free(_Pre_maybenull_ _Post_invalid_ void * _Memory);
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_Size) _CRTIMP _CRT_JIT_INTRINSIC _CRTNOALIAS _CRTRESTRICT                              void * __cdecl malloc(_In_ _CRT_GUARDOVERFLOW size_t _Size);
_Success_(return!=0)
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_NewSize) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl realloc(_Pre_maybenull_ _Post_invalid_ void * _Memory, _In_ _CRT_GUARDOVERFLOW size_t _NewSize);
_Success_(return!=0)
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_Count*_Size) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl _recalloc(_Pre_maybenull_ _Post_invalid_ void * _Memory, _In_ _CRT_GUARDOVERFLOW size_t _Count, _In_ _CRT_GUARDOVERFLOW size_t _Size);
_CRTIMP _CRTNOALIAS void __cdecl _aligned_free(_Pre_maybenull_ _Post_invalid_ void * _Memory);
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_Size) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl _aligned_malloc(_In_ _CRT_GUARDOVERFLOW size_t _Size, _In_ size_t _Alignment);
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_Size) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl _aligned_offset_malloc(_In_ _CRT_GUARDOVERFLOW size_t _Size, _In_ size_t _Alignment, _In_ size_t _Offset);
_Success_(return!=0)
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_NewSize) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl _aligned_realloc(_Pre_maybenull_ _Post_invalid_ void * _Memory, _In_ _CRT_GUARDOVERFLOW size_t _NewSize, _In_ size_t _Alignment);
_Success_(return!=0)
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_Count*_Size) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl _aligned_recalloc(_Pre_maybenull_ _Post_invalid_ void * _Memory, _In_ _CRT_GUARDOVERFLOW size_t _Count, _In_ _CRT_GUARDOVERFLOW size_t _Size, _In_ size_t _Alignment);
_Success_(return!=0)
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_NewSize) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl _aligned_offset_realloc(_Pre_maybenull_ _Post_invalid_ void * _Memory, _In_ _CRT_GUARDOVERFLOW size_t _NewSize, _In_ size_t _Alignment, _In_ size_t _Offset);
_Success_(return!=0)
_Check_return_ _Ret_maybenull_ _Post_writable_byte_size_(_Count*_Size) _CRTIMP _CRTNOALIAS _CRTRESTRICT void * __cdecl _aligned_offset_recalloc(_Pre_maybenull_ _Post_invalid_ void * _Memory, _In_ _CRT_GUARDOVERFLOW size_t _Count, _In_ _CRT_GUARDOVERFLOW size_t _Size, _In_ size_t _Alignment, _In_ size_t _Offset);
_Check_return_ _CRTIMP size_t __cdecl _aligned_msize(_Pre_notnull_ void * _Memory, _In_ size_t _Alignment, _In_ size_t _Offset);

_Check_return_ _CRTIMP int     __cdecl _memicmp(_In_reads_bytes_opt_(_Size) const void * _Buf1, _In_reads_bytes_opt_(_Size) const void * _Buf2, _In_ size_t _Size);
_Check_return_ int     __cdecl memcmp(_In_reads_bytes_(_Size) const void * _Buf1, _In_reads_bytes_(_Size) const void * _Buf2, _In_ size_t _Size);

void *  __cdecl memset(_Out_writes_bytes_all_(_Size) void * _Dst, _In_ int _Val, _In_ size_t _Size);
void *  __cdecl memcpy(_Out_writes_bytes_all_(_Size) void * _Dst, _In_reads_bytes_(_Size) const void * _Src, _In_ size_t _Size);
void *  __cdecl memmove(_Out_writes_bytes_all_opt_(_Size) void * _Dst, _In_reads_bytes_opt_(_Size) const void * _Src, _In_ size_t _Size);

#ifdef __cplusplus
}
#endif
