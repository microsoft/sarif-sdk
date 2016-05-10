#include <specstrings.h>
#include "undefsal.h"

_At_buffer_(dest, _I_, count, _Post_equal_to_(c))
void *mymemset(__out_bcount(count) void *dest, int c, size_t count);

struct MyStruct
{
    int x_;
    int y_;
};

struct MyBiggerStruct
{
    int a_;
    MyStruct ms_;
    int b_;
};

_At_((char*)p, _Null_terminated_)  //ESP:948  if you have a redundant (but valid) typecast in an annotation, then it elicits a COM exception from the ICType objects that represent the type being cast to/from.  Unfortunately, this silently kills EspX analysis for that function
void foo(_In_ char* p)				//Expected error in this function was garnered from bo-test160.cpp.  With ESP:948 fixed, the error should be reported just as it would be in bo-test160.cpp.  If ESP:948 is NOT fixed, no errors will be reported
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
