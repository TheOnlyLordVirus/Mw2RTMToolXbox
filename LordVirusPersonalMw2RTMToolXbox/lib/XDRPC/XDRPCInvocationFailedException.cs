using System;

namespace XDRPC;

// Token: 0x02000018 RID: 24
public class XDRPCInvocationFailedException : XDRPCException
{
    // Token: 0x060000C5 RID: 197 RVA: 0x00004719 File Offset: 0x00003719
    public XDRPCInvocationFailedException(string message) : base(message)
    {
    }

    // Token: 0x060000C6 RID: 198 RVA: 0x00004722 File Offset: 0x00003722
    public XDRPCInvocationFailedException(string format, params object[] args) : base(string.Format(format, args))
    {
    }
}
