/*++

Copyright (c) Microsoft Corporation.  All rights reserved.

Module Name:

    fail_driver1.h

Environment:

    Kernel mode

--*/

#ifdef __cplusplus
extern "C" {
#endif
#include <wdm.h>
#ifdef __cplusplus
}
#endif

typedef struct _DRIVER_DEVICE_EXTENSION
{
    PKSPIN_LOCK queueLock; 
    PRKEVENT  Event;
    KPRIORITY  Increment;
    PIRP Irp;
    PDEVICE_OBJECT DeviceObject;
    ULONG ControllerVector;  
    PKINTERRUPT InterruptObject;
}
DRIVER_DEVICE_EXTENSION,*PDRIVER_DEVICE_EXTENSION;

#ifdef __cplusplus
extern "C"
#endif
DRIVER_INITIALIZE DriverEntry;

DRIVER_ADD_DEVICE DriverAddDevice;

__drv_dispatchType(IRP_MJ_CREATE)
DRIVER_DISPATCH DispatchCreate;

__drv_dispatchType(IRP_MJ_READ)
DRIVER_DISPATCH DispatchRead;

__drv_dispatchType(IRP_MJ_POWER)
DRIVER_DISPATCH DispatchPower;

__drv_dispatchType(IRP_MJ_SYSTEM_CONTROL)
DRIVER_DISPATCH DispatchSystemControl;

__drv_dispatchType(IRP_MJ_PNP)
DRIVER_DISPATCH DispatchPnp;

IO_COMPLETION_ROUTINE CompletionRoutine;

KSERVICE_ROUTINE InterruptServiceRoutine;

IO_DPC_ROUTINE DpcForIsrRoutine;

DRIVER_UNLOAD DriverUnload;
