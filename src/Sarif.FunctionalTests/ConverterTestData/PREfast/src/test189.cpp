#include <specstrings.h>
#include "mywin.h"
#include "undefsal.h"

void foo(_Inout_updates_bytes_(cbMask) BYTE* pMask, _In_ _In_range_(1, 16384) DWORD cbMask, _In_ _In_range_(1, 16384) DWORD cbHashAlg)
{
    BYTE* pbMaskIndex = pMask;
    DWORD cbMaskRemaining = cbMask;

    DWORD cIterations = cbMaskRemaining;
    //DWORD cIterations = cbMaskRemaining + ((cbHashAlg -1) / cbHashAlg);

    for (DWORD i = 0; i < cIterations; i++)
    {
        if (cbMaskRemaining >= cbHashAlg)
        {
            *pbMaskIndex = 1;
            cbMaskRemaining -= cbHashAlg;
            pbMaskIndex += cbHashAlg;
        }

    }
}
