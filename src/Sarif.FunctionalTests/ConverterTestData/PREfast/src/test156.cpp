#include <specstrings.h>
#include "undefsal.h"

// Without the fix for Win8#168368, then the casts in this annotation cause a PAG assertion
_At_buffer_((unsigned char*)dst, iter, count, _Post_satisfies_(((unsigned char*)dst)[iter] == ((unsigned char*)src)[iter]))
void mycpy(_Out_writes_bytes_all_(count) void* dst, _In_reads_bytes_(count) const void* src, _In_ size_t count);

void SimpleBadCalls(_In_ void* vp)
{
    char a[100];
    const char b[] = "Hello";
    
    mycpy(&a[0], b, 5);
    mycpy(a, b, 5);
    mycpy(a, b, 500); // let's ensure we get warnings for this simple case
    mycpy(&a[0], b, 500); // let's ensure we get warnings for this simple case
}

typedef struct _MyStruct
{
    char   DevicePath[10];
    int    DeviceID;   
} MyStruct;

// Without the fix for Win8#168368, then the casts in this annotation cause a PAG assertion
_At_((char*)data->DevicePath, _Post_ _Null_terminated_)
void MadePAGAndBad(_Inout_ MyStruct* data)
{
    data->DevicePath[0] = 'a'; // this should cause a warning as we are not null-terminating
}

// Without the fix for Win8#168368, then the casts in this annotation cause a PAG assertion
_At_((char*)data->DevicePath, _Post_ _Null_terminated_)
void MadePAGAndGood(_Inout_ MyStruct* data)
{
    data->DevicePath[0] = 'a'; 
    data->DevicePath[1] = 0;
}

// this is the semantically equivalent annotation that used to sneak through.
_At_(data->DevicePath, _Post_ _Null_terminated_)
void NeverMadePAGAndBad(_Inout_ MyStruct* data)
{
    data->DevicePath[0] = 'a'; // this should cause a warning as we are not null-terminating
}

// this is the semantically equivalent annotation that used to sneak through.
_At_(data->DevicePath, _Post_ _Null_terminated_)
void NeverMadePAGAndGood(_Inout_ MyStruct* data)
{
    data->DevicePath[0] = 'a'; 
    data->DevicePath[1] = 0;
}

// a test that we can use indexing in annotations safely
_At_(data->DevicePath[0], _Out_range_(==, 1))
void EnsureIndexingWorksGood(_Inout_ MyStruct* data)
{
    data->DevicePath[0] = 1; 
}

// a test that we can use indexing in annotations safely
_At_(data->DevicePath[0], _Out_range_(==, 1))
void EnsureIndexingWorksBad(_Inout_ MyStruct* data)
{
    data->DevicePath[0] = 2; // this fails the postcondition - we should see a warning
}

// Tests that contain an implicit address-of followed by field access
extern "C" __bcount(size) void *malloc(size_t size);

typedef struct _UNICODE_STRING {  
    unsigned short Length;  
    unsigned short MaximumLength;  
    _Field_size_bytes_part_(MaximumLength, Length) unsigned short* Buffer;  
} UNICODE_STRING;

AcceptsUnicodeString(  
    _In_ UNICODE_STRING ValueName
    ); 
    
void GoodUnicodeStringCall(_In_ UNICODE_STRING valueName)
{
    valueName.Buffer = (unsigned short*)malloc(10*sizeof(unsigned short));
    valueName.Length = 10;
    valueName.MaximumLength = 20;
    // in this call, &(valueName)->MaximumLength and &(valueName)->Length
    // are being evaluated implicitly, so test they pass annotation processing correctly
    AcceptsUnicodeString(valueName);   
}

void BadUnicodeStringCall(_In_ UNICODE_STRING valueName)
{
    valueName.Buffer = (unsigned short*)malloc(2*sizeof(unsigned short));
    valueName.Length = 20;
    valueName.MaximumLength = 10;
    // we expect a warning here that we have violated
    // the field constraints on UNICODE_STRING
    AcceptsUnicodeString(valueName); 
}


// Tests that ensure that tricky macros around taking the address of
// a trailing "flexarray" field in a structure do not cause a problem

// this macro (used in minkernel extensively) was causing annotations.cpp
// to PAG assert. Should get through fine now.
#define FIELD_OFFSET(type, field) ((size_t)&(((type *)0)->field))  

extern "C" __bcount(size) void *malloc(size_t size);

typedef struct _DEVICE_OBJECT
{
    int DeviceId;
    int ThreadCount;
} DEVICE_OBJECT, *PDEVICE_OBJECT;

typedef struct _DEVICE_RELATIONS
{
    size_t Count;
    PDEVICE_OBJECT Objects[1];  // variable length
} DEVICE_RELATIONS, *PDEVICE_RELATIONS;

void GoodFieldOffsetCall()
{
    size_t count = 2;
    size_t fldOffset = FIELD_OFFSET(DEVICE_RELATIONS, Objects[count]);
    PDEVICE_RELATIONS p = (PDEVICE_RELATIONS) malloc(fldOffset);
    
    p->Objects[count - 1]->DeviceId = 2;
}
