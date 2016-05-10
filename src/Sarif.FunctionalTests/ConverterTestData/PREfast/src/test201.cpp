#include <specstrings.h>
#include "undefsal.h"

typedef unsigned int DWORD;

class MyClass
{
public:
    MyClass(DWORD size) : m_Size(size) { m_Values = new int[size]; }
    DWORD GetSize() const { return m_Size; }

    int GetValue(DWORD index)
    {
        return m_Values[index]; // should give 26020 - incorrect validation of buffer access
    }

    int GoodGetValue(_In_ _In_range_(<, m_Size) DWORD index)
    {
        return m_Values[index]; // safe by annotation
    }

    int BestGetValue(DWORD index)
    {
        if (index < m_Size)
            return m_Values[index]; // really safe 
        else
            return -1;
    }

private:
    DWORD m_Size;
    _Field_size_(m_Size) int* m_Values;
};

class MyFixedSizeClass
{
public:
    int GetValue(DWORD index)
    {
        return m_Values[index]; // should give 26017 - low-pri incorrect validation of buffer access
    }

private:
    int m_Values[10];
};

