#include "AdditiveConstraints.h"

int main()
{
    VarId x0 = 0, x1 = 1, x2 = 2, x3 = 3;
    AdditiveConstraints c1 = 
        AdditiveConstraints().
        insert(VarSet(x0), VarSet(x2), 1). // x0 - x2 <= 1
        insert(VarSet(x1), VarSet(x0), -3).
        insert(VarSet(x1), VarSet(x3), 2). // x1 - x3 <= 2
        insert(VarSet(x2).insert(x3), VarSet(), 0); // x2 + x3 <= 0
    c1.entails(VarSet(x0).insert(x1), VarSet(), 5);

    /* for (i in [0..n-1]) do {j = n - i - 1; a[j] } */
    int i = 10, j = 20, n = 30;
    AdditiveConstraints c2 = 
        AdditiveConstraints().
        insert(VarSet(), VarSet(i), 0).  // i >= 0
        insert(VarSet(i), VarSet(n), -1). // i <= n-1
        insert(VarSet(j).insert(i), VarSet(n), -1).
        insert(VarSet(n), VarSet(i).insert(j), 1); // j = n - i - 1
    c2.entails(VarSet(), VarSet(j), 0);
    c2.entails(VarSet(j), VarSet(n), -1);

    /* for (i in [0..n-1]) do {j = n - i; a[j] } */
    AdditiveConstraints c3 = 
        AdditiveConstraints().
        insert(VarSet(), VarSet(i), 0).  // i >= 0
        insert(VarSet(i), VarSet(n), -1). // i <= n-1
        insert(VarSet(j).insert(i), VarSet(n), 0).
        insert(VarSet(n), VarSet(i).insert(j), 0); // j = n - i 
    c3.entails(VarSet(), VarSet(j), 0);
    c3.entails(VarSet(j), VarSet(n), -1);
    c3.entails(VarSet(i).insert(j),VarSet(n), 0);
    c3.entails(VarSet(n), VarSet(i).insert(j), 0);

    /* 5 <= i and i = 3 -> 5 <= 3 ? */
    AdditiveConstraints c4 =
        AdditiveConstraints().
        insert(VarSet(), VarSet(i), -5).
        insert(VarSet(i), VarSet(), 3).
        insert(VarSet(), VarSet(i), -3);
    c3.entails(VarSet(), VarSet(), 3 - 5);

    return 0;
}
