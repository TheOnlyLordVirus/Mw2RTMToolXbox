using System;

namespace XDRPC;

// Token: 0x02000019 RID: 25
public class XDRPCModuleNotFoundException : XDRPCException
{
    // Token: 0x060000C7 RID: 199 RVA: 0x00004731 File Offset: 0x00003731
    public XDRPCModuleNotFoundException(string moduleName) : base(string.Format("Module not found: {0}", moduleName))
    {
        this.ModuleName = moduleName;
    }

    // Token: 0x1700001C RID: 28
    // (get) Token: 0x060000C8 RID: 200 RVA: 0x0000474B File Offset: 0x0000374B
    // (set) Token: 0x060000C9 RID: 201 RVA: 0x00004753 File Offset: 0x00003753
    public string ModuleName { get; private set; }
}
