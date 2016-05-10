#include "specstrings.h"
#include "mymemory.h"

char GetItem()
{
    static char ch;
    return ++ch;
}

void main()
{
    int cnt = 1;
    int itemcnt = 0;
    char *p = new char[cnt];
    if (p == nullptr)
        return;

    char item = GetItem();
    while (item)
    {
        if (itemcnt == cnt)
         {
            cnt *= 2;
            char *q = new char[cnt];
            if (q == nullptr)
                break;
            memcpy(q, p, itemcnt);
            p = q;
        }
        p[itemcnt++] = item;
        item = GetItem();
    }
}
