/* MSRC6887 */
#include <specstrings.h>
#include "undefsal.h"

#define HUGE 
#define NATIVE
#define MsoAssertTag(x,y)
typedef struct _cell {
  /* .... */
  int fGhostDirty;
} CELL;
typedef struct _sh {
  /* .... */
  unsigned HUGE *hrgsb;
} SH;

struct ws {
  /* .... */
  int rwMic;
  int rwMac;
  unsigned int sbRw0;
};

typedef struct _clb {
  /* .... */
    void *unused;
    int colMic;
    int colMac;
    unsigned *rgbrgcell;

} CLB;

ws g_ws;
struct ws* HpwsFromHpsh(SH HUGE *hpsh)
{
    return &g_ws;
}

struct ws* HpwsCur_pGws()
{
    return &g_ws; 
}

SH g_sh;
SH *HpshCur_pGsh()
{
    return &g_sh;
}

CLB g_clb;
CLB HUGE* HpclbFromSbHrgsb(unsigned,unsigned HUGE *)
{
    return &g_clb;        
}

int magic;
int FEmptyClb(CLB HUGE *)
{
    return magic;
}

void *SbOfHp(CELL HUGE* cell)
{
    static bool magic;
    return magic ? nullptr : (void*)cell;
}

int LogRwMaxCLB;
int rwMaxCLB_1;
int grbiticell;
int fDebugHpcellFuncs;
void *sbNull;
CELL cellDefault;

// Note must have statically known size
// missed by espX because hpGhostRw is malloced from heap data.
CELL hpGhostRw[256]; 
// espX needs something like the following
// extern __global_ecount(colMax+1) CELL *hpGhostRw; 

// must have __out annotatoin
_Ret_cap_c_(1) _Ret_valid_ NATIVE CELL HUGE *HpcellFromRwCol(int rw, int col)
{
    CELL HUGE *hpcell;
    CLB HUGE *hpclb;
    unsigned icell;
    int rwMic, rwMac;
    int colMic;
    int colMac;
    int dcol;
    unsigned sbRw0;
    unsigned HUGE *hrgsb;
    unsigned sbRw;

    sbRw = rw;
    rwMic = HpwsCur_pGws()->rwMic;
    rwMac = HpwsCur_pGws()->rwMac;
    sbRw0 = HpwsCur_pGws()->sbRw0;
    sbRw >>= LogRwMaxCLB;
    hrgsb = HpshCur_pGsh()->hrgsb;
    sbRw += sbRw0;
    if (rw < rwMic || rw >= rwMac)
        goto UseGhostRw;
    hpclb = HpclbFromSbHrgsb(sbRw,hrgsb);
    rw &= rwMaxCLB_1;
    if (FEmptyClb(hpclb))
        goto UseGhostRw;
    colMic = *((int *)&hpclb->unused);
    colMac = *((int *)&hpclb->colMac);
    icell = hpclb->rgbrgcell[rw] & grbiticell;
    colMic >>= 16;
    colMac &= 0xffff;
    MsoAssertTag(colMic == hpclb->colMic, ASSERTTAG('bfgl'));
    MsoAssertTag(colMac == hpclb->colMac, ASSERTTAG('bfgm'));
    if (icell == 0)
        goto UseGhostRw;
    if ((dcol = col - colMic) >= 0)
    {
        dcol += icell;
        if (col < colMac)
            return (CELL HUGE *)hpclb + dcol;
    }
    MsoAssertTag(hpclb->fColFormat, ASSERTTAG('bfgn'));
    hpcell = (CELL HUGE *)hpclb + icell + colMac - colMic;
    if (hpcell->fGhostDirty
        || fDebugHpcellFuncs /* Lets help out the Hpcell functions */
        )
    {
        return(hpcell);
    }
UseGhostRw:
    MsoAssertTag(col<=colMax, ASSERTTAG('bfgo'));
    if (SbOfHp(hpGhostRw) == sbNull)
        return(&cellDefault);
    else
        return(hpGhostRw + col);    // BAD. If we not check range of col. ESPX:26031,26040
}

void main()
{
    NATIVE CELL HUGE *pCell;

    pCell = HpcellFromRwCol(0,-1);
    pCell->fGhostDirty = 1;     // BAD. We may have gotten hpGhostRW[-1]. [PFXFN] Somehow, PREfix misses this.

    pCell = HpcellFromRwCol(0,256);
    pCell->fGhostDirty = 1;     // BAD. We may have gotten hpGhostRW[256]. [PFXFN] Somehow, PREfix misses this.
}