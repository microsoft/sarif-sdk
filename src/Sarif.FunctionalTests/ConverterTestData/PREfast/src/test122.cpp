#include <specstrings.h>
#include "specstrings_new.h"
#include "mymemory.h"
#include "undefsal.h"

class List
{
    public:
        List()
            : m_dwSize(0), m_dwCount(0), m_buffer(nullptr)
        {
        }

        // There should be no warnings in this method
        void Add(char c)
        {
            if (m_dwSize == m_dwCount)
                Resize();
            m_buffer[m_dwCount++] = c;
        }
    private:
        _At_(this->m_dwCount, __out_range(<, this->m_dwSize))
        void Resize()
        {
            if (m_dwSize > m_dwCount)
                return;

            char* newBuf = (char*)realloc(m_buffer, ++m_dwSize);
            if (newBuf == nullptr)
            {
                m_dwSize = 0;
                m_dwCount = 0;
                free(m_buffer);
                throw "sorry";
            }
            m_buffer = newBuf;
        }

        unsigned int m_dwSize;
        unsigned int m_dwCount;
        __field_bcount_part(m_dwSize, m_dwCount) char *m_buffer;
};

typedef struct {
    unsigned long cBuffers;
    __field_ecount(cBuffers) int *pBuffers;
} BufferDesc;

// There should be no warnings in this function
void safe1(__inout_opt BufferDesc *pBD)
{
    unsigned long i;
    BufferDesc EmptyBuffer;

    if (pBD == 0)
    {
        pBD = &EmptyBuffer;
        memset(pBD, 0, sizeof(BufferDesc));
    }

    for (i = 0; i < pBD->cBuffers; i++) // NOTE: In dev12, we get 26003 for this line also saying possible postcondition violation
        pBD->pBuffers[i] = 0;           // However, if any line is added after this for loop, above warning moves to this line, without saying possible postcondition violation.
}

void Foo(__inout BufferDesc *pBD) { /* let's leave it as is */ }

void unsafe(__inout BufferDesc *pBD)
{
    unsigned long i;

    Foo(pBD);

    for (i = 0; i <= pBD->cBuffers; i++)
        pBD->pBuffers[i] = 0;   // BAD. Overflows. ESPX:26014 / PFX:25
}

#if 0    // don't turn this test on yet; it's still failing

// There should be no warnings in this function; assume that static variables
// are valid at entry.
static BufferDesc sEmptyBuffer;
void safe2(__inout_opt BufferDesc *pBD)
{
    unsigned long i;

    if (pBD == 0)
        pBD = &sEmptyBuffer;

    for (i = 0; i < pBD->cBuffers; i++)
        pBD->pBuffers[i] = 0;
}

#endif

void main()
{
    List l;
    try
    {
        for (int i = 0; i < 10; ++i)
            l.Add('a' + i);
    }
    catch(...)
    {
    }

    int buf[10];
    BufferDesc bd;
    bd.pBuffers = buf;
    bd.cBuffers = 10;
    unsafe(&bd);    // BAD. See unsafe()
}