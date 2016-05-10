#include <specstrings.h>

struct Test
{
    Test();
    void SomeFunction();

    __field_ecount_part(capacity, length) char *rg;
    __field_range(0, capacity) unsigned int length;
    unsigned int capacity;
};

struct Derived : Test
{
    Derived();
    void Safe(__out_bcount(this->capacity) char *buf);
};

struct Derived2 : Derived
{
    Derived2();	
    void AlsoSafe(__out_bcount(this->capacity) char* buf);
};

extern "C" void memcpy(__out_bcount(size) void *dst, __in_bcount(size) const void *src, unsigned int size);

void Derived::Safe(__out_bcount(this->capacity) char *buf)
{
    memcpy(buf, this->rg, this->length);
}

void Derived2::AlsoSafe(__out_bcount(this->capacity) char* buf)
{
    // this should be completely equivalent to Safe() above
    for (unsigned int i = 0; i < this->length ; ++i)
    {
        buf[i] = this->rg[i];
    }
}

void Good()
{
    Test test;
    test.SomeFunction();
}

void Bad()
{
    Derived derived;
    derived.SomeFunction();
    
    Derived2 derived2;
    derived2.SomeFunction();
}