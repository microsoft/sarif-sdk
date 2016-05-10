#include "specstrings.h"
#include "undefsal.h"   // Dummy - see below.

// Expansion of valid  annotation to use the annotation on the type. In this case the annotation is on a typedef more than one level deep
typedef unsigned short WCHAR;
typedef __readableTo(sentinel(0)) WCHAR * LPWSTR;
typedef LPWSTR PTSTR, LPTSTR;

void g(/*__in LPTSTR */ __readableTo(sentinel(0)) const WCHAR * s)
{
	int i = 0;
	if (*s)
		i += *s;
	s++;
	if (*s)
		i += *s;
}

void main()
{
    LPTSTR s = (LPTSTR)(L"");
    g(s);
}