#include <specstrings.h>

struct ReadPkt
{
    int a;
    int b;
    int c;
    int d;
    int e;
    int f;
    int g;
    int h;
};

struct WritePkt
{
    int a;
};

struct Request
{
    char somefield;
    union
    {
        ReadPkt* read;
        WritePkt* write;
    } Pkt;
};

void foo()
{
    char a[10];
    a[0] = 0;       // force EspX to check this function
    Request req;
    ReadPkt read;

    req.Pkt.read = &read;

    req.Pkt.read->a = 1;
    req.Pkt.read->h = 2;
}

