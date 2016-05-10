/* MSRC 6907 */
#include <specstrings.h>

#define icvMax 64
#define HUGE 
#define UNALIGNED 
typedef unsigned int CV;
typedef unsigned int LongRGB;
typedef struct _clrt {
  /* .... */
  CV rgcv[icvMax]; 
  int cvFore; 
 /* array must not be last or espx will treat it as a flex array */
} CLRT;

void _OldRgbToEnvCv(LongRGB UNALIGNED *poldRgb, CV *pcv)
{
    *pcv = (CV)*poldRgb;
}

void SetIcvOrgbClrt(CV icv, long *prgbOld, CLRT HUGE *hpclrt)
{
    CV cv;
    _OldRgbToEnvCv((LongRGB *)prgbOld, &cv);
    // BO occurs here. Rgcv is a statically allocated array of size 64.
    hpclrt->rgcv[icv] = cv;     // BAD. Can overflow. ESPX:26017
}

// Recommend Fix
void GoodFixSetIcvOrgbClrt(__in_range(0,icvMax-1) CV icv, 
                            long *prgbOld, 
                            CLRT HUGE *hpclrt)
{
    CV cv;
    _OldRgbToEnvCv((LongRGB *)prgbOld, &cv);
    hpclrt->rgcv[icv] = cv;
}

void OkFixSetIcvOrgbClrt(__in_range(0,icvMax-1) CV icv, 
                            long *prgbOld, 
                            CLRT HUGE *hpclrt)
{
    CV cv;
    _OldRgbToEnvCv((LongRGB *)prgbOld, &cv);
    if(!(icv < icvMax)) return;     // Q: Isn't this redundant with the above annotation? Shouldn't we remove the annotation for it to be "Ok"-fix?
    hpclrt->rgcv[icv] = cv;
}

void BadFixSetIcvOrgbClrt(__in_range(0,icvMax-1) CV icv, 
                            long *prgbOld, 
                            CLRT HUGE *hpclrt)
{
    CV cv;
    _OldRgbToEnvCv((LongRGB *)prgbOld, &cv);
    __analysis_assume(icv < icvMax);
    hpclrt->rgcv[icv] = cv;
}

void main()
{
    long rgbOld = 123l;
    CLRT clrt;
    SetIcvOrgbClrt((CV)128, &rgbOld, &clrt);    // BAD. Overflow. PFX25.
}