int C6305_Example1 (int *p)
{
   int i = 0;
   int cb = sizeof(int);

   p+=cb; //Expect 6305
   return i; 
}
