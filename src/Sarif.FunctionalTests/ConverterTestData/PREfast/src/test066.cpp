#include "specstrings.h"


typedef unsigned long   AAINDEX;
const AAINDEX  AA_IDX_UNKNOWN = (AAINDEX)~0UL;

struct InlineEvts {
    unsigned int adispidScriptlets[100];
    unsigned int aOffsetScriptlets[100];
    unsigned int aLineScriptlets[100];
    unsigned  cScriptlets;

    InlineEvts() : cScriptlets(0)
    {}
};

struct CAttr {
    unsigned int flags;
    unsigned int dispid;
    unsigned int _ulOffset;
    unsigned int _ulLine;
    AAINDEX FindIdx();
};

AAINDEX CAttr::FindIdx()
{
    AAINDEX idx;
    idx = (unsigned long)this % 2 == 0 ? (unsigned long) this : AA_IDX_UNKNOWN;
    return idx;
}

class CHtmTag {
public:
    unsigned GetAttrCount();
    CAttr *GetAttr(unsigned int i);
};

unsigned CHtmTag::GetAttrCount()
{
    return 101;
}

CAttr* CHtmTag::GetAttr(unsigned int i)
{
    CAttr* attr = new CAttr();
    if (attr != nullptr)
    {
        attr->flags = (int)this & 0xFFFF;
        attr->dispid = i;
        attr->_ulOffset = i % 0xFF;
        attr->_ulLine = (i % 0xFF00) >> 8;
    }

    return attr;
}

void InitAttrBag(CHtmTag *pht)
{
     unsigned i;
     InlineEvts *pInlineEvts = 0;
     for (i = pht ? pht->GetAttrCount() : 0; --i >= 0;)
     {
        CAttr *pattr = pht->GetAttr(i);
        if (pattr != nullptr && pattr->flags & 0x80)
        {
            if (!pInlineEvts)
                pInlineEvts = new InlineEvts;

            if (pInlineEvts)
            {
                pInlineEvts->adispidScriptlets[pInlineEvts->cScriptlets] = pattr->dispid;       // BAD. Can overflow. [PFXFN] PREfix does not warn these.
                pInlineEvts->aOffsetScriptlets[pInlineEvts->cScriptlets] = pattr->_ulOffset;    // BAD. PREfix treates the entire object as a "buffer"
                pInlineEvts->aLineScriptlets[pInlineEvts->cScriptlets++] = pattr->_ulLine;      // BAD
            }
         }
     }
}

void MergeAttrBag(CHtmTag *pht)
{
     unsigned i;
     InlineEvts inlineEvts;
     for (i = pht ? pht->GetAttrCount() : 0; --i >= 0;)
     {
        CAttr *pattr = pht->GetAttr(i);
        if (pattr->FindIdx() != AA_IDX_UNKNOWN)
           continue;
        if (pattr->flags & 0x80)
        {
            inlineEvts.adispidScriptlets[inlineEvts.cScriptlets] = pattr->dispid;       // BAD. Can overflow. [PFXFN] PREfix does not warn these.
            inlineEvts.aOffsetScriptlets[inlineEvts.cScriptlets] = pattr->_ulOffset;    // BAD. PREfix treates the entire object as a "buffer"
            inlineEvts.aLineScriptlets[inlineEvts.cScriptlets++] = pattr->_ulLine;      // BAD
        }
     }
}

void main()
{
    // The lines in the above functions we are interested in do not require an external caller.
    // CHtmTag::GetAttrCount always returns 101, and should overflow the xxxxScriptlets arrayes
    // of InlineEvts objects in both InitAttrBag and MergeAttrBag functions.
}