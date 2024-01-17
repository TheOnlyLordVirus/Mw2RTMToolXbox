using System;

namespace XDRPC;

// Token: 0x02000012 RID: 18
public class XDRPCInvalidTypeException : XDRPCException
{
    // Token: 0x060000AE RID: 174 RVA: 0x000045C3 File Offset: 0x000035C3
    public XDRPCInvalidTypeException(Type t) : this(t, string.Format("Invalid type {0}", t.Name))
    {
    }

    // Token: 0x060000AF RID: 175 RVA: 0x000045DC File Offset: 0x000035DC
    public XDRPCInvalidTypeException(Type t, string message) : base(message)
    {
        this.Type = t;
    }

    // Token: 0x17000014 RID: 20
    // (get) Token: 0x060000B0 RID: 176 RVA: 0x000045EC File Offset: 0x000035EC
    // (set) Token: 0x060000B1 RID: 177 RVA: 0x000045F4 File Offset: 0x000035F4
    public Type Type { get; private set; }
}
