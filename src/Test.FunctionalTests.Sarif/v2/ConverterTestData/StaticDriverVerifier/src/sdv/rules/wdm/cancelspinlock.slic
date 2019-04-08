/*++

    Copyright (c) Microsoft Corporation.  All rights reserved.

Rule Name:

    CancelSpinLock

Domain:

    wdm

Rule ID:

    Not Applicable

Description:

    This rule verifies that calls to IoAcquireCancelSpinLock and
    IoReleaseCancelSpinlock are used in strict alternation.

Help Link:

    http://go.microsoft.com/fwlink/?LinkId=507476

--*/


#include "slic_base.h"

state{
   enum {unlocked,locked} s = unlocked;
}


IoAcquireCancelSpinLock.exit
{
    if(s==locked) {
        abort "The driver is calling IoAcquireCancelSpinLock after already acquiring the spinlock.";
    } else {
        s=locked;
    }
}


IoReleaseCancelSpinLock.exit
{
    if(s==unlocked) {
        abort "The driver is calling IoReleaseCancelSpinLock without first acquiring the spinlock.";
    } else {
        s=unlocked;
    }
}


[SDV_RunDispatchFunction].exit
{
    if(s==locked) 
    {
        abort "The dispatch routine has returned without releasing the cancel spinlock.";
    }
}

[RemoveHeadList,sdv_containing_record,RemoveEntryList].entry
{
    halt;
}

[SDV_DRIVER_CANCEL].exit
{
    if(s==locked) 
    {
        abort "The Cancel routine has returned without releasing the cancel spinlock.";
    }
}
