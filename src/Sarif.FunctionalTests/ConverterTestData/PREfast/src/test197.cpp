#include <specstrings.h>
#include "mywin.h"
#include "undefsal.h"

struct SomeStruct
{
    int FieldOne;
    int FieldTwo;    
};

void BadGetData(
    _Out_ PUCHAR* header,
    _Out_ PULONG  logOffset
    );

void BadGetData2(
    _At_((PSTR*)buffer, _Outptr_result_buffer_(*length))  void** buffer,
    _Out_ PULONG length
    )
{
    *length = 0; 
    return; // should warn 26007 - we didn't set anything to NULL or zero length!
}

void GoodGetData(
    _At_((PSTR*)buffer, _Outptr_result_buffer_(*length))  void** buffer,
    _Out_ PULONG length
    )
{
    *buffer = NULL;
    *length = 0;
    return; // this is safe - we set the zero-length buffer to NULL (which SAL allows)
}

void foo()
{
    PUCHAR bufferHeader;
    ULONG offset;

    BadGetData(&bufferHeader, &offset);

    SomeStruct* p = (SomeStruct*) bufferHeader;
    int pages = p->FieldOne; // should generate 26007 - only one byte was advertised as being created!

    PWSTR buffer;
    ULONG length;
    BadGetData2((void**)&buffer, &length);
    if (length == 0) return;
    WCHAR firstChar = buffer[0]; // should generate 26018 - WCHAR/CHAR mismatch!

    GoodGetData((void**)&buffer, &length);
    if (length == 0) return;
    firstChar = buffer[0];      // should generate 26018 - WCHAR/CHAR mismatch!
}

