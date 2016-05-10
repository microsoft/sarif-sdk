
//
// ?: containing a throw used to cause a cfgbuilder crash (ESP 450)
// NOTE: PREfix does have problem with throw in test4. Need to look into it later.
//

int test3(int x)
{
    int buf[10];

    int *p = ((x>=0) ? &buf[0] : throw 999);

    p[11] = 1;        // 26000 on buf
    return 1;
}

int test4(int x)
{
    int buf[10];

    // implicit array-to-pointer conversion
    int *p = ((x>=0) ? buf : throw 999);

    p[11] = 1;        // 26000 on buf
    return 1;
}

typedef int I10[10];
I10 g1, g2;

int test5(int x)
{
    int *q = &((x>=0) ? g1[5] : g2[5]);
    return q[13];  // two 26000, one each for g1 and g2. PREfix reports just once for q[13].
}

int test6(int x)
{
    int *q = &(((x>=0) ? g1 : g2)[5]);
    return q[13];  // two 26000, one each for g1 and g2. PREfix reports just once for q[13].
}

void main() { /* Dummy */ }
