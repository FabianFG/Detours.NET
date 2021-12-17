using System;

namespace DetoursNET;

public interface IDetoursHook : IDisposable
{
    public unsafe void* OriginalPtr { get; }
    public unsafe void* OriginalFuncPtr { get; }
    public unsafe void* DetourPtr { get; }
}