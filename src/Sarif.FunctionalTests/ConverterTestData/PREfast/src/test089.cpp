#include "specstrings.h"
#include "mywin.h"
#include "mymemory.h"

typedef __nullterminated char *PSTR;

void congruent1(__in_ecount(n) PSTR *strings, size_t n)
{
    size_t needed = 1;
    for (size_t i = 0; i < n; i++)
    {
        if (strings[i][0] != 'a')
            needed++;
    }
    char **s = new char *[needed];
    if (s != nullptr)
    {
        needed = 0;
        for (size_t i = 0; i < n; i++)
        {
            if (strings[i][0] != 'a')
            {
                needed += strlen(strings[i]);
                s[needed++] = strings[i];   // BAD. May overflow. 'needed" grows faster than in the first for loop. [PFXFN] PREfix misses this. Not sure why.
            }
        }
        s[needed] = 0;

        delete[] s;
    }
}

struct Enum {
    int Next() { return _pos++;}
    bool AtEnd() { return _pos >= _count; }
    void Reset() { _pos = 0; }

    Enum(int count) : _count(count), _pos(0)
    { }

private:
    int _count;
    int _pos;
};

void congruent2(Enum *e)
{
    size_t needed = 0;
    // Note that we don't know where e is at.
    while (!e->AtEnd())
    {
        int i = e->Next();
        if (i > 0)
            needed++;
    }
    
    int *p = new int[needed];
    if (p != nullptr)
    {
        e->Reset();
        needed = 0;
        while (!e->AtEnd())
        {
            int i = e->Next();
            if (i > 0)
            {
                p[needed++] = i;    // BAD. May overflow. We may be iterating through more this time.
            }
        }

        delete[] p;
    }
}

__success(return)
bool congruent3(Enum *e, __out_ecount_full(n) int *p, size_t n)
{
    size_t needed = 0;
    // Note that we don't know where e is at.
    while (!e->AtEnd())
    {
        int i = e->Next();
        if (i > 0)
            needed++;
    }
    
    if (needed > n)
        return false;

    e->Reset();
    needed = 0;
    while (!e->AtEnd())
    {
        int i = e->Next();
        if (i > 0)
        {
            p[needed++] = i;    // BAD. May overflow. We may be iterating through more than n this time. [PFXFN] Not sure why...
        }
    }
    return true;
}

void notcongruent1(Enum *e)
{
    int size = 0;
    int cnt = 0;
    int *buf = 0;
    while (!e->AtEnd())
    {
        int i = e->Next();
        if (i > 0)
        {
            if (cnt == size)
            {
                size = size * 2 + 1;
                int *newbuf = new int[size];
                if (newbuf == nullptr)
                {
                    ++cnt;
                    continue;
                }

                if (buf)
                    memcpy(newbuf, buf, cnt);

                delete[] buf;
                buf = newbuf;
            }

            buf[cnt++] = i;
        }

        delete[] buf;   // This is wrong, but needed to trick PREfix for leaked memory.
    }
}

void notcongruent2(Enum *e)
{
    int size = 0;
    int cnt = 0;
    int *buf = 0;
    while (!e->AtEnd())
    {
        int i = e->Next();
        if (i > 0)
        {
            if (cnt == size)
            {
                size = size * 2 + 1;
                int *newbuf = new int[size];
                if (newbuf == nullptr)
                {
                    ++cnt;
                    continue;
                }

                if (buf)
                    memcpy(newbuf, buf, cnt);

                delete[] buf;
                buf = newbuf;
                buf[size] = 0; // BAD. Overflow. Simply wrong. [PFXFN] Not sure why...
            }

            buf[cnt++] = i;
        }

        delete[] buf;   // This is wrong, but needed to trick PREfix for leaked memory.
    }
}

void main()
{
    PSTR strings[5] = {"abc", "bcd", "bcd", "bcd", "bcd"};
    congruent1(strings, 5); // BAD. Should cause overflow. [PFXFN] PREfix misses this. Not sure why...

    Enum e(20);
    for (int i = 0; i < 10; ++i)
        e.Next();
    congruent2(&e); // BAD. Should cause overflow. [PFXFN] PREfix misses this. Not sure why...
    
    int p[10];
    e.Reset();
    for (int i = 0; i < 10; ++i)
        e.Next();
    congruent3(&e, p, 10);  // BAD. Should cause overflow. [PFXFN] PREfix misses this. Not sure why...

    e.Reset();
    notcongruent1(&e);  // OK

    e.Reset();
    notcongruent2(&e);  // BAD. Should cause overflow. [PFXFN] PREfix misses this. Not sure why...
}
