#include <specstrings.h>

void foo(void)
{
    size_t alloced = 1;
    size_t off = 0;
    char * buf = new char[alloced];
    while(1) {
        size_t need = off + 1;
        if(alloced < need) 
        {
            buf = new char[need];
            alloced = need;
        } 
        buf[off] = 1; // should give no warning, safe because of alloced
        off+=1;
    }

}

