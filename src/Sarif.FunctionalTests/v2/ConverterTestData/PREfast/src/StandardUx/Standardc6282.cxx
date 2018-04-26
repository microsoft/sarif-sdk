/*************************************************************************************
 * Copyright (c) Microsoft Corporation.  All rights reserved. 
 *	
 * Description : Doc Sample:
   warning C6282: Incorrect operator: assignment of constant in Boolean context. Consider using '==' instead   
   
 * Command line : cl /W4 /analyze /c <file>
 *
 * Author : ravkaur
 *
 * Date created : 04-04-2005
 *
 *************************************************************************************/
void f_d(int i)
{
   while (i = 5)	//@@@Expects:6282
   {
   // code  
   }
}

void f(int i)
{
   while (i == 5)
   {
   // code  
   }
}

