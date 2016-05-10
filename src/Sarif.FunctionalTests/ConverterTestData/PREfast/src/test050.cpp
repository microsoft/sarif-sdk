#include "specstrings.h"

int a[10];
void f(int i)
{
    a[0] = 10;
    a[a[i]] = 1; // Over/Underflow here.
    a[a[0]] = 1; // Overflow here.

    a[1] = 10;
    a[a[1]] = 1; // Over/Underflow here.

    a[0] = 10;
    a[1] = 1;
    a[*a] = 1;   // Overflow here.

    a[1] = 10;
    a[a[2]] = 0; // Over/Underflow here
}

struct S1 {
    int x, y;
};
struct S {
    S1 a[10];
    int b;
};

int arr[10];
void f(S *s)
{
    s->a[0].x = 1;
    s->a[1].x = 10;
    s->a[2].x = 10;

    arr[s->a[0].x] = 1; // No overflow
    arr[s->a[1].x] = 1; // Overflow here. ESPX warns this as "potential" overflow and underflow here. ESPX doesn't track values for all of the arrray elements.
}

void main()
{
    for (int i = 0; i < 10; ++i)
        a[i] = -1;

    f(3);   // Should cause a[a[i]], a[a[1]], a[a[2]] to underflow. [PFXFN] PREfix doesn't detect this.

    for (int i = 0; i < 10; ++i)
        a[i] = 10;

    f(3);   // Should cause a[a[i]], a[a[1]], a[a[2]] to overflow. [PFXFN] PREfix doesn't detect this.
}