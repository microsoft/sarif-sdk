#pragma warning (disable : 6201 6001)
void C6385_Example2(int i)
{
   int a[20];				
   int b[21];
   for (; i <= 20; i++) {	
	   b[i] = a[i];			
   }
}