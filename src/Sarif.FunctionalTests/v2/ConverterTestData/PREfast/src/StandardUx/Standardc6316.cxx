/*************************************************************************************
 * Copyright (c) Microsoft Corporation.  All rights reserved. 
 *	
 * Description : Doc Sample:
 * warning C6316: Incorrect operator: tested expression is constant and non-zero. Use bitwise-and to determine whether bits are set
 * 
 * Command line : cl /W4 /analyze /c <file>
 *
 * Author : ravkaur
 *
 * Date created : 04-04-2005
 *
 *************************************************************************************/
#define INPUT_VALUE 2
#define ALLOWED 1

void fd( int Flags)
{
  if (Flags | INPUT_VALUE) //@@@Expects:6316
  {
    // code
  }
}



void f( int Flags)
{
  if ((Flags & INPUT_VALUE) == ALLOWED)
  {
    // code
  }
}

