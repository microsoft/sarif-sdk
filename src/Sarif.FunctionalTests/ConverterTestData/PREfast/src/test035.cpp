struct S {
	int a;
	char b[100];
};

char f()
{
    char ch;
    S arr[10];
    S *p = arr + 1;
    int i = 1;
    S s1;
    int j;

    s1.a = 7;
    j = 99;
    ch = p[i + s1.a].b[j];  // OK.

    s1.a = 7;
    j = 100;
    ch &= p[i + s1.a].b[j]; // BAD. Overflows b for the last arr element.
    
    s1.a = 8;
    j = 99;
    ch &= p[i + s1.a].b[j]; // BAD. Overflows arr, and thus b as well.
    
    s1.a = 8;
    j = 100;
    return ch & p[i + s1.a].b[j];   // BAD. Overflows arr and b.
    // NOTE: Above reads will cause PREfix to warn also about unint memory accesses.
}

void main() { /* dummy */ }