using System;

namespace XDRPC;

// Token: 0x02000014 RID: 20
public class XDRPCInvalidArgumentTypeException : XDRPCException
{
    // Token: 0x060000B5 RID: 181 RVA: 0x0000462D File Offset: 0x0000362D
    public XDRPCInvalidArgumentTypeException(Type t, int idx) : base(string.Format("Type ({0}) for argument {1} is not valid", t.Name, idx))
    {
        this.ArgumentType = t;
        this.ArgumentIndex = idx;
    }

    // Token: 0x17000016 RID: 22
    // (get) Token: 0x060000B6 RID: 182 RVA: 0x00004659 File Offset: 0x00003659
    // (set) Token: 0x060000B7 RID: 183 RVA: 0x00004661 File Offset: 0x00003661
    public Type ArgumentType { get; private set; }

    // Token: 0x17000017 RID: 23
    // (get) Token: 0x060000B8 RID: 184 RVA: 0x0000466A File Offset: 0x0000366A
    // (set) Token: 0x060000B9 RID: 185 RVA: 0x00004672 File Offset: 0x00003672
    public int ArgumentIndex { get; private set; }
}