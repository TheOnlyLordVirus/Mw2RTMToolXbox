using System;

namespace XDRPC;

// Token: 0x02000015 RID: 21
public class XDRPCTooManyArgumentsException : XDRPCException
{
    // Token: 0x060000BA RID: 186 RVA: 0x0000467B File Offset: 0x0000367B
    public XDRPCTooManyArgumentsException(int count, int supported) : base(string.Format("{0} arguments specified when only {1} are supported. Note: If the return type is a struct then that takes up one argument slot. If one (or more) of the arguments is a struct being passed by value, it can count as more than one argument based on the size of the struct.", count, supported))
    {
        this.ArgumentCount = count;
        this.MaxArgumentsSupported = supported;
    }

    // Token: 0x17000018 RID: 24
    // (get) Token: 0x060000BB RID: 187 RVA: 0x000046A7 File Offset: 0x000036A7
    // (set) Token: 0x060000BC RID: 188 RVA: 0x000046AF File Offset: 0x000036AF
    public int ArgumentCount { get; private set; }

    // Token: 0x17000019 RID: 25
    // (get) Token: 0x060000BD RID: 189 RVA: 0x000046B8 File Offset: 0x000036B8
    // (set) Token: 0x060000BE RID: 190 RVA: 0x000046C0 File Offset: 0x000036C0
    public int MaxArgumentsSupported { get; private set; }
}
