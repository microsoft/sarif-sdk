
struct MyStruct { int a; int b; int c; };

void foo()
{
    union MyUnion
    {
        MyStruct* s;
        unsigned char* c;
    };

    MyUnion Local;

    // this is safe, but was reporting as a false positive
    // 26000 when the buffer for Local.s was returned as 
    // the buffer for Local.s->c
    Local.s->c = 12;

    // force EspX to inspect the function
    char a[10];
    a[0] = 0;
}

