/* MSRC6705/6886 - I think the warning at 152 is the important warning here */
#include<specstrings.h>
#define TRUE 1
#define HUGE 
#define NULL 0
#define MsoAssertTag(x,y)
#define MsoAssertMsgTag(x,y,z)
#define MsoAssertSzTag(x,y,z)
#define Assert(x)
typedef unsigned char BYTE;
typedef unsigned BOOL;

typedef struct _book {
    void *hpstPassword;
} BOOK;

typedef struct _rst {
    BOOL fExt;
} RST;

typedef struct _srst
{
    /* ... */
    RST rst;

} SRST;

typedef struct _extrst 
{
    /* ... */
    unsigned cb;
    unsigned terst;
    BOOL fNext;
} EXTRST;

typedef  struct _rthdr
{
    /* ... */
    unsigned cb;

} RTHDR;

typedef  struct _rphs
{
    unsigned cb;
    /* ... */
    struct {
        void *ifnt;
    } phs;
    struct {
        char st[1];
    } rphssub;

} RPHS;
typedef  struct _fnt
{
    BOOL fMarked;
} FNT;

struct 
{
    unsigned lcCur;
}  fosgc;

struct {
    /* */
    BOOL fHasExtRst;
} hpbookCur;
int min(int x,int y) { return (x < y) ? x : y; }
EXTRST extrsts[256];
EXTRST* PerstFirstFromPrst(RST*) { return extrsts; }
EXTRST* PerstNextFromPerst(EXTRST*) { static EXTRST* pExtrst = extrsts; return ++pExtrst; }
int rtContinue;
BOOL fEncrypted;
unsigned terstPhonetic;
unsigned wverLastXLSaved;
unsigned verXL2K;
unsigned wBiffVer;
unsigned verMajor6;

void SwapBytes(__inout_bcount(cb) void * data, unsigned int cb) { /* TODO; */ }
void BltB(__out_bcount(cb) void * pvFrom,__in_bcount(cb) void * pvTo, unsigned int cb)  // Q?? Aren't pvFrom and pvTo swapped?
{
    if (pvFrom && pvTo && cb > 0)
    {
        for (unsigned int i = 0; i < cb; ++i)
            *(BYTE*)pvTo = *(BYTE*)pvFrom;
    }
}  
void DecryptRecord(BYTE *, int, BOOL) { /* TODO; */ }
BOOK book;
BOOK* HpbookCur_pGbook(void) { return &book; }
BOOL FNullLHp(void *) { static bool magic; return magic; }
int RtOfNextRecord(void) { static int magic; return magic; }
int iMagic;
int CbOfNextRecord(void)  { return iMagic; }
int DataOfNextRecordRgbBiff(BYTE *,int,int)  { static int magic; return magic; }
FNT fnt;
FNT*  HpfntdFromIfnt(void *) { return &fnt; }

#define fTrue 1
#define fFalse 0
BYTE *PbLoadExtRstSst(SRST HUGE *hpsrst, int cbextrst, 
                      BYTE *rgb, 
                      __inout_ecount((*ppbMax)- pb) BYTE *pb, 
                      __deref_in_ecount(0) BYTE **ppbMax)
{
    int cbT, cbPhinfo;
    EXTRST *perst;
    BYTE *pbT;
    BOOL fHeadErst = fTrue;
    int rt, cb;
    hpsrst->rst.fExt = fTrue;
    hpbookCur.fHasExtRst = fTrue;
    perst = PerstFirstFromPrst(&hpsrst->rst);
    while (cbextrst > 0)
    {
        if (sizeof(EXTRST) > *ppbMax - pb)
        {
            rt = RtOfNextRecord();
            if (rt != rtContinue)
                return NULL;
            cb = CbOfNextRecord();

            DataOfNextRecordRgbBiff(rgb,cb,cb);

            if (fEncrypted && !FNullLHp(HpbookCur_pGbook()->hpstPassword))
                DecryptRecord(rgb, cb, TRUE);
            pb = rgb;
            *ppbMax = rgb+cb;
            fosgc.lcCur += (cb+sizeof(RTHDR));
        }
        cbPhinfo = cbT = ((EXTRST *)pb)->cb;

        BltB(pb, (BYTE *)perst, sizeof(EXTRST));

        SwapBytes((BYTE *)perst, sizeof(EXTRST));
        pbT = (BYTE *)perst + sizeof(EXTRST);
        pb += sizeof(EXTRST);
        cbextrst -= sizeof(EXTRST);


        while (cbT > 0)
        {
            if (pb >= *ppbMax)
            {
                rt = RtOfNextRecord();
                if (rt != rtContinue)
                    return NULL;
                cb = CbOfNextRecord();

                DataOfNextRecordRgbBiff(rgb,cb,cb);

                if (fEncrypted && !FNullLHp(HpbookCur_pGbook()->hpstPassword))
                    DecryptRecord(rgb, cb, TRUE);
                pb = rgb;
                *ppbMax = rgb+cb;
                fosgc.lcCur += (cb+sizeof(RTHDR));
            }

            cb = min(cbT, *ppbMax-pb);

            BltB(pb, pbT, cb);      // BAD. pb can overflow. cb is not related to pb's size. ESPX:26015 / PFX:25
            SwapBytes(pbT, cb);
            if (perst->terst == terstPhonetic && fHeadErst)
            {
                HpfntdFromIfnt(((RPHS *)pbT)->phs.ifnt)->fMarked = fTrue;

                // MacXL 98 saves 0-length phonetic info incorrectly:
                // their RPHS structure is 2 bytes shorter than ours. They save 2 ANSI chars instead 2 Unicode chars
                // when saving 0-length phonetic info even though they correctly save Unicode for regular strings.

                MsoAssertMsgTag(FImplies(cbPhinfo < sizeof(RPHS),
                    perst->terst == terstPhonetic && cbPhinfo == 10 && perst->cb == 10 && FIsRup(wverLastXLSaved, 5506, verXL2K)),
                    "Bad phonetic info", ASSERTTAG('ccdm'));

                if ((wBiffVer >= verMajor6) && cbPhinfo == 10 && ((char *)(((RPHS *)pbT)->rphssub.st))[0] == 0)
                {
                    // pbT should be pointing to the RPHS which is at the end of the EXTRST
                    MsoAssertTag((BYTE *)(perst + 1) == pbT, ASSERTTAG('ccdn'));
                    MsoAssertMsgTag(fFalse, "Warning: fixing up bad Mac phonetic info", ASSERTTAG('ccdo'));

                    perst->cb = sizeof(RPHS);

                    MsoAssertTag(((RPHS *)pbT)->rphssub.st[0] == 0, ASSERTTAG('ccdp'));
                    ((RPHS *)pbT)->rphssub.st[1] = 0;
                    pbT += 2;
                }
            }
            fHeadErst = fFalse;
            pbT += cb;
            pb += cb;
            cbT -= cb;
            cbextrst -= cb;
        }
        if (!perst->fNext)  // next Extended record
            break;

        perst = PerstNextFromPerst(perst);
    }
    return pb;
}
#if 0  
BYTE *FixedPbLoadExtRstSst(SRST HUGE *hpsrst, int cbextrst,  
                           __inout_ecount(pbRgbMax - rgb) BYTE *rgb, 
                           __in_ecount(0) const BYTE *pbRgbMax, 
                           __inout_ecount((*ppbMax)- pb) BYTE *pb, 
                           __deref_in_ecount(0) BYTE **ppbMax)
{
    int cbT, cbPhinfo;
    EXTRST *perst;
    BYTE *pbT;
    BOOL fHeadErst = fTrue;
    int rt, cb;

    hpsrst->rst.fExt = fTrue;
    hpbookCur.fHasExtRst = fTrue;
    perst = PerstFirstFromPrst(&hpsrst->rst);
    while (cbextrst > 0)
    {
        if (sizeof(EXTRST) > *ppbMax - pb)
        {
            rt = RtOfNextRecord();
            if (rt != rtContinue)
                return NULL;
            cb = CbOfNextRecord();
            DataOfNextRecordRgbBiff(rgb,cb,pbRgbMax-rgb);
            if (fEncrypted && !FNullLHp(HpbookCur_pGbook()->hpstPassword))
                DecryptRecord(rgb, cb, TRUE);
            pb = rgb;
            *ppbMax = rgb+cb;
            fosgc.lcCur += (cb+sizeof(RTHDR));
        }
        cbPhinfo = cbT = ((EXTRST *)pb)->cb;
        *perst = *(EXTRST*)pb;
        SwapBytes((BYTE *)perst, sizeof(EXTRST));
        pbT = (BYTE *)perst + sizeof(EXTRST);
        pb += sizeof(EXTRST);
        cbextrst -= sizeof(EXTRST);

        // O12 Bug 673309: If we didn't have enough bytes for EXTRST then fail the load.
        if (cbT > cbextrst || cbextrst < 0)
        {
            MsoAssertSzTag(fFalse, "Phonetic cbT > cbextrst in shared string table -- corrupt document", ASSERTTAG('8unm'));
            return NULL;
        }
        Assert(cbT <= cbextrst);
        Assert(FImplies((cbT > 0), (cbextrst > 0)));

        while (cbT > 0)
        {
            if (pb >= *ppbMax)
            {
                rt = RtOfNextRecord();
                if (rt != rtContinue)
                    return NULL;
                cb = CbOfNextRecord();
                DataOfNextRecordRgbBiff(rgb,cb,pbRgbMax-rgb);
                if (fEncrypted && !FNullLHp(HpbookCur_pGbook()->hpstPassword))
                    DecryptRecord(rgb, cb, TRUE);
                pb = rgb;
                *ppbMax = rgb+cb;
                fosgc.lcCur += (cb+sizeof(RTHDR));
            }
            cb = min(cbT, (*ppbMax)-pb);
            Assert(cb <= cbT);
            Assert(cb <= cbextrst);

            BltB(pb, pbT, cb);
            SwapBytes(pbT, cb);
            if (perst->terst == terstPhonetic && fHeadErst)
            {
                HpfntdFromIfnt(((RPHS *)pbT)->phs.ifnt)->fMarked = fTrue;


                // MacXL 98 saves 0-length phonetic info incorrectly:
                // their RPHS structure is 2 bytes shorter than ours. They save 2 ANSI chars instead 2 Unicode chars
                // when saving 0-length phonetic info even though they correctly save Unicode for regular strings.

                MsoAssertMsgTag(FImplies(cbPhinfo < sizeof(RPHS),
                    perst->terst == terstPhonetic && cbPhinfo == 10 && perst->cb == 10 && FIsRup(wverLastXLSaved, 5506, verXL2K)),
                    "Bad phonetic info", ASSERTTAG('ccdm'));

                if ((wBiffVer >= verMajor6) && cbPhinfo == 10 && ((char *)(((RPHS *)pbT)->rphssub.st))[0] == 0)
                {
                    // pbT should be pointing to the RPHS which is at the end of the EXTRST
                    MsoAssertTag((BYTE *)(perst + 1) == pbT, ASSERTTAG('ccdn'));
                    MsoAssertMsgTag(fFalse, "Warning: fixing up bad Mac phonetic info", ASSERTTAG('ccdo'));

                    perst->cb = sizeof(RPHS);

                    MsoAssertTag(((RPHS *)pbT)->rphssub.st[0] == 0, ASSERTTAG('ccdp'));
                    ((RPHS *)pbT)->rphssub.st[1] = 0;
                    pbT += 2;
                }
            }
            fHeadErst = fFalse;
            pbT += cb;
            pb += cb;
            cbT -= cb;
            cbextrst -= cb;
        }
        if (!perst->fNext)  // next Extended record
            break;

        // O12 Bug 673309: If we don't have any bytes left then there can't be another extended rst.
        if (cbextrst == 0)
        {
            perst->fNext = fFalse;
            break;
        }

        perst = PerstNextFromPerst(perst);
    }
    return pb;
}
#endif 

void main() 
{
    iMagic = 256;
    SRST hpsrst;
    BYTE rgb;
    BYTE pb[0xFFFF] = {};
    BYTE* pbMax = &pb[0xFFFE];

    // TODO: Need more work to help PREfix to discover the issues we are interested. For now, skipping this test case...
    // Q: What is the key point of this test case?

    BYTE data = *PbLoadExtRstSst(
        &hpsrst,
        128, 
        &rgb, 
        pb, 
        &pbMax);
}
