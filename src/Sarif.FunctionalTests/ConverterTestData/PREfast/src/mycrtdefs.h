#pragma once

#ifndef _CRTIMP
#if defined(CRTDLL) && defined(_CRTBLD)
#define _CRTIMP __declspec(dllexport)
#else  /* defined(CRTDLL) && defined(_CRTBLD) */
#ifdef _DLL
#define _CRTIMP __declspec(dllimport)
#else  /* _DLL */
#define _CRTIMP
#endif  /* _DLL */
#endif  /* defined(CRTDLL) && defined(_CRTBLD) */
#endif  /* _CRTIMP */

/* Define _CRTNOALIAS, _CRTRESTRICT */
#ifndef _CRTNOALIAS
#define _CRTNOALIAS __declspec(noalias)
#endif  /* _CRTNOALIAS */

#ifndef _CRTRESTRICT
#define _CRTRESTRICT __declspec(restrict)
#endif  /* _CRTRESTRICT */

/* __declspec(guard(overflow)) enabled by /sdl compiler switch for CRT allocators */
#ifdef _GUARDOVERFLOW_CRT_ALLOCATORS
#define _CRT_GUARDOVERFLOW __declspec(guard(overflow))
#else
#define _CRT_GUARDOVERFLOW 
#endif

/* jit64 instrinsic stuff */
#ifndef _CRT_JIT_INTRINSIC
#if defined (_M_CEE) && defined (_M_X64)
/* This is only needed when managed code is calling the native APIs, targeting the 64-bit runtime */
#define _CRT_JIT_INTRINSIC __declspec(jitintrinsic)
#else  /* defined (_M_CEE) && defined (_M_X64) */
#define _CRT_JIT_INTRINSIC
#endif  /* defined (_M_CEE) && defined (_M_X64) */
#endif  /* _CRT_JIT_INTRINSIC */
