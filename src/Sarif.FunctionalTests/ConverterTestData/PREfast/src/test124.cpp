#include "specstrings.h"

typedef __nullterminated char *LPSTR;

class DeserializeContext
{
public:
    void Foo1(LPSTR s1, LPSTR s2);
    void Foo2(LPSTR s1, LPSTR s2);

    int m_i;
};

class Handler
{
public:
    typedef void (DeserializeContext::*FooFunc)(LPSTR, LPSTR);

    FooFunc m_func;

    void Convert(DeserializeContext *, FooFunc) {}

    // When recursively collecting annotates for pDC (via its type),
    // espx was crashing when it followed the pointer-to-member-func type,
    // and thus thinking there were two formals, and tried to put type annos
    // on both.  The correct behavior here is: don't crash.
    void MyFunc(DeserializeContext *pDC)
    {
        Convert(pDC, m_func);
    }
};

void main()
{
    DeserializeContext dc;
    Handler h;
    h.MyFunc(&dc);  // OK.
}