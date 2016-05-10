#include <specstrings.h>
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

typedef void* PVOID;
typedef long LONG;
typedef long LONG_PTR;

void bar(_Outptr_result_maybenull_ PWSTR* pp);
extern "C"
_Post_equal_to_(dst)
_At_buffer_(dst, _I_, count, _Post_satisfies_(((BYTE*)dst)[_I_] == ((BYTE*)src)[_I_]))
void* CopyMemory(
    _Out_writes_bytes_all_(count) void* dst,
    _In_reads_bytes_(count) const void* src,
    _In_ size_t count
);

struct MyStruct
{
    PCWSTR p;
};

void should_not_warn(_In_ MyStruct* s)
{
    bar((PWSTR*)&(s->p));
}

struct UString
{
    USHORT Length;
    USHORT MaximumLength;
    _Field_size_bytes_part_(MaximumLength, Length) PWCH Buffer;
};

_At_(&s->Buffer, _Outptr_result_bytebuffer_(s->MaximumLength))
void CreateUstring(_Out_ UString* s);

void should_not_warn2(_In_ UString* p)
{
    CreateUstring(p);
}

#define FIELD_OFFSET(type, field)    ((LONG)(LONG_PTR)&(((type *)0)->field))
#define RTL_FIELD_SIZE(type, field) (sizeof(((type *)0)->field))
#define RTL_SIZEOF_THROUGH_FIELD(type, field) (FIELD_OFFSET(type, field) + RTL_FIELD_SIZE(type, field))
#define RTL_CONTAINS_FIELD(Struct, Size, Field)  ( (((PCHAR)(&(Struct)->Field)) + sizeof((Struct)->Field)) <= (((PCHAR)(Struct))+(Size)) )

struct FIXED_SENSE_DATA
{
    PVOID Blah;
    ULONG AdditionalSenseLength;
};

typedef FIXED_SENSE_DATA* PFIXED_SENSE_DATA;

// this function should not issue warnings....as the RTL_* macros are safe
// - they are just dealing in addresses, but were triggering false positives
// from EspX when the address-of operators caused the wrong buffers to be checked
// for overrun.
void ScsiGetTotalSenseByteCountIndicated (
   _In_reads_bytes_(SenseInfoBufferLength) PVOID SenseInfoBuffer,
   _In_  UCHAR SenseInfoBufferLength
   )
{
    ULONG byteCount;
    if (SenseInfoBuffer == NULL ||
        SenseInfoBufferLength == 0) {
        return;
    }


    PFIXED_SENSE_DATA senseInfoBuffer = (PFIXED_SENSE_DATA)SenseInfoBuffer;

    if (RTL_CONTAINS_FIELD(senseInfoBuffer,
                           SenseInfoBufferLength,
                           AdditionalSenseLength)) {

        if (senseInfoBuffer->AdditionalSenseLength <=
            (256 - RTL_SIZEOF_THROUGH_FIELD(FIXED_SENSE_DATA, AdditionalSenseLength))) {

            byteCount = senseInfoBuffer->AdditionalSenseLength
                        + RTL_SIZEOF_THROUGH_FIELD(FIXED_SENSE_DATA, AdditionalSenseLength);
        }
    }
}

struct Interface
{
    ULONG BufferLength;
    char Buffer[1];
};

struct Implementation
{
    ULONG BufferLength;
    char Buffer[12];
};

void ReadFrom(_In_reads_bytes_(p->BufferLength) Interface* p);

void shold_not_warn3()
{
    Implementation s;

    Interface* p = (Interface*)&s;
    p->BufferLength = sizeof(s);

    ReadFrom(p);
}

typedef  const char* PCNZCH;

_Check_return_ const char *  strchr(_In_z_ const char * _Str, _In_ int _Val);

_Success_(return)
bool StringCchCopyN(
            _Out_writes_(cchDest) _Always_(_Post_z_) LPSTR pszDest,
            _In_ size_t cchDest,
            _In_reads_or_z_(cchToCopy) PCNZCH pszSrc,
            _In_ size_t cchToCopy);


// should not warn as everything is ok.
void f648(_In_ PCSTR FullPath)
{
    char DepotPart[256];
    if (strlen(FullPath) > sizeof(DepotPart))
        return;

    PCSTR t = strchr(FullPath, '\\');
    if (t == NULL)
        return;

    StringCchCopyN(DepotPart, sizeof(DepotPart), FullPath, t - FullPath);
}

PWSTR GetText715(LONG *pcchValid);
void f715(LONG cch, __out_ecount(cch) PWSTR pch)
{
	LONG cchValid;
	const WCHAR *pchRead;
	while( cch > 0 )                    // ESPX:26036 - addition of CFG_ENDSCOPE caused this to be hidden by the warning for line 148
	{
		pchRead = GetText715(&cchValid);
		if(!pchRead)					// No more text
			break;                      // ESPX:26036

		if (cchValid > 0 && cchValid <= cch)
			CopyMemory(pch, pchRead, cchValid*sizeof(WCHAR));

		pch += cchValid;
		cch -= cchValid;
    }
}

unsigned int f819(
    __in unsigned int i
)
{
    const unsigned int Indices[] = { 0, 1, 2, 3, 4, 5, 6 };
    return Indices[i]; // should get a warning about potential overflow
}

struct Struct869
{
    unsigned int m_size;
    __field_bcount(m_size) char *m_data;
};

void f869a(Struct869 st)
{
    // should not trigger a warning - has no effect for callers
    st.m_size++;
}

void f869b(Struct869 st)
{
    Struct869 mst = st;
    // should not trigger a warning - has no effect for callers
    mst.m_size++;
}

void f869c(Struct869& st1)
{
    // should trigger a warning - violates postcondition for callers
    st1.m_size++;
}

void f890(__inout_ecount(cb) BYTE*& pDest, __in_ecount(cb) const BYTE* pSrc, DWORD cb)
{
    // apparently there was a spuriouswarning here at one point....there should be none
    memcpy(pDest, pSrc, cb);
}

class CBuffer918
{
public:

    _At_(m_pb, __out_range(==, pv))  
    _At_(m_size, __out_range(==, size))  
    CBuffer918(__inout_bcount(size) void *pv, size_t size) : m_pb((unsigned char *)pv), m_size(size)  
    {  
    }  

    _At_(m_pb, __out_range(==, buf.m_pb))  
    _At_(m_size, __out_range(==, buf.m_size))  
    CBuffer918(__in const CBuffer918& buf) : m_pb(buf.m_pb), m_size(buf.m_size)  
    {  
    }
	
    _When_(offset <= m_size, _At_(return.m_pb, __out_range(==, m_pb + offset)))  
    _When_(offset <= m_size, _At_(return.m_size, __out_range(==, m_size - offset)))  
    _When_(offset > m_size, _At_(return.m_pb, __out_range(==, m_pb + m_size)))  
    _When_(offset > m_size, _At_(return.m_size, __out_range(==, 0)))  
    CBuffer918 Offset(size_t offset) const  
    {  
        if (offset > m_size)  
        {  
            offset = m_size;  
        }  
        return CBuffer918(m_pb + offset, m_size - offset);  
    }  
	
    _When_(index >= this->m_size, __out_range(==, 0))  
    _When_(index < this->m_size, __out_range(==, this->m_pb[index]))  
    unsigned char operator[](size_t index) const  
    {  
        if (index < m_size)  
        {  
            return m_pb[index];  
        }  
        else  
        {  
            return 0;  
        }  
    }  
  
  size_t m_size;  
  
protected:  
    
    __field_bcount(m_size) unsigned char * m_pb; 	
};

int Test918(CBuffer918& inBuffer)
{	
	if (inBuffer.m_size > 8)
	{	
		CBuffer918 buffer(inBuffer.Offset(4));
		// there should be no warnings anywhere here
		if (buffer.m_size > 1)
			buffer[0] == 'f';
	}	
}

typedef struct _MyStruct926
{
    char   DevicePath[10];
} MyStruct926, *MyStructPtr926;

__success(return == true)
_At_(data->DevicePath, __out_z)
bool f926(
__inout_bcount_part_opt(dataSize, *requiredSize) MyStructPtr926 data,
__in long dataSize,
__out_opt long* requiredSize)
{
    // previously caused PAG and so saw no warnings. Should generate two postcondition failure warnings
    data->DevicePath[0] = 'a';
    return true;
}

