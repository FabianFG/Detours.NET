using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DetoursNET;
using Vanara.PInvoke;

using var hook = new DetoursHook<Delegates.GetErrorModeDel>("kernel32.dll", "GetErrorMode", it =>
{
    return () =>
    {
        var result = it.OrigFuncDelegate();
        Console.WriteLine($"GetErrorMode: {result}");
        return 0xDEADDEAD;
    };
});

Console.WriteLine(IntPtr.Size);
Console.WriteLine("0x{0:X}", Kernel32.GetErrorMode());

static class Delegates
{
    // Can't use that if we wanna use the same delegate to call the original function,
    // it will be implicitly marked with the UnmanagedCallersOnly-attribute which will then make Marshal.GetDelegateForFunctionPointer fail
    //[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    public delegate uint GetErrorModeDel();
}