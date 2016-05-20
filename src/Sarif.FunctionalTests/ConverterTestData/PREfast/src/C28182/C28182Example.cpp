 #pragma warning (disable : 4189)
typedef struct xlist {
    struct xlist *pNext;
    struct xlist *pPrev;
 } list;

 list *pNodeFree;
 list *masterList;
 int nBlockSize;

 void C28182Example()
 {
    if (pNodeFree == 0)
    {
		 list* pNode = pNodeFree;
         pNode->pPrev = 0;  
    }
	else
	{
		list* nNode = masterList;
	}
 }



