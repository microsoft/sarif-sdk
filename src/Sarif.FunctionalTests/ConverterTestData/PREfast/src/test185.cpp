#include <sal.h>
#include "undefsal.h"

typedef _Writable_bytes_(6) char *PDOT11_ADDR;
typedef _Writable_bytes_(6) char DOT11_ADDR[6];


//
//Test1:
// Test to ensure that espx picks up buffer size annotations on typedefs
// of array types as formal parameters.
//

void OutCallee1(_Out_ DOT11_ADDR input)
{
    for (int i = 0; i < 6; i++)
        input[i] = 0;  // no expected buffer overrun here
}

void OutCaller1()
{
    char c = 0;
    OutCallee1(&c);  // expected buffer overrun here
}


void InoutCallee1(_Inout_ DOT11_ADDR input)
{
    for (int i = 0; i < 6; i++)
        input[i] = 0;  // no expected buffer overrun here
}

void InoutCaller1()
{
    DOT11_ADDR myaddr = {0};
    char c = 0;
    InoutCallee1(&c);  // expected buffer overrun here
}


//
//Test2:
// Test to ensure that espx picks up buffer size annotations on typedefs
// of pointer types as formal parameters.
//

void OutCallee2(_Out_ PDOT11_ADDR input)
{
    for (int i = 0; i < 6; i++)
        input[i] = 0;
}

void OutCaller2()
{
    char c = 0;
    OutCallee2(&c);  // expected buffer overrun here
}


void InoutCallee2(_Inout_ PDOT11_ADDR input)
{
    for (int i = 0; i < 6; i++)
        input[i] = 0;
}

void InoutCaller2()
{
    DOT11_ADDR myaddr = {0};
    char c = 0;
    InoutCallee2(&c);  // expected buffer overrun here
}

