#include <specstrings.h>

enum MyEnum : unsigned char
{
    ValueZero,
    ValueOne,
    ValueTwo
};

MyEnum GetValue(
    _In_reads_bytes_(num) MyEnum *myEnums,
    unsigned int num,
    unsigned int which
    )
{
    if (which >= num)
        return ValueZero;

    return myEnums[which];
}

