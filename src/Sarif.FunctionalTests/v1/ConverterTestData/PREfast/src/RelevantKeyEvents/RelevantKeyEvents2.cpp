 typedef struct xlist {
    struct xlist *pNext;
    struct xlist *pPrev;
 } list;

 list *pNodeFree;
 list *masterList;
 int nBlockSize;

 void fun()
 {
    if (pNodeFree == 0)
    {
        list *pNode = masterList;

        for (int i = nBlockSize-1; i >= 0; i--, pNode--)
        {
            pNode->pNext = pNodeFree;
            pNodeFree = pNode;
        }
    }

    list* pNode = pNodeFree;
    pNode->pPrev = 0;  //28182
 }



