#pragma warning(disable: 6014)
#include <malloc.h>

void C6011_Example( )
{
	char *p = ( char * ) malloc( 10 );		
	*p = '\0';								
	free( p );								
}