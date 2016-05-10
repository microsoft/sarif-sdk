void ReproFunction()
{
    int a[5];

    int flags = 0x0;

    flags = flags | 0x0;
    if (flags)
    {
        a[5] = 1;   // No warning. Won't be executed.
    }

    flags |= 0x0;
    if (flags)
    {
        a[6] = 1;   // No warning. Won't be executed.
    }

    flags += 0x10;
    if (flags & 0x10)
    {
        a[6] = 1;   // Warning 26000.
    }

    flags -= 0x8;
    if (flags & 0x8)
    {
        a[6] = 1;   // Warning 26000.
    }

    flags -= 0x8;
    if (flags)
    {
        a[6] = 1;   // No warning. Won't be executed.
    }

    flags += 0x1;
    flags <<= 8;
    if (flags & 0x00000100)
    {
        a[6] = 1;   // Warning 26000.
    }

    flags = 0x1;
    flags *= 0x10;
    if (flags & 0x10)
    {
        a[6] = 1;   // Warning 26000.
    }

    flags /= 0x10;
    if (flags)
    {
        a[6] = 1;   // Warning 26000.
    }

    flags -= 0x1;
    if (flags)
    {
        a[6] = 1;   // No warning. Won't be executed.
    }

    flags += 9;
    flags %= 0x5;
    if (flags & 0x4)
    {
        a[6] = 1;   // Warning 26000.
    }
}