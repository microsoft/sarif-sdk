#pragma warning (disable : 4701)
int C6001_Example2( bool b, bool c )
{
   int x=6;				
   int i;				
   if ( c ) {				 
	   x=5;			
	   if ( b )
	   {
		   i = 0;
	   } else {
		   x=7;
	   }
   } else {
	   i=0;
   }
   return i;
}