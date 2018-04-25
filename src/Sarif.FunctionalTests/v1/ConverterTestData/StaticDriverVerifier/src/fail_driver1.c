/*++

Copyright (c) Microsoft Corporation.  All rights reserved.

Module Name:

    fail_driver1.c

Abstract:

    This is a sample driver that contains intentionally placed
    code defects in order to illustrate how Static Driver Verifier
    works. This driver is not functional and not intended as a 
    sample for real driver development projects.

Environment:

    Kernel mode

--*/

#include "fail_driver1.h"

#define _DRIVER_NAME_ "fail_driver1"

#ifndef __cplusplus
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, DriverAddDevice)
#pragma alloc_text (PAGE, DispatchCreate)
#pragma alloc_text (PAGE, DispatchRead)
#pragma alloc_text (PAGE, DispatchPower)
#pragma alloc_text (PAGE, DispatchSystemControl)
#pragma alloc_text (PAGE, DispatchPnp)
#pragma alloc_text (PAGE, DriverUnload)
#endif

NTSTATUS
DriverEntry(
    IN PDRIVER_OBJECT  DriverObject,
    IN PUNICODE_STRING RegistryPath
    )
{

    UNREFERENCED_PARAMETER(RegistryPath);
    DriverObject->MajorFunction[IRP_MJ_CREATE]         = DispatchCreate;
    DriverObject->MajorFunction[IRP_MJ_READ]           = DispatchRead;
    DriverObject->MajorFunction[IRP_MJ_POWER]          = DispatchPower;
    DriverObject->MajorFunction[IRP_MJ_SYSTEM_CONTROL] = DispatchSystemControl;
    DriverObject->MajorFunction[IRP_MJ_PNP]            = DispatchPnp;
    DriverObject->DriverExtension->AddDevice           = DriverAddDevice;
    DriverObject->DriverUnload                         = DriverUnload;

    return STATUS_SUCCESS;
}

NTSTATUS
DriverAddDevice(
    IN PDRIVER_OBJECT DriverObject,
    IN PDEVICE_OBJECT PhysicalDeviceObject
    )
{
    PDEVICE_OBJECT device;
    PDRIVER_DEVICE_EXTENSION extension ;
    NTSTATUS status;
    
    UNREFERENCED_PARAMETER(DriverObject);
    UNREFERENCED_PARAMETER(PhysicalDeviceObject);
       

    status = IoCreateDevice(DriverObject,                 
                            sizeof(DRIVER_DEVICE_EXTENSION), 
                            NULL,                   
                            FILE_DEVICE_DISK,  
                            0,                     
                            FALSE,                 
                            &device                
                            );
  
   extension = (PDRIVER_DEVICE_EXTENSION)(device->DeviceExtension);

   IoInitializeDpcRequest(device,DpcForIsrRoutine);

   return status;
}

NTSTATUS
DispatchCreate (
    IN PDEVICE_OBJECT DeviceObject,
    IN PIRP Irp
    )
{   
    KAFFINITY ProcessorMask;
    PDRIVER_DEVICE_EXTENSION extension ;
    
    PVOID *badPointer = NULL;

    UNREFERENCED_PARAMETER(DeviceObject);
    UNREFERENCED_PARAMETER(Irp);
    
    ExFreePool(badPointer);


    extension = (PDRIVER_DEVICE_EXTENSION)DeviceObject -> DeviceExtension;

    ProcessorMask   =  (KAFFINITY)1;
    
    IoConnectInterrupt( &extension->InterruptObject,
                         InterruptServiceRoutine,
                         extension,
                         NULL,
                         extension->ControllerVector,
                         PASSIVE_LEVEL,
                         PASSIVE_LEVEL,
                         LevelSensitive,
                         TRUE,
                         ProcessorMask,
                         TRUE );
	
    return STATUS_SUCCESS;
}

NTSTATUS
DispatchRead (
    IN PDEVICE_OBJECT DeviceObject,
    IN PIRP Irp
    )
{  
    /*
       This defect is injected for the "SpinLock" rule.
    */
    KSPIN_LOCK  queueLock;
    KIRQL oldIrql;

    UNREFERENCED_PARAMETER(DeviceObject);
    UNREFERENCED_PARAMETER(Irp);
     
    KeAcquireSpinLock(&queueLock, &oldIrql);
	
    return STATUS_SUCCESS;
}

NTSTATUS
DispatchPower (
    IN PDEVICE_OBJECT DeviceObject,
    IN PIRP Irp
    )
{
    NTSTATUS status;
    PDRIVER_DEVICE_EXTENSION extension = (PDRIVER_DEVICE_EXTENSION)(DeviceObject->DeviceExtension); 
    
    
    IoSetCompletionRoutine(Irp, CompletionRoutine, extension, TRUE, TRUE, TRUE);
    
    status = IoCallDriver(DeviceObject,Irp);
    return status;
}

NTSTATUS
DispatchSystemControl (
    IN  PDEVICE_OBJECT  DeviceObject,
    IN  PIRP            Irp
    )
{   
    /*
       This defect is injected for the "CancelSpinLock" rule.
    */
    KIRQL oldIrql;

    UNREFERENCED_PARAMETER(DeviceObject);
    UNREFERENCED_PARAMETER(Irp);
    
    IoAcquireCancelSpinLock(&oldIrql);
	
    return STATUS_SUCCESS;
}

NTSTATUS
DispatchPnp (
    IN PDEVICE_OBJECT DeviceObject,
    IN PIRP Irp
    )
{   
    /*
       This defect is injected for "LowerDriverReturn" rule.
    */
    NTSTATUS status = IoCallDriver(DeviceObject,Irp);

    status = STATUS_SUCCESS;

    return status;
}

NTSTATUS
CompletionRoutine(
    IN PDEVICE_OBJECT DeviceObject,
    IN PIRP Irp,
    IN PVOID EventIn
    )
{
    PKEVENT Event = (PKEVENT)EventIn;
    KIRQL oldIrql;
    PDRIVER_DEVICE_EXTENSION extension = (PDRIVER_DEVICE_EXTENSION)(DeviceObject->DeviceExtension); 
    UNREFERENCED_PARAMETER(Irp);
    KeRaiseIrql(DISPATCH_LEVEL, &oldIrql);

    /*
       This defect is injected for IrqlKeSetEvent rule
    */ 
    KeSetEvent(Event, extension->Increment, TRUE);
    return STATUS_SUCCESS;
}

BOOLEAN
InterruptServiceRoutine (
    IN PKINTERRUPT Interrupt,
    IN PVOID DeviceExtensionIn
    )
{     
    PDRIVER_DEVICE_EXTENSION DeviceExtension = (PDRIVER_DEVICE_EXTENSION)DeviceExtensionIn;
    PVOID Context = NULL;        
     
    UNREFERENCED_PARAMETER(Interrupt); 
    IoRequestDpc(DeviceExtension->DeviceObject, DeviceExtension->Irp, Context);
    
    return TRUE;
}

VOID
DpcForIsrRoutine(    
IN PKDPC  Dpc,    
IN struct _DEVICE_OBJECT  *DeviceObject,    
IN struct _IRP  *Irp,    
IN PVOID  Context)
{
    UNREFERENCED_PARAMETER(DeviceObject);
    UNREFERENCED_PARAMETER(Irp);
    UNREFERENCED_PARAMETER(Context);
    UNREFERENCED_PARAMETER(Dpc);
    /*
       This defect is injected for IrqlIoApcLte rule
    */ 
    IoGetInitialStack();
}

VOID
DriverUnload(
    IN PDRIVER_OBJECT DriverObject
    )
{
    UNREFERENCED_PARAMETER(DriverObject);
     
    return;
}
