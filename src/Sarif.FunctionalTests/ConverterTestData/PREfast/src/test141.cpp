#include "specstrings.h"
#include "undefsal.h"

//
// Test case for Esp:730 -
//   Correct handling of field annotations and reference parameters
//

typedef unsigned long ULONG;
typedef unsigned char UCHAR;

extern "C" void memset(__out_bcount(count) void *dst, int val, size_t count);
extern "C" __bcount(size) void *malloc(size_t size);

typedef struct _MYSTRUCT {
   ULONG something;
   ULONG Length;
    __field_bcount_opt(Length) UCHAR Data[1];
} MYSTRUCT, *PMYSTRUCT;

template <class myType>
class MyClass {
public:
    void MyFunc(myType &pobject);
};

#if 0
template <class myType>
void MyClass<myType>::MyFunc(myType &pobject)
{
    myType obj1;
}
#endif


void NoErrorTestFunc()
{
    PMYSTRUCT pObj1;
    MyClass<MYSTRUCT *> myclassobj;

    pObj1 = (PMYSTRUCT)malloc(20);
    pObj1->Length = 12;

    myclassobj.MyFunc(pObj1);
}

void ErrorTestFunc()
{
    PMYSTRUCT pObj1;
    MyClass<MYSTRUCT *> myclassobj;

    pObj1 = (PMYSTRUCT)malloc(20);
    pObj1->Length = 20;

    myclassobj.MyFunc(pObj1);   // expecting warning here
}


template <class myType>
class MyClass2
{
public:
    void RefParams(
        __out int& size,
        _Outref_result_buffer_(size) myType *& ptr
        );
};

template <class myType>
void MyClass2<myType>::RefParams(
    __out int& size,
    _Outref_result_buffer_(size) myType*& ptr
    )
{
    size = 20;
    ptr = (char *)malloc(size * sizeof(myType));
}

void UseRefParamsFromMyClass2()
{
    MyClass2<char> myclass2obj;
    int size;
    char *cp;

    myclass2obj.RefParams(size, cp);
    cp[size-1] = 0;      // no warning expected here
    cp[size] = 0;        // yes warning expected here
}

void RefParamsNoTemplate(
    __out int& size,
    _Outref_result_buffer_(size) char*& ptr
    )
{
    size = 20;
    ptr = (char *)malloc(size);
}

void UseRefParamsNoTemplate()
{
    int size;
    char *cp;

    RefParamsNoTemplate(size, cp);
    cp[size-1] = 0;      // no warning expected here
    cp[size] = 0;        // yes warning expected here
}

