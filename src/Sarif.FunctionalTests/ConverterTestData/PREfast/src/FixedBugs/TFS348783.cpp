
// Array size shown in Key Events for 6386

#include<string>

void count(const wchar_t* pSource)
{
  wchar_t buffer[10]; // should indicate buffer is an array of 10 elements
  wcscpy_s(buffer,sizeof(buffer),pSource);
}
