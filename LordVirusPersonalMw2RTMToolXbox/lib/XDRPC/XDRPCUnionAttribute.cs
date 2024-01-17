using System;

namespace XDRPC;

// Token: 0x02000006 RID: 6
[AttributeUsage(AttributeTargets.Field)]
public class XDRPCUnionAttribute : Attribute
{
    // Token: 0x0600000D RID: 13 RVA: 0x00002495 File Offset: 0x00001495
    public XDRPCUnionAttribute(string memberToWrite)
    {
        this.Value = memberToWrite;
    }

    // Token: 0x17000001 RID: 1
    // (get) Token: 0x0600000E RID: 14 RVA: 0x000024A4 File Offset: 0x000014A4
    // (set) Token: 0x0600000F RID: 15 RVA: 0x000024AC File Offset: 0x000014AC
    public string Value { get; private set; }
}
