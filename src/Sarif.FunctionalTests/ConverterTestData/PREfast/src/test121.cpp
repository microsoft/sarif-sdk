#include <specstrings.h>
#include "specstrings_new.h"
#include "mymemory.h"
#include "undefsal.h"

typedef unsigned int DWORD;

typedef __struct_bcount(cbStruct) struct tagBIND_OPTS {
    DWORD           cbStruct;
    DWORD           grfFlags;
    DWORD           grfMode;
    DWORD           dwTickCountDeadline;
} BIND_OPTS;

int Foo(__inout BIND_OPTS *pBO)
{
    if (pBO)
    {
        pBO->grfMode = pBO->grfFlags;
    }
    return 0;
};

void Good1()
{
    BIND_OPTS bo = {sizeof(bo), 0};
    Foo(&bo);   // Not very interesting... What are we testing here?
    Foo(&bo);   // Not very interesting... What are we testing here?
}

struct Data {
    int *pData;
    int i;
    int j;
};

void Bad1(__inout_ecount(n) int *data, int n)
{
    Data d = { data, n };   // d.i = n, d.j = 0

    if (n <= 0)
        return;

    d.pData[d.j] = 10;  // OK. this reference should be safe
    d.pData[d.i] = 10;  // BAD. this reference is not safe. overflows. ESPX:26000 [PFXFN] PREfix does not detect this. NOTE: It also misses data[d.i].
}

extern Data GetData() { static Data d; return d; }

struct DataWrap
{
    int k;
    Data data;
    int *myData;
};


__ecount(10)
    int *Bad2()    // this was causing cfgbuilder to crash
{
    static int buffer[2];
    DataWrap dw = { 2, GetData(), buffer };

    return dw.myData;  // expected postcondition failure. ESPX:26040
}

// test returnUDT member functions
struct SizedData
{
    size_t numElems_;
    __field_ecount(numElems_) char *elems_;
};

struct DataClass
{
    SizedData data_;
};

struct DataGetter
{
    SizedData getData()
    {
        static SizedData sd;
        sd.elems_ = (char*)malloc(10);
        sd.numElems_ = sd.elems_ ? 10 : 0;
        return sd; 
    }
};

char Bad3(DataGetter *pDG)
{
    DataClass dc = { pDG->getData() };
    if (dc.data_.numElems_ == 0)
        return 0;

    dc.data_.elems_[dc.data_.numElems_ - 1] = 'X';  // safe. [ESPXFP] ESPX reports 26017, saying dc.DataClass::data_.SizedData::numElems_`78 is not constrained by (result.getData-&gt;SizedData::numElems_)`77
    return dc.data_.elems_[dc.data_.numElems_];     // unsafe
}


// Using _At_(p->m, _Const_) should leave p->m unchanged across call
_At_(pBO->cbStruct, _Const_)
    void BindOptsUser(__inout BIND_OPTS *pBO)
{
    // Things in pBO can change except cbStruct
    if (pBO)
    {
        pBO->grfMode = ++(pBO->grfFlags);
        --(pBO->dwTickCountDeadline);
    }
}

_At_(pBO->cbStruct, __in_range(>=, sizeof(struct tagBIND_OPTS)))
    void ModifyBindOpts(__inout BIND_OPTS *pBO)
{
    DWORD size = pBO->cbStruct;
    BindOptsUser(pBO);
    if (size != pBO->cbStruct)  // never true due to SAL on BindOptsUser
    {
        int x[1];
        x[1] = 5;   // force buffer overrun if this path is taken
    }
    memset(pBO, 0, size);
    pBO->cbStruct = size;
}

// const fields don't change at call sites
struct SizedData2
{
    SizedData2(size_t count)
        :  numElems_(count)
    {
        elems_ = (char*)malloc(count);
        if (!elems_)
            throw "sorry";
    }

    const size_t numElems_;
    __field_ecount(numElems_) char *elems_;
};

void ModifySizedData2(__inout SizedData2 *pSD) { /* can't change size of elems_ */ }

void TestConstField(SizedData2 *pSD)
{
    size_t numElems = pSD->numElems_;
    if (numElems == 0)
        return;
    ModifySizedData2(pSD);
    pSD->elems_[numElems - 1] = 0;  // OK
}


// Inheritance and BIND_OPTS
#if 0
// Test currently disabled; failing due to Esp:623
struct BIND_OPTS2 : BIND_OPTS
{
    int m_extra;
};

void GoodInheritance1()
{
    BIND_OPTS2 bo;
    bo.cbStruct = sizeof(bo);
    Foo(&bo);
}
#endif

void main()
{
    int buf[10];
    int size = 10;
    Bad1(buf, size);    // BAD. Should cause overflow. [PFXFN] See Bad1.

    int* buf10 = Bad2(); // Bad2 promises to return 10-elem buffer by contract.
    buf10[9] = 1; // Should be fine, but overflows. Bad2 returns 2-elem buffer. [PFXFN] PREfix misses this...

    DataGetter dg;
    Bad3(&dg);      // BAD. will cause overflow. [PFXFN] PREfix misses this.

    BIND_OPTS bo = {(DWORD)sizeof(BIND_OPTS)};
    ModifyBindOpts(&bo);    // OK

    try
    {
        SizedData2 sd(10);
        TestConstField(&sd);    // OK
    }
    catch(...)
    {
    }
}