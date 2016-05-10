int a[3];

int QQ();

void foo()
{
    unsigned long index;

    for (index = 0;  index < 3;  index++)
    {
        a[index] = QQ();
        //if (a[index] != 0)
        if (QQ())
        {
            while (index--) {
                a[index] = 1;
            }
            return;
        }
    }
}

void fooSigned()
{
    long index;

    for (index = 0;  index < 3;  index++)
    {
        a[index] = QQ();
        //if (a[index] != 0)
        if (QQ())
        {
            while (index--) {
                a[index] = 1;
            }
            return;
        }
    }
}
