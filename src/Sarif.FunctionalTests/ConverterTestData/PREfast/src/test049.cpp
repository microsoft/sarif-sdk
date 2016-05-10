#include "specstrings.h"
#include "undefsal.h"

class Base {
    int a;
public:
    virtual bool Compare(__in const Base *other);
};

bool Base::Compare(__in const Base *other)
{
    return a == other->a;
}

class Derived : public Base {
    int b;
public:
    virtual bool Compare(__in_bcount(sizeof(Derived)) const Base *other);
};

bool Derived::Compare(__in_bcount(sizeof(Derived)) const Base *other)
{
    if (Base::Compare(other)) {
        return true;
    }
    return b == ((Derived *)other)->b;
}

void test_b_a(__in_ecount(c->a) char *buf, const Base *c)
{
    buf[100]=0;
}

void test_d_b(__in_ecount(c->b) char *buf, const Derived *c)
{
    buf[100]=0;
}


void test_d_a(__in_ecount(c->a) char *buf, const Derived *c)
{
    buf[100]=0;
}

void main()
{
    // With the implementations of the above test_* functions,
    // the second paramaters to all of the above functions are meaningless for PREfix
    // as they are not used.
    char a100[100];
    char a101[101];

    test_b_a(a100, nullptr);    // Bad!
    test_b_a(a101, nullptr);    // OK.
}