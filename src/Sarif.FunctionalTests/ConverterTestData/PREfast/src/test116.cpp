void main()
{
    int a[15];
    int i = 0;

    do
    {
        a[i] = 1;		// Without fix for ESP562, got 26017 here
        ++i;
    } while (i < 3);

    a[2-i] = 1;        // Sanity check: ESPX:26001 / PFX:24
}
