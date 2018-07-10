#include <string.h>
#define MAX 25

void foo(void)
{
  char a[35];
  char b[10] = {'a','b','c','d','\0'};
  
  strcpy(a, b);										
  strncat(a, "this string is long", sizeof (a));	//@@@Expects: 6059
}

