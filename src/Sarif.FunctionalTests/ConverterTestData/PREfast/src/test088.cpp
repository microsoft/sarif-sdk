#include "mywin.h"
#include "mymemory.h"
#include "undefsal.h"

void f(__nullterminated char *p)
{
    if (p == nullptr)
        return;

    char ch = 'a';
    while (*p)
        ch += *(p++);
}

void f(/* __in PWSTR */ _Null_terminated_ WCHAR const * p)
{
    if (p == nullptr)
        return;

    char ch = 'a';
    while (*p)
        ch += *p++;
}

void g(__in char *q)
{
    f(q);   // BAD. q is not guaranteed to be null-terminated. [PFXFN] PREfix does not handle null-terminated strings well.
}

HRESULT Fill(PSTR s, unsigned int n)
{
    // Signature does not guarantee/imply any operations on s. Let's do nothing.
    return S_OK;
}

void Bad(__out_ecount(n) PSTR s, unsigned int n)
{
    Fill(s, n);     // BAD. We are not null-terminating s.  [PFXFN] PREfix does not enforce this.
}

void Good(__out_ecount(n) PSTR s, unsigned int n)
{
    s[0] = 0;
    Fill(s, n);
}

void Explicit()
{
    wchar_t buf[10];
    for (int i = 0; i < 9; ++i)
        buf[i] = L'a' + i;
    buf[9] = L'\0';
    f(buf); // OK    
}

void ExplicitChar()
{
    char buf[10];
    for (int i = 0; i < 9; ++i)
        buf[i] = L'a' + i;
    buf[9] = '0'; // error: using '0' instead of '\0'
    f(buf); // BAD. buf is not null-terminated.
}

struct Item
{
    __field_ecount(size) PWSTR text;
    size_t size;
};

__success(return)
bool GetItem(int index, __out Item *item)
{
    static Item items[10];

    if (0 <= index && index < 10)
    {
        *item = items[index];
        return true;
    }

    return false;
}

void TestStruct()
{
    wchar_t szText[256];
    Item item;
    item.text = szText;
    item.size = 256;
    if (GetItem(0, &item))
        f(item.text);   // OK. item.text is PWSTR - i.e., null-terminated.
}

void ArrInit1()
{
    char s[10] = "Hello";
    f(s);   // OK
}

void ArrInit2()
{
    char s[10] = {1, 2, 0};
    f(s);   // OK
}

void ArrInit3()
{
    wchar_t s[10] = {L"abc"};
    f(s);   // OK
}

//Some noise issues not yet addressed
void ArrInit4()
{
    char s[10] = {'a', "tail"};
    f(s);   // Should be OK. [ESPXFP]
}

// extern "C" void memset(void *dst, int c, size_t size);
void ZeroMemory()
{
    WCHAR str[256];
    memset(str, 0, 256 * sizeof(str[0]));
    f(str);
}

void NotZeroMemroy()
{
    WCHAR str[256];
    memset(str, 1, 256 * sizeof(str[0]));
    f(str);     // BAD. str not null-terminated. [PFXFN] PREfix does not handle null-terminated strings well.
}

extern "C"
_At_buffer_(dst, _I_, size, _Post_equal_to_(c))
void MyMemset(__out_bcount_full(size) void *dst, size_t size, int c)
{
    memset(dst, c, size);
}

void GoodMemset(__out_ecount(size) PWSTR p, size_t size)
{
    MyMemset(p, size * sizeof(wchar_t), 0);     // OK. p null-terminated.    
}

void BadMemset(__out_ecount(size) PWSTR p, size_t size)
{
    MyMemset(p, size * sizeof(wchar_t), 10);    // BAD. p not null-terminated. [PFXFN] PREfix does not enforce this.
}

void TestOptional(__out_ecount_opt(n) PSTR p, unsigned int n)
{
    if (p == 0)
        return;
    if (n > 0)
        *p = 0; // OK.
}

//BSTRs have a built in null terminated annotation on them.
void TestBSTR(__in BSTR b)
{
    f(b);       // OK.
}

void TestBSTRTypefix(__in __typefix(BSTR) WCHAR *b)
{
    f(b);       // OK.
}

void main()
{
    char ca[3] = {'a','b','c'}; // not null-terminated
    g(ca);  // BAD. g will pass ca to f that expects null-terminated string. [PFXFN] PREfix does not handle null-terminated strings well.
}