using System;

namespace XDRPC;

// Token: 0x02000016 RID: 22
public class XDRPCNotSupportedException : XDRPCException
{
    // Token: 0x060000BF RID: 191 RVA: 0x000046C9 File Offset: 0x000036C9
    public XDRPCNotSupportedException(string response) : base("XDRPC not supported by console")
    {
        this.Response = response;
    }

    // Token: 0x1700001A RID: 26
    // (get) Token: 0x060000C0 RID: 192 RVA: 0x000046DD File Offset: 0x000036DD
    // (set) Token: 0x060000C1 RID: 193 RVA: 0x000046E5 File Offset: 0x000036E5
    public string Response { get; private set; }
}
