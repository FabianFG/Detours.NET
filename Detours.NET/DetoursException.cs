using System;

namespace DetoursNET;

public enum DetoursAction
{
    TransactionBegin,
    UpdateThread,
    Attach,
    Detach,
    TransactionCommit
}
    
public class DetoursException : Exception
{
    public DetoursException(string message) : base(message)
    {
    }
        
    public DetoursException(DetoursAction action, int code) : base($"Detours failed with code {code} at action {action}")
    {
    }
}