#include <stdlib.h>
#pragma warning (disable: 6014)
void C6200_Example()
{
	int *buff = (int *)malloc(14);

	if(buff != 0)
	{
		for (int i=0; i<=14;i++)
		{
			buff[i]= 0;  //6200
		}
	}

}


