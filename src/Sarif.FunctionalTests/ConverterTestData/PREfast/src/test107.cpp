#include "specstrings.h"

template<typename S>
__range(lo, hi) S MakeInRange(S lo,S hi,S v) 
{ 
    /* swap if lo is not really lo */
    if((lo <= v) && (v <= hi)) return v;
    throw "bad acces";
}

template<typename S,typename I>
S& SafeIdx(__in_ecount(len) S arr[],I len, I idx) 
{
    I sidx = MakeInRange<I>(0,len-1,idx);
    return arr[sidx];
}

template<typename T>
class SafeArray
{
public:
    SafeArray(int len_,__in_ecount(len_) T* v_) : len(len_), v(v_) 
        {  }
    T& operator[](int idx)
    {
        return SafeIdx(v,len,idx);
    }
 private:
    int len;
    __field_ecount(len) T* v;
};

template<size_t min,size_t max>
class Range
{
    enum { MIN = min, MAX = max };
 public:
    Range() : val(MIN) {};
    void Set(__range(MIN,MAX) size_t x)
    {
        if(x < MIN) throw "bad";
        if(x > MAX) throw "bad";
        val = x;
    }
    __range(MIN,MAX) size_t Get() 
    {
        // this should not be flagged but currently is
        // due to a failure to get annotations on fields of template types 
        // from NMM (DevDiv:500778)
        return val; 
    }
 private:
    __field_range(MIN,MAX) size_t val;
};

void main() 
{
    try
    {
        char buf[128];
        char cidx = 255;
        SafeIdx<char,char>(buf,128,cidx);
        int ibuf[1024];
        size_t idx = 100000;
        SafeIdx<int,size_t>(ibuf,1024,idx);

        SafeArray<char> sbuf(128,buf);
        sbuf[100000];
    }
    catch(...)
    {
        // ignore errors.
    }

    Range<100,200> r1;
    Range<150,200> r2;
    Range<160,170> r3;
    r1.Set(10); // BAD. Out of range. ESPX:26070/PFX:89(exception)
    r1.Set(100); // OK
    size_t v1 = r1.Get();
    r2.Set(v1); // BAD. r1.Get returns value between 100 and 200. r2.Set expects value between 150 and 200. NOTE: PREfix does not repeat warning 89 here. Not a good idea...
    size_t v2 = r3.Get();
    r2.Set(v2); // OK.
}
