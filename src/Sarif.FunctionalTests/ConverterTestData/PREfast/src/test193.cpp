typedef unsigned char* PUCHAR;
typedef unsigned short USHORT;

typedef struct LOG_DATA
{
    PUCHAR Line;
    PUCHAR FirstFragmentEnd;
    PUCHAR SecondFragmentEnd;
    PUCHAR ThirdFragmentEnd;
}*PLOG_DATA;

int
UlpFixupIISLogRecord(
     PLOG_DATA LogData
    )
{
    char a[10];
    PUCHAR First;
    PUCHAR Second;
    PUCHAR Third;
    PUCHAR FirstEnd;
    PUCHAR SecondEnd;
    PUCHAR ThirdEnd;

    First = LogData->Line;
    FirstEnd = LogData->FirstFragmentEnd;

    Second = LogData->Line + 4;
    SecondEnd = LogData->SecondFragmentEnd;

    Third = LogData->Line + 6;

    if (FirstEnd <= First || FirstEnd > Second ) {
        return -2;
    }

    // the following overflow was not being detected because
    // pointer comparisons were done incorrectly and so this
    // branch was always considered to be infeasible.
    if (SecondEnd <= Second || SecondEnd > Third ) {
        a[10] = 1; 
        return -2;
    }

    return 0;
}

