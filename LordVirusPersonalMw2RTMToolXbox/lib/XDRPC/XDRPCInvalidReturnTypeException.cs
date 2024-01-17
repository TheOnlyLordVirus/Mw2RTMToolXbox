using System;

namespace XDRPC;

// Token: 0x02000013 RID: 19
public class XDRPCInvalidReturnTypeException : XDRPCException
{
    // Token: 0x060000B2 RID: 178 RVA: 0x000045FD File Offset: 0x000035FD
    public XDRPCInvalidReturnTypeException(Type t) : base(string.Format("Invalid return type {0}", t.Name))
    {
        this.ReturnType = t;
    }

    // Token: 0x17000015 RID: 21
    // (get) Token: 0x060000B3 RID: 179 RVA: 0x0000461C File Offset: 0x0000361C
    // (set) Token: 0x060000B4 RID: 180 RVA: 0x00004624 File Offset: 0x00003624
    public Type ReturnType { get; private set; }
}
