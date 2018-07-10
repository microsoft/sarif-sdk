/*************************************************************************************
 * Copyright (c) Microsoft Corporation.  All rights reserved. 
 *	
 * Description : Doc Sample:
 * warning C6306: incorrect call to <function>: consider using <function> which accepts a va_list as an argument
 * 
 * Command line : cl /W4 /analyze /c <file>
 *
 * Author : ravkaur
 *
 * Date created : 04-04-2005
 *
 *************************************************************************************/
#pragma warning (disable: 36530 28247 28251)
#include <stdio.h>
#include <stdarg.h>
#pragma warning(default: 36530 28247 28251) 


void fd(int i, ...)
{
    va_list v;
      
	va_start(v, i);
	//code...
    printf("%d", v); //@@@Expects:6306; 6273
    va_end(v);
}


void f(int i, ...)
{
    va_list v;
  
	va_start(v, i);
    //code...
	vprintf_s("%d",v);
    va_end(v);

}





