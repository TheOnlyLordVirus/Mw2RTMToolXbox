using System;

namespace XDRPC;

// Token: 0x02000017 RID: 23
public class XDRPCInvalidResponseException : XDRPCException
{
    // Token: 0x060000C2 RID: 194 RVA: 0x000046EE File Offset: 0x000036EE
    public XDRPCInvalidResponseException(string response) : base(string.Format("Invalid response received from console: {0}", response))
    {
        this.Response = response;
    }

    // Token: 0x1700001B RID: 27
    // (get) Token: 0x060000C3 RID: 195 RVA: 0x00004708 File Offset: 0x00003708
    // (set) Token: 0x060000C4 RID: 196 RVA: 0x00004710 File Offset: 0x00003710
    public string Response { get; private set; }
}
