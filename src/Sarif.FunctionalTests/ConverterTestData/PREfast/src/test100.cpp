#include "specstrings.h"
#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

typedef struct _BUFFER {
    __bcount(fcb) void *fp;
    size_t fcb;
} BUFFER;

void good(__in_bcount(cb) void *p, size_t cb, __inout BUFFER *b)
{
    if (cb <= b->fcb)
    {
        memcpy(b->fp, p, cb);
    }
    else
    {
        b->fp = p;
        b->fcb = cb;
    }
}

void bad1(__in_bcount(cb) void *p, size_t cb, __inout BUFFER *b)
{
    memcpy(b->fp, p, cb);   // BAD. May Overflow. 26015. b->fp can be smaller than cb. [PFXFN] PREfix misses this. Not sure why...
}

void bad2(__in_bcount(cb) void *p, size_t cb, __inout BUFFER *b)
{
    b->fp = p;  // BAD. 26045. PREfix does not enforce this, but catches this if b is used per contract. See line 101.
}

struct Buffer
{
    __ecount(m_len) int *m_buf;
    int m_len;
   __range(0, m_len-1) int m_index;

    Buffer()
	: m_buf(0), m_len(0), m_index(-1)
    {}

    Buffer(__in_ecount(len) int *buf, int len)
    {
        m_buf = buf;
        m_len = len;
	m_index = 0;
    };

    Buffer(__in_ecount(100) int *buf)
    {
        m_buf = buf;
        m_len = 200;    // BAD. But ESPX does not report this. Wonder why...
	m_index = -2;   // BAD. 26061, 26030. Does not satisfy contract. PREfix won't enforce this, but can detect if Buffer is used per contract.   
    };

    int goodfetch(int index)
    {
        if (index >= 0 && index < m_len)
            return m_buf[index];
        else
            return 0;
    }

    int badfetch(int index)
    {
        return m_buf[index];    // BAD. Can underflow / overflow.
    }
};

int Bad1(__in Buffer *b)
{
    return b->m_buf[b->m_len];
}

int Good1(/* __in Buffer * */ Buffer const * b)
{
    return b->m_buf[b->m_index];
}

void Bad2()
{
    Buffer b;
    b.m_buf = new int[100];
    if (b.m_buf != nullptr)
    {
        b.m_len = 100 * sizeof(int);
        b.m_index = 0;
        Good1(&b);  // BAD for ESPX per contract. Because we say b.m_buf has 400 elements (instead of 400 bytes), we overflow b. OK for PREfix.

        delete[] b.m_buf;
    }
}

void main()
{
    BUFFER buf;
    buf.fcb = 10;
    buf.fp = new char[10];
    if (buf.fp)
    {
        char str20[20] = "my test string.";

        good(str20, 20, &buf);
        bad1(str20, 20, &buf);    // BAD. Should overflow. [PFXFN] PREfix misses this. Not sure why...
        char str5[5] = "test";
        bad2(str5, 5, &buf);
        ((char*)buf.fp)[buf.fcb - 1];    // Should be OK for ESPX per contract for BUFFER. Overflow for PREfix.
    }

    int int100[100] = {};
    Buffer buf1a(int100, 100);
    buf1a.m_buf[buf1a.m_index] = 1;     // OK
    buf1a.m_buf[buf1a.m_len - 1] = 1;   // OK
    buf1a.goodfetch(-1);    // OK
    buf1a.goodfetch(1000);  // OK
    buf1a.goodfetch(99);    // OK

    buf1a.badfetch(-1);    // BAD. Underflows.
    buf1a.badfetch(1000);  // BAD. Overflows.


    Buffer buf1b(int100);
    buf1b.m_buf[buf1b.m_index] = 1;     // OK for ESPX per contract. Underflows for PREfix.

    Bad1(&buf1a);   // BAD. Overflows.
    Good1(&buf1a);  // OK
}