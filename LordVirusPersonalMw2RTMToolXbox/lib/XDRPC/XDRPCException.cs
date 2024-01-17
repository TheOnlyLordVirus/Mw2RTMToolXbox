using System;
using System.Text;

namespace XDRPC;

// Token: 0x02000010 RID: 16
public class XDRPCException : Exception
{
    // Token: 0x060000A9 RID: 169 RVA: 0x00004592 File Offset: 0x00003592
    public XDRPCException()
    {
    }

    // Token: 0x060000AA RID: 170 RVA: 0x0000459A File Offset: 0x0000359A
    public XDRPCException(string message) : base(message)
    {
    }

    // Token: 0x060000AB RID: 171 RVA: 0x000045A3 File Offset: 0x000035A3
    public XDRPCException(string message, Exception innerException) : base(message, innerException)
    {
    }
}