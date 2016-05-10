#include <specstrings.h>
#include "undefsal.h"

typedef _Return_type_success_(return >= 0) long NTSTATUS;

struct WNF_IMMERSIVE_MONITOR_DATA
{
    unsigned long monitorId;
    unsigned long hMonitor;
    bool fIsImmersive;
};

struct WNF_IMMERSIVE_MODE_DATA
{
    unsigned int               cMonitorData;
    _Readable_elements_(cMonitorData) WNF_IMMERSIVE_MONITOR_DATA rgMonitorData[1]; 
};


_When_(cbBuffer != 0, _At_((WNF_IMMERSIVE_MODE_DATA const *)pvBuffer, _In_ _Readable_bytes_(cbBuffer)))
_When_(cbBuffer == 0, _At_(pvBuffer, _Null_))
NTSTATUS ImmersiveModeWNFCallback(_In_ const void* pvBuffer, unsigned long cbBuffer)
{
    if (pvBuffer != 0 && cbBuffer != 0)
    {
        auto pModeData = static_cast<WNF_IMMERSIVE_MODE_DATA const *>(pvBuffer);
        // this next line should give a potential buffer overrun as the size of pModeData (aliasing pvBuffer)
        // is supposed to be given by cbBuffer....and we have only checked that it is non-zero so far...
        bool fTest = pModeData->rgMonitorData[pModeData->cMonitorData].fIsImmersive;
    }
    return 0;
}