int main()
{
    int a[400];
    for (int k = 0; k <= 100; k ++)
      for (int j = 0; j <= 100; j ++)
        for (int i = 0; i < 200; i ++)
        {
            a[i+j+k] = 0;
        }
}