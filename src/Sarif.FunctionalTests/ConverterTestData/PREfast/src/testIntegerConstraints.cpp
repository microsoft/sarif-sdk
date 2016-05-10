#include <espx/IntegerConstraints.h>

int main()
{
    VarId x0 = 0, x1 = 1, x2 = 2, x3 = 3;
    IntegerConstraints c1 = 
        IntegerConstraints().
        insert(VarMap(x0, 1), VarMap(x2, 1), 1). // x0 - x2 <= 1
        insert(VarMap(x1, 1), VarMap(x0, 1), -3).
        insert(VarMap(x1, 1), VarMap(x3, 1), 2). // x1 - x3 <= 2
        insert(VarMap(x2, 1).insert(x3, 1), VarMap(), 0); // x2 + x3 <= 0
    c1.entails(VarMap(x0, 1).insert(x1, 1), VarMap(), 5);

    /* for (i in [0..n-1]) do {j = n - i - 1; a[j] } */
    int i = 10, j = 20, n = 30;
    IntegerConstraints c2 = 
        IntegerConstraints().
        insert(VarMap(), VarMap(i, 1), 0).  // i >= 0
        insert(VarMap(i, 1), VarMap(n, 1), -1). // i <= n-1
        insert(VarMap(j, 1).insert(i, 1), VarMap(n, 1), -1).
        insert(VarMap(n, 1), VarMap(i, 1).insert(j, 1), 1); // j = n - i - 1
    c2.entails(VarMap(), VarMap(j, 1), 0);
    c2.entails(VarMap(j, 1), VarMap(n, 1), -1);

    /* for (i in [0..n-1]) do {j = n - i; a[j] } */
    IntegerConstraints c3 = 
        IntegerConstraints().
        insert(VarMap(), VarMap(i, 1), 0).  // i >= 0
        insert(VarMap(i, 1), VarMap(n, 1), -1). // i <= n-1
        insert(VarMap(j, 1).insert(i, 1), VarMap(n, 1), 0).
        insert(VarMap(n, 1), VarMap(i, 1).insert(j, 1), 0); // j = n - i 
    c3.entails(VarMap(), VarMap(j, 1), 0);
    c3.entails(VarMap(j, 1), VarMap(n, 1), -1);
    c3.entails(VarMap(i, 1).insert(j, 1),VarMap(n, 1), 0);
    c3.entails(VarMap(n, 1), VarMap(i, 1).insert(j, 1), 0);

    /* 5 <= i and i = 3 -> 5 <= 3 ? */
    IntegerConstraints c4 =
        IntegerConstraints().
        insert(VarMap(), VarMap(i, 1), -5).
        insert(VarMap(i, 1), VarMap(), 3).
        insert(VarMap(), VarMap(i, 1), -3);
    c3.entails(VarMap(), VarMap(), 3 - 5);

    IntegerExpr arraySizeInBytes = IntegerExpr(VarMap(n,1), VarMap(), 1) * 3;
    IntegerConstraints c5 =
        IntegerConstraints().insertLE(IntegerExpr(VarMap(i,1), VarMap(), 0) - arraySizeInBytes, 0);
    c5.entails(VarMap(i, 1), VarMap(n, 3), 3);

    IntegerConstraints c6 = 
        IntegerConstraints()
        //.insertLE(arraySizeInBytes - IntegerExpr(VarMap(x0, 1), VarMap(), 0), 0)
        .insert(VarMap(i, 1), VarMap(n, 1), 0)
        .insert(VarMap(j, 1), VarMap(n, 1), 0);
    c6.entails(VarMap(i, 1).insert(j, 2), VarMap(n, 3), 3);

    IntegerConstraints c7 = 
        IntegerConstraints()
        .insert(VarMap(i, 1), VarMap(n, 1), 0)
        .insert(VarMap(n, 30), VarMap(j, 1), 0);
    c7.entails(VarMap(i, 30), VarMap(j, 1), 0);

    IntegerConstraints c8 = 
        IntegerConstraints().
        insert(VarMap(x0, 1), VarMap(x1, 1), -1).
        insert(VarMap(), VarMap(x2, 1), -1).
        insert(VarMap(x2, 1), VarMap(x3, 1), 0).
        insert(VarMap(x1, 1), VarMap(x0, 1).insert(x2, 1), 0);
    c8.entails(VarMap(x0, 1), VarMap(), 4096);
    return 0;
}
