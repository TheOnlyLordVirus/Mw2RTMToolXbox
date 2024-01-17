using System;

namespace XDRPC;

// Token: 0x02000011 RID: 17
public class XDRPCInvalidOperationException : XDRPCException
{
    // Token: 0x060000AC RID: 172 RVA: 0x000045AD File Offset: 0x000035AD
    public XDRPCInvalidOperationException() : this("Invalid operation")
    {
    }

    // Token: 0x060000AD RID: 173 RVA: 0x000045BA File Offset: 0x000035BA
    public XDRPCInvalidOperationException(string message) : base(message)
    {
    }
}
