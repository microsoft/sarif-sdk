#include <specstrings.h>
#include <string.h>

#define SIZE 4
#define NUM_INDICES 4

//
// With the advent of SAL 2.0 __field_range on an array becomes meaningless
// so we no longer bother testing it. The correct way to express it is either
// via typedef or _At_buffer_ as we demonstrate in this file.
//


// 1. Try field range directly: this should not work it is at the wrong level of dereference
struct ReproType1
{
    int a;
    _Field_range_(0, SIZE - 1) int indices[NUM_INDICES];
};

// 2. Try _At_buffer_ syntax: this should work and apply to the whole buffer (it's magic!)
struct ReproType2 
{
    _At_buffer_(indices, _I_, NUM_INDICES, __range(0, SIZE - 1))
    int indices[NUM_INDICES];
};

// 3. Try an annotation on a typedef and use that for the index's base type
typedef __range(0, SIZE - 1) int ReproIndexType;

struct ReproType3  
{
    ReproIndexType indices[NUM_INDICES];
};


int globalArray[SIZE];

/*
// 1. Try field range directly.
//     Warnings expected on both array accesses.  See above note.
void espRepro(ReproType1* someType)
{   
    int localArray[SIZE];
    int ci = someType->indices[0];
    localArray[ci] = 0;
    globalArray[ci] = 0;
}
*/
// 2. Try field range deref.
//     No warnings expected in this function.
void espRepro(ReproType2* someType)
{   
    int localArray[SIZE];
    int ci = someType->indices[0];
    localArray[ci] = 0;
    globalArray[ci] = 0;
}

/*
// 3. Try annotation on typedef.
//     No warnings expected in this function.
void espRepro(ReproType3* someType)
{   
    int localArray[SIZE];
    int ci = someType->indices[0];
    localArray[ci] = 0;
    globalArray[ci] = 0;
}
*/

