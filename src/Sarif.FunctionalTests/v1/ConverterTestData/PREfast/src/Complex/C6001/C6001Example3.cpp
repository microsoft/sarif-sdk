#pragma warning (disable : 4701 4100 4700)
int C6001_Example3( bool b, bool c )
{
	int i;				
	goto labelb;		
labela:					
	goto labelc;
labelb:					
	goto labela;
labelc:					
	int j = i+1;		
	return j;			
}