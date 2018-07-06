/*++

    Copyright (c) Microsoft Corporation.  All rights reserved.

Rule Name:

    DispatchRoutine

Domain:

    wdm

Rule ID:

    Not Applicable

Description:

    The property is satisfied if the driver defines a dispatch routine.

Help Link:

    http://go.microsoft.com/fwlink/?LinkId=507476

--*/


#include "slic_base.h"
		 
[sdv_CheckDispatchRoutines].exit
{
  if($return)
  {
    abort "The property is satisfied as the driver defines a dispatch routine.";
  }
}

		 
       

