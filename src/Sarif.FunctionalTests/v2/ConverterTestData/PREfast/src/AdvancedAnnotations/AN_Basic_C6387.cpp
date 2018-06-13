
#include <sal.h>


extern void RequiresNotNull(_In_ int * input);

extern void RequiresNotNullOut(_Out_ int * input);

extern _Post_maybenull_ int * MayReturnNull();


void BasicC6387()
{
    int * data = 0;

    RequiresNotNull(data);
}


void BasicC6387_Opt(_In_opt_ int * data)
{
    RequiresNotNull(data);
}


void BasicC6387_PostNull()
{
    int * data = MayReturnNull();

    RequiresNotNull(data);
}


void BasicC6387_Out()
{
    int * data = 0;

    RequiresNotNullOut(data);
}


#pragma warning(suppress: 28196)
void BasicC6387_OutNotNull(_Outptr_ int ** pOut)
{
    int * data = 0;

    *pOut = data;
}


void BasicC6387_OutMaybeNull(_Outptr_result_maybenull_ int ** pOut)
{
    int * data = 0;

    *pOut = data; // No 6387
}

