// Expansion of valid  annotation to use the annotation on the type. In this case the annotation is on a typedef more than
// one level deep
#include "specstrings.h"
#include "mymemory.h"

struct S {
	int a;
	int b;
	int c;
};

void f()
{
    S *p = (S *)malloc(sizeof(S) - sizeof(int));    // ESPX warns this unsafe cast. [PFXFN] PREfix doesn't.
    if (p == nullptr)
        return;

    p->a = 1;
    p->b = 10;

    // [PFXFN] There should be an error next line. PREfix does not report this.
    p->c = 30;

    free(p);
}

struct Node
{
    Node *next;
    int data;
};

void linkedList()   // Q: What did we want to test through this??? There was no warning expected...
{
    Node *current = new Node;
    if (current == nullptr)
        return;

    Node *header = current;
    for (int i = 0; i < 10; i ++)
    {
        Node *newNode = new Node;
        if (newNode != nullptr)
        {
            current->next  = newNode;
            newNode->next = nullptr;
            newNode->data = i;
            current = newNode;
        }
    }

    current = header;
    while(current != nullptr)
    {
        Node *temp = current;
        current = current->next;
        delete temp;
    }
}


struct extensible
{
    int size;
    int b[20];
    int c[30];
    int a[1];
};

void doExtensible(size_t fullLength)
{
    extensible *x = (extensible *) 
        malloc(sizeof(extensible) + sizeof(int) * (fullLength-1));

    if (x == nullptr)
        return;

    x->size = fullLength;

    for (int i = 0; i < fullLength; i ++)
    {
        x->a[i] = i;
    }
    
    for (int i = 0; i <= fullLength; i ++)
    {
        x->a[i] = i; // Potential overflow here.
    }
    
    // Pure craziness below, but do we want to give overrun warning?
    // ESPX warns this. [PFXFN] PREfix doesn't. It is arguable whether this is really an error or not.
    x->b[25] = 3;

    // Or field arithmetrics
    int cOffset = reinterpret_cast<int>(&((extensible *)0)->c);
    int aOffset = reinterpret_cast<int>(&((extensible *)0)->a);
    memset(&(x->c), 0, aOffset-cOffset);

    // The next one is plainly wrong
    memset((char*)&(x->c), 0, 200);

    free(x);
}

void main() { /* Dummy */ }

