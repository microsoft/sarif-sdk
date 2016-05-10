#define var1(N) int n0##N = 0; int n1##N = 0;
#define var2(N) var1(0##N) var1(1##N)
#define var3(N) var2(0##N) var2(1##N)
#define var4(N) var3(0##N) var3(1##N)
#define var5(N) var4(0##N) var4(1##N)
#define var6(N) var5(0##N) var5(1##N)
#define var7(N) var6(0##N) var6(1##N)
#define var8(N) var7(0##N) var7(1##N)
#define var9(N) var8(0##N) var8(1##N)

template<int N>
void g()
{
    var9(0);
    if (n0000000000)
    {
        var9(1);
        while (n0000000010)
        {
            var9(2);
            if (n0000000101)
            {
                n0000000002 = 1;
                break;
            }

            n0000000002 = 3;
        }
    }

    n0000000000 = 0;
}

template<int N>
void k()
{
    g<N>();
    k<N - 1>();
}

template<>
void k<1>()
{
    g<1>();
}

void f()
{
    k<20>();
}
