#include "specstrings.h"

#define ARRAYSIZE(a)   (sizeof(a)/sizeof(a[0]))

//Repro case sent by Oleg Kagan
struct LEVEL_CACHE

{
    LEVEL_CACHE(unsigned long Size): m_CacheSize(Size) {m_pCache = new char[m_CacheSize];}
    ~LEVEL_CACHE() {/* delete[] */ delete m_pCache;}
    unsigned long m_CacheSize;
    __field_ecount(m_CacheSize) char* m_pCache;
};

struct CDXGKDX_DUMP_LOG
{
    CDXGKDX_DUMP_LOG()
    {
        for (unsigned long i = 0; i < ARRAYSIZE(LevelCache); ++i)
            LevelCache[i] = new LEVEL_CACHE(1024 * i);
    }

    ~CDXGKDX_DUMP_LOG()
    {
        for (unsigned long i = 0; i < ARRAYSIZE(LevelCache); ++i)
            delete LevelCache[i];
    }

    LEVEL_CACHE* LevelCache[10];
}; 

int __cdecl main()
{
    CDXGKDX_DUMP_LOG x;

    LEVEL_CACHE* LevelCache[10];

    for (unsigned long i = 0; i < ARRAYSIZE(LevelCache); ++i)
        LevelCache[i] = new LEVEL_CACHE(1024 * (i + 1));

    for (unsigned long i = 0; i < ARRAYSIZE(LevelCache); ++i)
        delete LevelCache[i];

    return 0;
} // main()

