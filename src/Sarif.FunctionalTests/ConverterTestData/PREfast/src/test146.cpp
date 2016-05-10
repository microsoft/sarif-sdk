#include <specstrings.h>
#include "undefsal.h"

// Testcase 1 - repro case from bug Esp:658

typedef __struct_bcount(StructSize) struct _STRUCT1
{
  unsigned long StructSize;
  unsigned long Value;
} STRUCT1, *PSTRUCT1;

void UseStructs(
  __in_ecount(StructCount) PSTRUCT1 *Structs,
  __in unsigned long StructCount
  )
{
  if (StructCount > 0)
  {
    Structs[StructCount - 1]->Value = 7;
  }
}

int main1()
{
  STRUCT1 Struct;
  PSTRUCT1 StructArray[1];
  PSTRUCT1 StructPointer;

  Struct.StructSize = sizeof(Struct);

  StructArray[0] = &Struct;
  StructPointer = &Struct;

  UseStructs(StructArray, 1);
  UseStructs(&StructPointer, 1);
}



// Testcase 2 - repro case from bug Esp:658
//

#define MAX 5

typedef struct _STRUCT2
{
  __field_range(<=, MAX)
  unsigned long Count;
} STRUCT2, *PSTRUCT2;

void UseStruct(
  __in PSTRUCT2 Struct
  )
{
  unsigned long i;
  unsigned long Array[MAX];
  PSTRUCT2 StructCopy = Struct;

  for (i = 0; i < StructCopy->Count; i++)
  {
    Array[i] = 7;    // no buffer overrun here
  }
}

int main2()
{
  STRUCT2 Struct;

  Struct.Count = 2;

  UseStruct(&Struct);

  return 0;
}



// Testcase 3 - repro case from bug Esp:658
//

#define MAX_SIZE 8
 
typedef struct _MyStruct
{
    __field_range(0, MAX_SIZE) unsigned short cElems;
} CMyStruct;
 
typedef struct 
{
    CMyStruct *pMyStruct;
} CBigStruct;
 
void Test3(__in CMyStruct *pMyStruct, /* __in CBigStruct *pBigStruct */ CBigStruct const *pBigStruct)
{
    CMyStruct *pIntermediateStruct = pBigStruct->pMyStruct;
    int array[MAX_SIZE];
    int i;
 
    // Using pBigStruct->pMyStruct is fine
    for (i = 0; i < pBigStruct->pMyStruct->cElems; i++)
        array[i] = 0;
 
    // Using function argument pMyStruct is fine
    for (i = 0; i < pMyStruct->cElems; i++)
        array[i] = 0;
 
    // Using intermediate variable 'pIntermediateStruct' had caused false positives
    for (i = 0; i < pIntermediateStruct->cElems; i++)
        array[i] = 0;    // no buffer overrun here
}


// Testcase 4 - repro case from bug Esp:658

typedef struct _bthpan_ccb_ex_array
{
    // The number of valid CCB entries.
    unsigned long          ulValid;

    // The array of pointers to the CCB.
    __field_ecount(ulValid) int  ccbExArray[1];

} BTHPAN_CCB_EX_ARRAY, *PBTHPAN_CCB_EX_ARRAY;

void Test4()
{
    BTHPAN_CCB_EX_ARRAY      ccbExArrayUnicast = {0};
    PBTHPAN_CCB_EX_ARRAY     pCcbExArray       = 0;

    ccbExArrayUnicast.ulValid = 0;

    pCcbExArray = &ccbExArrayUnicast;

    // Adding this __analysis_assume makes the warning go away
    //__analysis_assume(pCcbExArray->ulValid == ccbExArrayUnicast.ulValid);
    for (unsigned int ulTemp = 0;
         ulTemp < pCcbExArray->ulValid;
         ulTemp++)
    {
        pCcbExArray->ccbExArray[ulTemp] = 0;
    }
}


// Testcase 5 - repro cases for bug Esp:580

typedef struct _tagMyStruct5 {
    __field_range(0, sizeof(struct _tagMyStruct5)) int size;
    char buf[200];
} MyStruct5, *PMyStruct5;

void Function5(__inout_bcount(size) void *p,  int size);

MyStruct5* Get5_1();
Get5_2(__out MyStruct5**);

void Test5_1()
{
    int XXX[50];
    MyStruct5 *p;
    p = Get5_1();
    Function5(&XXX, p->size);   // expected 26014 here: p->size could be >= 201
}

void Test5_2()
{
    int XXX[50];
    MyStruct5 *p;
    Get5_2(&p);
    Function5(&XXX, p->size);   // expected 26014 here: p->size could be >= 201
}

void Test5_3()
{
    int XXX[50];
    MyStruct5 *p, *q;
    Get5_2(&p);
    q = p;
    Function5(&XXX, q->size);   // expected 26014 here: q->size could be >= 201
}



// Testcase 6 - repro cases for bug Esp:604

typedef __struct_bcount(Size) struct  _W6
{
    int Size;
} W6;


W6* Get6_1();

W6* Get6_2()
{
    return Get6_1();
}

W6* Copy6(W6* p)
{
    return p;
}

void Copy6(bool b, W6* p, __deref_out W6** o)
{
    *o = p;
    if (b)
    {
        p = 0;
        return;
    }
    return;
}


struct S6
{
    __nullterminated char *s;
    __field_ecount(len) char *buffer;
    int len;
};

S6* Get1S_6();

void Get2S_6(S6** o)
{
    S6 *p = Get1S_6();
    *o = p;
    return;
}


__success(return != false)
bool Get6_3(__out_bcount(size) W6 *pW6, unsigned int size)
{
    if (size < sizeof(W6))
        return false;

    pW6->Size = size;
    return true;
}

