#ifndef _WINERROR_
#define _WINERROR_

#if defined (_MSC_VER) && (_MSC_VER >= 1020) && !defined(__midl)
#pragma once
#endif

#ifndef FORCEINLINE
#if (_MSC_VER >= 1200)
#define FORCEINLINE __forceinline
#else
#define FORCEINLINE __inline
#endif
#endif

#include <specstrings.h>

#define FACILITY_WINDOWS                 8
#define FACILITY_WIN32                   7


// typedef for HRESULT copied from ntdef.h
#ifndef _HRESULT_DEFINED
#define _HRESULT_DEFINED
#ifdef __midl
typedef LONG HRESULT;
#else
typedef _Return_type_success_(return >= 0) long HRESULT;
#endif // __midl
#endif // !_HRESULT_DEFINED

#ifdef RC_INVOKED
#define _HRESULT_TYPEDEF_(_sc) _sc
#else // RC_INVOKED
#define _HRESULT_TYPEDEF_(_sc) ((HRESULT)_sc)
#endif // RC_INVOKED

#define SUCCEEDED(hr) (((HRESULT)(hr)) >= 0)
#define FAILED(hr) (((HRESULT)(hr)) < 0)

#define S_OK                             ((HRESULT)0L)
#define S_FALSE                          ((HRESULT)1L)
#define E_FAIL                           _HRESULT_TYPEDEF_(0x80004005L)

// HRESULT_FROM_WIN32(x) used to be a macro, however we now run it as an inline function
// to prevent double evaluation of 'x'. If you still need the macro, you can use __HRESULT_FROM_WIN32(x)
#define __HRESULT_FROM_WIN32(x) ((HRESULT)(x) <= 0 ? ((HRESULT)(x)) : ((HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000)))
#ifndef __midl
FORCEINLINE HRESULT HRESULT_FROM_WIN32(unsigned long x) { return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);}
#else
#define HRESULT_FROM_WIN32(x) __HRESULT_FROM_WIN32(x)
#endif

#endif