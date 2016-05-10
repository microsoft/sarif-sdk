#include "specstrings.h"
#include "mywin.h"
#include "undefsal.h"

UCHAR
TxfConvertTxfFileIdToMungedName (
    __in int * TxfFileId,
    __out_ecount(32) PWCHAR FileName
    )
{
    PWCHAR NextChar;
    UCHAR FileNameLength = (0x10);
    ULONG HalfID;
    ULONG Index, IndexBase;

    static const WCHAR HexTable[] = {L'0',L'1',L'2',L'3',L'4',L'5',L'6',L'7',L'8',L'9',L'A',L'B',L'C',L'D',L'E',L'F'};

    NextChar = &FileName[FileNameLength];

    Index = IndexBase = 0;

    while (Index < FileNameLength)  // Index will be 0, 8, and 16. FileNameLength is 16.
    {
        if (IndexBase == 0)
        {
            HalfID = (ULONG) *TxfFileId & 0xFFFFFFFF;

        }
        else
        {
            HalfID = (ULONG) (*TxfFileId >> 32) & 0xFFFFFFFF;
        }

        for (Index = IndexBase; Index < 8 + IndexBase; Index += 1)
        {
            NextChar -= 1;
            *NextChar = HexTable[ HalfID & 0xF ];   // [ESPXFP] ESPX Reports underflow, but that cannot happen.
            HalfID >>= 4;
        }
        IndexBase = Index;      // IndexBase and Index will be 8 and 16
    }

    return FileNameLength;
}

void g()
{
    char s[7 * 24 * 2];
    char *p = s;

    for (int day = 0; day < 7; day++)
    {
        for (int hour = 0; hour < 24; hour++)
        {
	    *p++ = day + hour;          // [ESPXFP] p++ happening in nested loop confuses ESPX
	    *p++ = (day + hour) * 2;    // [ESPXFP] p++ happening in nested loop confuses ESPX
        }
    }
}

void f()
{
    int j = 0;
    int n = 2;
    int a[20];
    for (int i = 0; i < 10; i++)
    {
        a[j] = 1;
        j += n;
    }
}

char gstr[20];
char *returnchar()
{
    unsigned char i;
    static unsigned char start = 0;
    if (start > 19)
        start = 19;
    for (i = start; i < 19; ++i)
        gstr[i] = 'a' + i % 26;
    gstr[i] = '\0';
    return gstr;
}

void foo(__out_ecount(n) int *a, int n)
{
    int i = 0;

    int *p = a;
    while (i++ < n) {
        *p++ = 1;               // OK. p increments only by 1 in while loop.
         char *s = returnchar();
         while (*s++)
            *s = ' ';
    }
}

struct S1 {
    int a, b, c, d;
};

struct S {
    char a[20];
    S1 b;
    S1 c;
    S1 s;
};

void bar(__in int z)
{
    // dummy
}

void baz(__in_ecount(n) S *p, size_t n)
{
    while (n--)
    {
        bar(p->b.c);
        p++;
    }
}

size_t read()
{
    static size_t size;
    return size;
}

// ESP Bug #381: Fixed. Should get no warning for foo.
void foo()
{
    unsigned short buff[257];
    size_t n = read();
    if (n > sizeof(buff) / sizeof(buff[0]))
    {
        return;
    }
    size_t i = 0;
    size_t bits;
    while(i < n) {
        switch(read()) {
        case 0:
            buff[i++] = 0;
            break;
        case 1:
            bits = read();
            if (bits > n ||
                3 > (n - bits) ||
                i > (n - (3 + bits)))
            {
                return;
            }      
            bits = bits + 3 + i;
            for(; i < bits; ++i)
            {
                buff[i] = 0;
            } 
            break;
        default:
            buff[i++] = 0;
        }
    }
}

void main()
{
    int id = 5;
    wchar_t fn[32];
    UCHAR ch = TxfConvertTxfFileIdToMungedName(&id, fn);    // OK

    int a[100];
    foo(a, 100);    // OK

    S s[10];
    S* ps = s;
    for (int i = 0; i < 10; ++i)
    {
        ps->b.c = i + 1;
        ++ps;
    }
    baz(s, 10);     // OK
}
