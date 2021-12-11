using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DetoursNET
{
    public class DetoursHook<TDelegate> : IDetoursHook where TDelegate : Delegate
    {
        public unsafe void* OriginalPtr { get; private set; }
        public unsafe void* OriginalFuncPtr { get; private set; }
        public unsafe void* DetourPtr { get; private set; }

        public TDelegate OrigFuncDelegate { get; private set; }
        public TDelegate DetourFuncDelegate { get; private set; }
        
        public DetoursHook(string moduleName, string funcName, Func<DetoursHook<TDelegate>, TDelegate> delegateGetter,
            bool createOrigFuncDelegate = true) : this(WinApi.GetProcAddress(WinApi.GetModuleHandle(moduleName), funcName), delegateGetter, createOrigFuncDelegate)
        {
            
        } 

        public DetoursHook(string moduleName, string funcName, TDelegate targetFuncDelegate,
            bool createOrigFuncDelegate = true) : this(WinApi.GetProcAddress(WinApi.GetModuleHandle(moduleName), funcName), targetFuncDelegate, createOrigFuncDelegate)
        {
            
        }
        
        public DetoursHook(IntPtr originalPtr, Func<DetoursHook<TDelegate>, TDelegate> delegateGetter,
            bool createOrigFuncDelegate = true)
        {
            unsafe
            {
                DetourFuncDelegate = delegateGetter(this);
                DetourPtr = Marshal.GetFunctionPointerForDelegate(DetourFuncDelegate).ToPointer();
            
                Hook(originalPtr.ToPointer(), DetourPtr, createOrigFuncDelegate);
            }
        } 

        public DetoursHook(IntPtr originalPtr, TDelegate targetFuncDelegate, bool createOrigFuncDelegate = true)
        {
            unsafe
            {
                DetourFuncDelegate = targetFuncDelegate;
                DetourPtr = Marshal.GetFunctionPointerForDelegate(targetFuncDelegate).ToPointer();
            
                Hook(originalPtr.ToPointer(), DetourPtr, createOrigFuncDelegate);
            }
        }
        
        public unsafe DetoursHook(void* originalPtr, Func<DetoursHook<TDelegate>, TDelegate> delegateGetter,
            bool createOrigFuncDelegate = true)
        {
            DetourFuncDelegate = delegateGetter(this);
            DetourPtr = Marshal.GetFunctionPointerForDelegate(DetourFuncDelegate).ToPointer();
            
            Hook(originalPtr, DetourPtr, createOrigFuncDelegate);
        } 

        public unsafe DetoursHook(void* originalPtr, TDelegate targetFuncDelegate, bool createOrigFuncDelegate = true)
        {
            DetourFuncDelegate = targetFuncDelegate;
            DetourPtr = Marshal.GetFunctionPointerForDelegate(targetFuncDelegate).ToPointer();
            
            Hook(originalPtr, DetourPtr, createOrigFuncDelegate);
        }
        
        public DetoursHook(string moduleName, string funcName, IntPtr detourPtr,
            bool createOrigFuncDelegate = true) : this(WinApi.GetProcAddress(WinApi.GetModuleHandle(moduleName), funcName), detourPtr, createOrigFuncDelegate)
        {
            
        }

        public DetoursHook(IntPtr originalPtr, IntPtr detourPtr, bool createOrigFuncDelegate = true)
        {
            unsafe
            {
                Hook(originalPtr.ToPointer(), detourPtr.ToPointer(), createOrigFuncDelegate);
            }
        }
        
        public unsafe DetoursHook(string moduleName, string funcName, void* detourPtr,
            bool createOrigFuncDelegate = true) : this(WinApi.GetProcAddress(WinApi.GetModuleHandle(moduleName), funcName).ToPointer(), detourPtr, createOrigFuncDelegate)
        {
            
        }

        public unsafe DetoursHook(void* originalPtr, void* detourPtr, bool createOrigFuncDelegate = true)
        {
            Hook(originalPtr, detourPtr, createOrigFuncDelegate);
        }

        private unsafe void Hook(void* originalPtr, void* detourPtr, bool createOrigFuncDelegate)
        {
            OriginalPtr = originalPtr;
            OriginalFuncPtr = originalPtr;
            DetourPtr = detourPtr;

            var transactionBegin = Detours.DetourTransactionBegin();
            if (transactionBegin != 0)
            {
                Detours.DetourTransactionAbort();
                throw new DetoursException(DetoursAction.TransactionBegin, transactionBegin);    
            }

            var updateThread = Detours.DetourUpdateThread(WinApi.GetCurrentThread());
            if (updateThread != 0)
            {
                Detours.DetourTransactionAbort();
                throw new DetoursException(DetoursAction.UpdateThread, updateThread);
            }

            var originalFuncPtr = OriginalFuncPtr;
            
            var attach = Detours.DetourAttach(&originalFuncPtr, DetourPtr);
            if (attach != 0)
            {
                Detours.DetourTransactionAbort();
                throw new DetoursException(DetoursAction.Attach, attach);
            }
            
            var commit = Detours.DetourTransactionCommit();
            if (commit != 0)
            {
                Detours.DetourTransactionAbort();
                throw new DetoursException(DetoursAction.TransactionCommit, commit);
            }
            
            OriginalFuncPtr = originalFuncPtr;
            if (createOrigFuncDelegate)
            {
                OrigFuncDelegate = Marshal.GetDelegateForFunctionPointer<TDelegate>(new IntPtr(OriginalFuncPtr));
            }
        }

        public void Dispose()
        {
            try
            {
                unsafe
                {
                    Detours.DetourTransactionBegin(); 
                    Detours.DetourUpdateThread(WinApi.GetCurrentThread());
                    var origFuncPtr = OriginalFuncPtr;
                    Detours.DetourDetach(&origFuncPtr, DetourPtr);
                    Detours.DetourTransactionCommit();
                }
            }
            catch (Exception)
            {
                Detours.DetourTransactionAbort();
                throw;
            }
            finally
            {
                unsafe
                {
                    OrigFuncDelegate = default(TDelegate);
                    OriginalFuncPtr = OriginalPtr;
                }
            }
        }
        
    }
    
    internal static class WinApi
    {
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetCurrentThread();
        
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle([Optional] string lpModuleName);
    }
}
