
#include <sal.h>
#include <string.h>

void C6305FromAnnotation(
		_In_reads_bytes_(inc) int *p,
		int inc,
		bool b)
{
  int cb = 1;
  if (b)
	  p +=inc;
  else
	  p += cb;

}

int RemoveRedundantKeyEvents1(wchar_t * p)
{
    int i = 6;

    int j = sizeof(p);

    j = j + wcslen(p);

    return i;
}

int RemoveRedundantKeyEvents2(wchar_t * p)
{
	int i = sizeof(p);	 // Yes, sizeof(wchar_t*) doesn't make any sense

	i = wcslen(p) - i;
	int j = i;
	i += wcslen(p);
	i -= wcslen(p);

	i = wcslen(p) + i;
	i = wcslen(p) - i;

	return j;
}

int CorrectAliasing(wchar_t * p)
{
    int i = 6;
    int j = sizeof(p);

    i += j;

    int k = i + wcslen(p);

    return k;
}

int ShowKeyEventsWhenNoLocation()
{
    int source [] = {1, 2, 3, 4, 5, 6, 7};
    int * p = source;

    p += sizeof(int);

    return *p;
}

void ShowKeyEventsWhenNoLocation2()
{
	int i = 0;
	int* pI = &i;
	pI += sizeof(int);
}

void ShowKeyEventsForPointerArithmetic(int *p)
{
   int cb = sizeof(int);

   p += cb + 2;
}
