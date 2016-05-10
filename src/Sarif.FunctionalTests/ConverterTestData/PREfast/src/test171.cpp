#include <sal.h>
#include "undefsal.h"

void GetDataSimple(
    _Outptr_ char** pData
);

void UseDataSimple()
{
    unsigned char* data;
    GetDataSimple((char**)&data);
    data[0] += 1; // should be ok, assumed to be at least 1 byte long
    data[1] += 1; // should get warning, we don't know if it is longer than 1 byte
}

void GetDataSized(
    _Outptr_result_bytebuffer_(*pdwSize) char** pData,
    _Out_ unsigned int* pdwSize
);

void UseDataSized()
{
    unsigned char* data;
    unsigned int size;
    GetDataSized((char**)&data, &size);
    data[0] += 1; // should be ok, assumed to be at least 1 byte long
    data[1] += 1; // should get unvalidated access warning.
}
    
void GetData(
     _When_(pdwSize != 0, _Outptr_result_bytebuffer_(*pdwSize))
     _When_(pdwSize == 0, _Outptr_)
                 char **pData,    
    _Out_opt_   unsigned int *pdwSize
);

void UseData()
{
    unsigned char *data;
    unsigned int size;

    GetData((char **)&data, &size);
    
    if (size >= 2)
    {
        data[0] += 1;  // no overrun
        data[1] += 1;  // no overrun
    }

    GetData((char **)&data, 0);

    data[0] += 1;  // should be ok, assumed to be at least 1 byte long
    data[1] += 1;  // should get warning, we don't know if it is longer than 1 byte
}

void SomeStringFunc(_When_(bufferSize >= 0, _In_reads_(bufferSize)) _When_(bufferSize < 0, _In_z_) const char* buffer, _In_ unsigned int bufferSize);

void EnsureConditionsPropagateToImplicitAnnotations(_In_reads_(size) const char* buffer, _In_ unsigned int size)
{
    if (buffer[0] == 't')
    {
        buffer++;
        size--;
        // this call should be safe, but is erroneously flagged
        // if the _When_'s are not propagated to all
        // implicitly-generated annotations.
        SomeStringFunc(buffer, size); 
    }
}