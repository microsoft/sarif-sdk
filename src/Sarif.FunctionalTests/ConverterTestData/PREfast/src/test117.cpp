#include "specstrings.h"

//Bug: Not tracking switch statements
int* foo(int b)
{
    enum AllocType {
        A1,
        A2
    } a;

    int *p = 0;
    if (b)
    {
        a = A1;
        p = new int[10];
    }
    else
    {
        a = A2;
        p = new int[20];
    }
    
    if (p)
    {
        switch (a)
        {
            case A1:
                p[9] = 1;
                break;
            case A2:
                p[20] = 1;  // BAD. Overflows. ESPX:26000 / PFX:23
                break;
        }
    }

    return p;
}

// Modeled after PREFAST_MSGID_11288867
enum TT { AA = 2, BB = 7, CC = 8, DD = 9 };

char * foo2a(TT t)
{
    size_t size = sizeof(int);

    switch (t)
    {
      case AA:
        size += sizeof(double);
        break;
      case BB:
        size += sizeof(int);
        break;
      case CC:
        size += sizeof(double);
        size += sizeof(int);
        break;
    }

    char *buffer = new char[size];
    if (buffer)
    {
        char *p = buffer;

        *(int*)p = 99;
        p += sizeof(int);

        switch (t)
        {
          case AA:
            *(double*)p = 1.23;
            p += sizeof(double);
            break;
          case BB:
            *(int*)p = 123;
            p += sizeof(int);
            break;
          case CC:
            *(double*)p = 1.23;
            p += sizeof(double);
            *(int*)p = 123;
            p += sizeof(int);
            break;
        }
    }

    return buffer;
}

char * foo2b(TT t)
{
    size_t size = sizeof(int);

    switch (t)
    {
      case AA:
        size += sizeof(double);
        break;
      case BB:
        size += sizeof(int);
        break;
      case CC:
        size += sizeof(double);
        size += sizeof(int);
        break;
    }

    char *buffer = new char[size];
    if (buffer)
    {
        char *p = buffer;

        *(int*)p = 99;
        p += sizeof(int);

        switch (t)
        {
          case AA:
            *(double*)p = 1.23;
            p += sizeof(double);
            break;
          case BB:
            *(int*)p = 123;
            p += sizeof(int);
            break;
          case CC:
            *(double*)p = 1.23;
            p += sizeof(double);
            *(int*)p = 123;
            p += sizeof(int);
            break;
          case DD:
            *(double*)p = 1.23;    // BAD. p points to a 4 byte buffer. ESPX:26000 / PFX:23
            p += sizeof(double);
            *(double*)p = 1.23;
            p += sizeof(double);
        }
    }

    return buffer;
}

void foo3(size_t x)
{
    int aa[10], bb[10], cc[10];

    switch (x % 7)
    {
        //case 0: return;
      case 1: return;
        //case 2: return;
        //case 3: return;
        //case 4: return;
      case 5: return;
        //case 6: return;
    }

    switch (x % 7)
    {
        //case 0: return;
        //case 1: return;
        //case 2: return;
      case 3: bb[x+3] = 1; return;  // 26015
        //case 4: return;
        //case 5: return;
        //case 6: return;
    }

    switch (x % 7)
    {
      case 0: return;
        //case 1: return;
      case 2: cc[x]  = 1; return;  // 26015
      case 3: aa[99] = 1; return;  // don't get here
      case 4: return;
      case 5: aa[-99] = 1; return; // don't get here
      case 6: return;
    }
    
    bb[-99] = 1;  // don't get here. [PFXFP] PREfix thinks we can reach this - unable to deal cases of x % 7. 
}

void main()
{
    foo3(70);   // OK. [PFXFP] PREfix thinks this can hit line 163.
    foo3(71);   // OK. [PFXFP] PREfix thinks this can hit line 163.
    foo3(72);   // BAD. PFX:27
    foo3(73);   // BAD. PFX:27
    foo3(74);   // OK. [PFXFP] PREfix thinks this can hit line 173.
    foo3(75);   // OK. [PFXFP] PREfix thinks this can hit line 173.
    foo3(76);   // OK. [PFXFP] PREfix thinks this can hit line 173.
}