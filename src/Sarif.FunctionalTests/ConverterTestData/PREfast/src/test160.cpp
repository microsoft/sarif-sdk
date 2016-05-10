#include <specstrings.h>
#include "specstrings_new.h"


_At_buffer_(dest, _I_, count, _Post_equal_to_(c))
void *mymemset(__out_bcount(count) void *dest, int c, size_t count);

_At_buffer_(dest, _I_, count, _Post_equal_to_(0))
void *myzeroints(__out_ecount(count) int *dest, size_t count);

extern "C"
_Post_equal_to_(_String_length_(p)) unsigned int
strlen(_In_z_ const char *p);


struct MyStruct
{
    int x_;
    int y_;
};

//
// For ease of telling expected vs. unexpected buffer overrun warnings,
// a[10] is unexpected, and a[11] is expected.
//


void Test1()
{
    MyStruct ms;
    int a[10];

    mymemset((void *)&ms, 0, sizeof(ms));

    if (ms.x_ != ms.y_)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (ms.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}


struct MyBiggerStruct
{
    int a_;
    MyStruct ms_;
    int b_;
};

void Test2()
{
    MyBiggerStruct mg;
    int a[10];

    mg.a_ = 5;
    mg.b_ = 6;
    mymemset((void *)&mg.ms_, 0, sizeof(struct MyStruct));

    if (mg.a_ != 5)
        a[10] = 6;          // not reachable; should not report buffer overrun
    if (mg.a_ != mg.b_ - 1)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (mg.ms_.x_ != mg.ms_.y_)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (mg.ms_.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}


void Test3()
{
    MyStruct ms;
    int a[10];

    ms.x_ = 1;
    mymemset((void *)&ms.y_, 0, sizeof(int));

    if (ms.x_ != ms.y_ + 1)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (ms.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}

struct MyBitfieldStruct
{
    int w_;
    int x_ : 6;
    int y_ : 6;
};

void Test4()
{
    MyBitfieldStruct ms;
    int a[10];

    mymemset((void *)&ms, 0, sizeof(ms));

    if (ms.x_ != ms.y_)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (ms.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}

void Test5()
{
    MyStruct ms;
    int a[10];

    myzeroints((int *)&ms, 2);

    if (ms.x_ != ms.y_)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (ms.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}

struct MyArrayStruct
{
    int n_;
    char mystring_[100];
};

unsigned int Test6()
{
    MyArrayStruct ms;

    mymemset((void *)&ms, 0, sizeof(ms));

    return strlen(ms.mystring_);    // is zero-terminated string due to mymemset
}


//
// Tests for using _Post_satisfies_ rather than _range_
//
_At_buffer_(dest, _I_, count, _Post_satisfies_(((char *)dest)[_I_] == c))
void *mymemset2(__out_bcount(count) void *dest, int c, size_t count);

_At_buffer_(dest, _I_, count, _Post_satisfies_(0 == dest[_I_]))
void *myzeroints2(__out_ecount(count) int *dest, size_t count);

void Test2_1()
{
    MyStruct ms;
    int a[10];

    mymemset2((void *)&ms, 0, sizeof(ms));

    if (ms.x_ != ms.y_)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (ms.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}

void Test2_5()
{
    MyStruct ms;
    int a[10];

    myzeroints2((int *)&ms, 2);

    if (ms.x_ != ms.y_)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (ms.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}

//
//
// Tests for using _Post_satisfies_ rather than _range_
//
struct MyPtrStruct
{
    int *x_;
    int *y_;
};


void Test3_1()
{
    MyPtrStruct ms;
    int a[10];

    mymemset((void *)&ms, 0, sizeof(ms));

    if (ms.x_ != ms.y_)
        a[10] = 6;          // not reachable; should not report buffer overrun

    if (ms.y_ == 0)
        a[11] = 1;          // should report buffer overrun
}


