
// Key Events for 6385
// Add key events to track the subscript

#include <Windows.h>

void fd6327 ()
{
  BSTR bstr;

  int x;
  x=1;
  int j = 90;
  x+=j;
  bstr = SysAllocStringLen(L"txt", x); //@@@Expects:6385

  // code...
  SysFreeString(bstr);
}

