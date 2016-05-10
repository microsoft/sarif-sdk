// This test case adds patterns that triggered PAGs to EspX test suite.
// On a future compiler drop, these PAGs should be easy to detect.

class A
{
public:
    ~A();
};

void Test1()
{
    new A(A());
}

void Test2(int n)
{
    switch (n)
    {
    case 1 <= 0 ? 1 : 0 : break;
    }
}

int Test3()
{
    char blah[] = "";
    return 0;
}

class Top
{
public:
    virtual ~Top() {};
};

class Left : public virtual Top
{
public:
    ~Left() {};
};

void Test4()
{
    Left* left = new Left();
}

#define FIELD_OFFSET(type, field)    ((int)(char *)&(((type *)0)->field))

// note that the offset of field b = 4
typedef struct { int a; int b; } t;

void Test5()
{
  switch(0)
  {
    case (FIELD_OFFSET(t, a)): ;
    case (FIELD_OFFSET(t, b)): ;
  }
}

class myclass {
public:
    void mymethod1() {};
    void mymethod2() {};
};

// Define a type which is a pointer to a non-static member function of the 'myclass'class
typedef void (myclass::*mytypedef)( );

void Test6() {
    mytypedef myfunctionpointer = &myclass::mymethod2;

    myclass x;

    (x.*(myfunctionpointer))();
}

struct C {
        ~C();
};

void Test7()
{
    C c = {};
}

class D
{
public:
        ~D();
        D(int);
};

void Test8(int* p)
{
    D v[] = { *p };
}

struct __declspec(dllimport) E 
{
    virtual ~E();
};

void Test9() 
{
    new E[1];
}

struct F 
{
    ~F();
};

void Test10() 
{
    F a[1];
}

enum 
{
    G
};

void Test11() 
{
    throw G;
}

typedef unsigned short wchar_t;

void Test12(wchar_t*, int)
{
	wchar_t s[1];
	Test12(s, reinterpret_cast<int>(const_cast<wchar_t *>(L" ")));
}

typedef
    __declspec("SAL_name" "(" "\"_Null_terminated_\"" "," "\"\"" "," "\"2\"" ")"
)
    __declspec("SAL_begin")
    __declspec("SAL_nullTerminated" "(" "__yes" ")")
    __declspec("SAL_end")
    char *T;

struct S {
    T t;
    void g(long, S);
    void f(long v)
    {
        auto m = &S::g;
        struct S s = {};
        (this->*m)(v, s);
    }
};

struct S1 {
    operator int();
};

void Test13_g(
    void *p,
    __declspec("SAL_when((int)s)")
    __declspec("SAL_begin")
        __declspec("SAL_name" "(" "\"_Pre_satisfies_\"" "," "\"\"" "," "\"2\"" ")")
        __declspec("SAL_begin") 
            __declspec("SAL_pre")
            __declspec("SAL_satisfies" "(" "1" ")" )
        __declspec("SAL_end")
    __declspec("SAL_end") 
    S1 s);

void Test13() {
    S1 s = {};
    Test13_g(0, s);
}

void Test14() {
    try
    {
    }
    catch (int*)
    {
    }
}
