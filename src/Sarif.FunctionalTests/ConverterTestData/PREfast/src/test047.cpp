#include "specstrings.h"

typedef short CODEPAGE;
typedef bool BOOL;

struct ENCODINGWRITEFUNC
{
    CODEPAGE cp;
    void *s;
};

BOOL check()
{
    static int i;
    return ((int)&i % 2) == 1;
}

#define ARRAY_SIZE(a) (sizeof(a)/sizeof(*a))

void Fumble(CODEPAGE cp, BOOL *pfDifferentEncoding, BOOL fNeedRestart )
{
    BOOL fSuccess = false, fSwitched;
    void *v;    
    static const struct ENCODINGWRITEFUNC aEncodingFuncs[] =
    {
        { 1252,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 8859,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 1254,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 1257,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
    };

    const struct ENCODINGWRITEFUNC * p = aEncodingFuncs;
    const struct ENCODINGWRITEFUNC * pStop = aEncodingFuncs + ARRAY_SIZE(aEncodingFuncs);

    // See if we can handle this codepage natively.
    for (;
         p < pStop;
         p++)
    {
        if (cp == p->cp)
            break;
    }

    fSuccess = (p < pStop);

    // If we cannot handle this codepage natively, hand it over to mlang
    if (!fSuccess)
    {
        fSuccess = check();
    }

    if (fSuccess)
    {
        if (p == pStop)  //this does not work currently, p >= pStop works.
        {
            v = 0;
        }
        else
        {
            v = p->s;
        }
    }
}


void Fumble2(CODEPAGE cp, BOOL *pfDifferentEncoding, BOOL fNeedRestart )
{
    BOOL fSuccess = false, fSwitched;
    void *v;    
    static const struct ENCODINGWRITEFUNC aEncodingFuncs[] =
    {
        { 1252,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 8859,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 1254,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 1257,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
    };

    // See if we can handle this codepage natively.
    int i = 0;
    int N = ARRAY_SIZE(aEncodingFuncs);
    for (;
         i < N;
         i++)
    {
        if (cp == aEncodingFuncs[i].cp)
            break;
    }

    fSuccess = (i < N);

    // If we cannot handle this codepage natively, hand it over to mlang
    if (!fSuccess)
    {
        fSuccess = check();
    }

    if (fSuccess)
    {
        if (i == N)
        {
            v = 0;
        }
        else
        {
            v = aEncodingFuncs[i].s;
        }
    }
}

 

void Fumble3(CODEPAGE cp, BOOL *pfDifferentEncoding, BOOL fNeedRestart )
{
    BOOL fSuccess = false, fSwitched;
    void *v;    
    static const struct ENCODINGWRITEFUNC aEncodingFuncs[] =
    {
        { 1252,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 8859,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 1254,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
        { 1257,   "&CEncodeWriter::MultiByteFromWideCharGeneric" },
    };

    const struct ENCODINGWRITEFUNC * p = aEncodingFuncs;
    const struct ENCODINGWRITEFUNC * pStop = aEncodingFuncs + ARRAY_SIZE(aEncodingFuncs);

    // See if we can handle this codepage natively.
    for (;
         p < pStop;
         p++)
    {
        if (cp == p->cp)
            break;
    }

    if (p < pStop)
      fSuccess = true;
    if (p>pStop)
      return;

    // If we cannot handle this codepage natively, hand it over to mlang
    if (!fSuccess)
    {
        fSuccess = check();
    }

    if (fSuccess)
    {
        if (p == pStop)
        {
            v = 0;
        }
        else
        {
            v = p->s;
        }
    }
}

struct Node {
        int data;
        Node *next;
};

Node *head;

void sort(__inout_ecount(size) int *a, size_t size)
{
    // Just mimic array access per annotation
    for (size_t i = 0; i < size; ++i)
        a[i] = a[i] + 1;
}

void SortList()
{
    int a[100];
    size_t cnt = 0;
    for (Node *p = head; p; p = p->next)
    {
        if (p->data)
            a[cnt++] = p->data; // BAD. This can overflow. [PFXFN] PREfix does not warn this.
    }
    sort(a, cnt);               // BAD. This can also overflow. [PFXFN] PREfix does not warn this.
}

void SortListUnroll()
{
    int a[10];
    int i = 0;
    Node *p = head;
    if (p)
    {
        p = p->next;
    }
    if (p)
    {
        i = 10;
        p = p->next;
    }

    if (!p)
        a[i] = 0;   // This can overflow, if i == 10, set above.
}

Node* BuildList(int& count, int data)
{
    if (count < 1)
        return nullptr;

    Node* hn;
    Node* cn;

    cn = hn = new Node();
    if (cn == nullptr)
        return nullptr;

    cn->data = data;

    int i = 1;
    while (i < count)
    {
        cn->next = new Node();
        cn = cn->next;
        if (cn == nullptr)
            break;
        cn->data = data;
        ++i;
    }

    if (cn != nullptr)
        cn->next = nullptr;

    count = i;
    return hn;
}

void main()
{
    int count = 120;
    head = BuildList(count, 1); // This doesn't really help PREfix. This doesn't change output for below call.
    if (head != nullptr && count > 100)
        SortList();     // [PFXFN] This should cause overflow, but PREfix seems unable to detect this.
}