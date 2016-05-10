#include <specstrings.h>
#include "undefsal.h"

//
// No buffer overrun warnings expected in either function
//

#define SIZE 10

class MyClass
{
public:
    void InitByAddr();
    void InitByRef();
    bool SetValue(unsigned int i);

private:
    // typedef void *MyDataType;
    typedef int MyDataType;

    void InitOneItemByAddr(__out MyDataType *pd);
    void InitOneItemByRef(__out MyDataType& d);

    MyDataType m_data[SIZE];
};

void MyClass::InitByAddr()
{
    InitOneItemByAddr(&m_data[0]);
    InitOneItemByAddr(&m_data[1]);
}

void MyClass::InitByRef()
{
    InitOneItemByRef(m_data[0]);
    InitOneItemByRef(m_data[1]);
}

bool MyClass::SetValue(unsigned int i)
{
    if (i >= SIZE)
        return false;
    InitOneItemByAddr(&m_data[i]);
    return (m_data[i] != 0);
}

